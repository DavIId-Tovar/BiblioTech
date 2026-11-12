using System.Net;
using System.Net.Http.Json;
using BiblioTech.Web.Client.Auth;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

/// <summary>
/// Base de todos los servicios HTTP. Centraliza tres cosas para no repetirlas:
///  1. Crear el HttpClient "Api" (que ya trae el token).
///  2. Si la API responde con error, convertir su ProblemDetails en un ApiException con mensaje claro.
///  3. Si responde 401 (token vencido o inválido), cerrar sesión y mandar al login.
/// </summary>
public abstract class ApiServiceBase(IHttpClientFactory fabrica, JwtAuthStateProvider sesion, NavigationManager nav)
{
    public const string NombreCliente = "Api";

    protected async Task<T> GetAsync<T>(string url)
    {
        var respuesta = await EnviarAsync(http => http.GetAsync(url));
        return (await respuesta.Content.ReadFromJsonAsync<T>())!;
    }

    protected async Task<T> PostAsync<T>(string url, object datos)
    {
        var respuesta = await EnviarAsync(http => http.PostAsJsonAsync(url, datos));
        return (await respuesta.Content.ReadFromJsonAsync<T>())!;
    }

    protected Task PutAsync(string url, object? datos = null)
        => EnviarAsync(http => datos is null ? http.PutAsync(url, null) : http.PutAsJsonAsync(url, datos));

    protected Task DeleteAsync(string url)
        => EnviarAsync(http => http.DeleteAsync(url));

    private async Task<HttpResponseMessage> EnviarAsync(Func<HttpClient, Task<HttpResponseMessage>> peticion)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await peticion(fabrica.CreateClient(NombreCliente));
        }
        catch (HttpRequestException)
        {
            throw ApiException.SinConexion();
        }

        if (respuesta.IsSuccessStatusCode) return respuesta;

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
        {
            await sesion.CerrarSesionAsync();
            nav.NavigateTo("login");
            throw new ApiException("Tu sesión expiró. Inicia sesión de nuevo.", 401);
        }

        throw await ApiException.DesdeRespuestaAsync(respuesta);
    }
}
