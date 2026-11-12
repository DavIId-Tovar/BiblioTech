using BiblioTech.Api.DTOs.Socios;

namespace BiblioTech.Api.Services.Interfaces;

public interface ISocioService
{
    Task<List<SocioDto>> ListarAsync(bool soloActivos);
    Task<SocioDto> ObtenerAsync(int id);
    Task<SocioDto> CrearAsync(SocioGuardarDto dto);
    Task ActualizarAsync(int id, SocioGuardarDto dto);
    Task DesactivarAsync(int id);
}
