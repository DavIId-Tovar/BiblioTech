using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BiblioTech.Web.Client.Services;

public class LibroApiService(IHttpClientFactory f, JwtAuthStateProvider s, NavigationManager n) : ApiServiceBase(f, s, n)
{
    public Task<List<LibroDto>> ListarAsync(string? buscar = null) =>
        GetAsync<List<LibroDto>>(string.IsNullOrWhiteSpace(buscar)
            ? "api/libros"
            : $"api/libros?buscar={Uri.EscapeDataString(buscar.Trim())}");

    public Task<LibroDto> ObtenerAsync(int id) => GetAsync<LibroDto>($"api/libros/{id}");
    public Task<LibroDto> CrearAsync(LibroGuardarDto dto) => PostAsync<LibroDto>("api/libros", dto);
    public Task ActualizarAsync(int id, LibroGuardarDto dto) => PutAsync($"api/libros/{id}", dto);
    public Task EliminarAsync(int id) => DeleteAsync($"api/libros/{id}");
}
