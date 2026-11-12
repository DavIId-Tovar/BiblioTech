using System.Net.Http.Json;
using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;

namespace BiblioTech.Web.Client.Services;

/// <summary>
/// No hereda de ApiServiceBase a propósito: aquí un 401 significa
/// "contraseña incorrecta", no "sesión vencida", así que no redirige.
/// </summary>
public class AuthApiService(IHttpClientFactory fabrica, JwtAuthStateProvider sesion)
{
    public async Task IniciarSesionAsync(LoginDto dto)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await fabrica.CreateClient(ApiServiceBase.NombreCliente).PostAsJsonAsync("api/auth/login", dto);
        }
        catch (HttpRequestException)
        {
            throw ApiException.SinConexion();
        }

        if (!respuesta.IsSuccessStatusCode)
            throw await ApiException.DesdeRespuestaAsync(respuesta);

        var datos = await respuesta.Content.ReadFromJsonAsync<LoginRespuestaDto>();
        await sesion.IniciarSesionAsync(datos!.Token);
    }

    public Task CerrarSesionAsync() => sesion.CerrarSesionAsync();
}
