using BiblioTech.Api.DTOs.Autores;

namespace BiblioTech.Api.Services.Interfaces;

public interface IAutorService
{
    Task<List<AutorDto>> ListarAsync();
    Task<AutorDto?> ObtenerAsync(int id);
    Task<AutorDto> CrearAsync(AutorGuardarDto dto);
    Task<bool> ActualizarAsync(int id, AutorGuardarDto dto);
    Task<bool> EliminarAsync(int id);
}
