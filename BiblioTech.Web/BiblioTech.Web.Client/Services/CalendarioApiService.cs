using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

// Blazor NO llama a la API de festivos: llama a NUESTRA API, y es la API la que
// consulta la externa. Así la URL externa, la caché y los errores quedan en el servidor.
public class CalendarioApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<CalendarioMesDto> ObtenerMesAsync(int anio, int mes)
        => GetAsync<CalendarioMesDto>($"api/calendario?anio={anio}&mes={mes}");

    public Task<AlertasDto> ObtenerAlertasAsync()
        => GetAsync<AlertasDto>("api/calendario/alertas");
}
