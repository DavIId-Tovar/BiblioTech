using BiblioTech.Api.DTOs.Calendario;

namespace BiblioTech.Api.Services.Interfaces;

/// <summary>Cliente de la API externa de festivos.</summary>
public interface IFestivosClient
{
    /// <summary>
    /// Festivos del año. Si la API externa falla, devuelve Disponible = false y lista vacía
    /// (nunca lanza excepción: un servicio externo caído no debe tumbar BiblioTech).
    /// </summary>
    Task<(bool Disponible, List<FestivoDto> Festivos)> ObtenerAsync(int anio);
}
