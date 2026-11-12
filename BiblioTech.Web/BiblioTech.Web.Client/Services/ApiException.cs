using System.Net.Http.Json;
using System.Text.Json;
using BiblioTech.Web.Client.Models;

namespace BiblioTech.Web.Client.Services;

/// <summary>Error que devolvió la API, ya traducido a un mensaje para mostrar en pantalla.</summary>
public class ApiException(string mensaje, int codigo) : Exception(mensaje)
{
    public int Codigo { get; } = codigo;

    public static ApiException SinConexion() =>
        new("No se pudo conectar con la API. Verifica que BiblioTech.Api esté corriendo.", 0);

    /// <summary>Lee el ProblemDetails (400, 404, 409, 500) y arma el mensaje.</summary>
    public static async Task<ApiException> DesdeRespuestaAsync(HttpResponseMessage respuesta)
    {
        var codigo = (int)respuesta.StatusCode;
        ProblemaApi? problema = null;

        try
        {
            problema = await respuesta.Content.ReadFromJsonAsync<ProblemaApi>();
        }
        catch (JsonException)
        {
            // la respuesta no era JSON: se usa el mensaje genérico
        }

        // 400 de validación: trae un diccionario campo → mensajes
        if (problema?.Errors is { Count: > 0 } errores)
            return new ApiException(string.Join(" ", errores.SelectMany(e => e.Value)), codigo);

        return new ApiException(problema?.Detail ?? problema?.Title ?? $"Error {codigo} al llamar a la API.", codigo);
    }
}
