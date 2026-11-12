using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

public class PrestamoApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<List<PrestamoDto>> ListarAsync(string? estado = null) =>
        GetAsync<List<PrestamoDto>>(string.IsNullOrEmpty(estado) ? "api/prestamos" : $"api/prestamos?estado={estado}");

    public Task<PrestamoDto> CrearAsync(PrestamoCrearDto dto) => PostAsync<PrestamoDto>("api/prestamos", dto);
    public Task DevolverAsync(int id) => PutAsync($"api/prestamos/{id}/devolver");
    public Task<List<PrestamoVencidoDto>> ListarVencidosAsync() => GetAsync<List<PrestamoVencidoDto>>("api/prestamos/vencidos");
}
