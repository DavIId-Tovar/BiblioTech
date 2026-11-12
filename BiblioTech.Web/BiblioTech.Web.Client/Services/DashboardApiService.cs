using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

// Cada servicio "traduce" los endpoints de la API a métodos C#:
// la página llama a ObtenerAsync() y no sabe nada de URLs ni de JSON.
public class DashboardApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<DashboardDto> ObtenerAsync() => GetAsync<DashboardDto>("api/dashboard");
}
