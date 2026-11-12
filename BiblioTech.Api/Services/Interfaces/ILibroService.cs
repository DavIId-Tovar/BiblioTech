using BiblioTech.Api.DTOs.Libros;

namespace BiblioTech.Api.Services.Interfaces;

public interface ILibroService
{
    Task<List<LibroDto>> ListarAsync(string? buscar);
    Task<LibroDto> ObtenerAsync(int id);
    Task<LibroDto> CrearAsync(LibroGuardarDto dto);
    Task ActualizarAsync(int id, LibroGuardarDto dto);
    Task EliminarAsync(int id);
}
