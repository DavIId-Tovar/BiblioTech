using BiblioTech.Api.DTOs.Prestamos;

namespace BiblioTech.Api.Services.Interfaces;

public interface IPrestamoService
{
    Task<List<PrestamoDto>> ListarAsync(string? estado);
    Task<PrestamoDto> ObtenerAsync(int id);
    Task<PrestamoDto> CrearAsync(PrestamoCrearDto dto);
    Task DevolverAsync(int id);
    Task<List<PrestamoVencidoDto>> ListarVencidosAsync();
}
