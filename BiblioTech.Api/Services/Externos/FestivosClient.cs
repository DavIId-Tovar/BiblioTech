using System.Net.Http.Json;
using System.Text.Json;
using BiblioTech.Api.Configuracion;
using BiblioTech.Api.DTOs.Calendario;
using BiblioTech.Api.DTOs.Externos;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BiblioTech.Api.Services.Externos;

/// <summary>
/// CONSUMO DE UNA API EXTERNA (Nager.Date). Es un "cliente tipado":
///  · El HttpClient lo crea IHttpClientFactory (registrado en Program.cs con AddHttpClient),
///    ya configurado con BaseAddress y Timeout. Nunca se hace "new HttpClient()".
///  · Los festivos casi no cambian: se guardan en memoria (IMemoryCache) para no llamar
///    a la API externa en cada petición.
///  · Si la API externa falla (sin internet, timeout, JSON raro), se registra en el log
///    y se responde "no disponible" en vez de romper el calendario.
/// </summary>
public class FestivosClient(
    HttpClient http,
    IMemoryCache cache,
    IOptions<FestivosApiOpciones> opciones,
    ILogger<FestivosClient> log) : IFestivosClient
{
    private readonly FestivosApiOpciones config = opciones.Value;

    public async Task<(bool Disponible, List<FestivoDto> Festivos)> ObtenerAsync(int anio)
    {
        var clave = $"festivos-{config.CodigoPais}-{anio}";

        // 1. ¿Ya los tenemos en memoria?
        if (cache.TryGetValue(clave, out List<FestivoDto>? enCache) && enCache is not null)
            return (true, enCache);

        try
        {
            // 2. GET https://date.nager.at/api/v3/PublicHolidays/{anio}/CO
            //    GetFromJsonAsync = petición GET + verificar 2xx + convertir el JSON a objetos C#
            var respuesta = await http.GetFromJsonAsync<List<NagerFestivoRespuesta>>(
                $"PublicHolidays/{anio}/{config.CodigoPais}") ?? [];

            // 3. Traducir SU formato al NUESTRO (si la API externa cambia, solo se toca aquí)
            var festivos = respuesta
                .Select(f => new FestivoDto { Fecha = f.Date, Nombre = f.LocalName })
                .OrderBy(f => f.Fecha)
                .ToList();

            // 4. Guardar en memoria por unas horas
            cache.Set(clave, festivos, TimeSpan.FromHours(config.HorasCache));
            return (true, festivos);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // HttpRequestException: sin conexión o respuesta 4xx/5xx
            // TaskCanceledException: se superó el Timeout
            // JsonException: la respuesta no tenía la forma esperada
            log.LogWarning(ex, "No se pudieron obtener los festivos {Anio} de la API externa", anio);
            return (false, []);
        }
    }
}
