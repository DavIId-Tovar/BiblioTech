using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

public class SocioApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<List<SocioDto>> ListarAsync(bool soloActivos = false) =>
        GetAsync<List<SocioDto>>($"api/socios?soloActivos={soloActivos}");

    public Task<SocioDto> ObtenerAsync(int id) => GetAsync<SocioDto>($"api/socios/{id}");
    public Task<SocioDto> CrearAsync(SocioGuardarDto dto) => PostAsync<SocioDto>("api/socios", dto);
    public Task ActualizarAsync(int id, SocioGuardarDto dto) => PutAsync($"api/socios/{id}", dto);
    public Task DesactivarAsync(int id) => DeleteAsync($"api/socios/{id}");
}
