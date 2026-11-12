using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

public class AutorApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<List<AutorDto>> ListarAsync() => GetAsync<List<AutorDto>>("api/autores");
    public Task<AutorDto> ObtenerAsync(int id) => GetAsync<AutorDto>($"api/autores/{id}");
    public Task<AutorDto> CrearAsync(AutorGuardarDto dto) => PostAsync<AutorDto>("api/autores", dto);
    public Task ActualizarAsync(int id, AutorGuardarDto dto) => PutAsync($"api/autores/{id}", dto);
    public Task EliminarAsync(int id) => DeleteAsync($"api/autores/{id}");
}
