using BiblioTech.Api.DTOs.Calendario;

namespace BiblioTech.Api.Services.Interfaces;

public interface ICalendarioService
{
    Task<FestivosAnioDto> ObtenerFestivosAsync(int anio);
    Task<CalendarioMesDto> ObtenerMesAsync(int anio, int mes);
    Task<AlertasDto> ObtenerAlertasAsync();
}
