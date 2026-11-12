using System.ComponentModel.DataAnnotations;
using BiblioTech.Api.DTOs.Calendario;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

/// <summary>Calendario de entregas + festivos de Colombia (API externa) + alertas.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarioController(ICalendarioService servicio) : ControllerBase
{
    /// <summary>Entregas pendientes y festivos de un mes. Ej: api/calendario?anio=2026&amp;mes=11</summary>
    [HttpGet]
    public async Task<ActionResult<CalendarioMesDto>> ObtenerMes(
        [FromQuery, Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100.")] int anio,
        [FromQuery, Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")] int mes)
        => Ok(await servicio.ObtenerMesAsync(anio, mes));

    /// <summary>Préstamos en mora y próximos a entregar (para la campana de notificaciones).</summary>
    [HttpGet("alertas")]
    public async Task<ActionResult<AlertasDto>> ObtenerAlertas()
        => Ok(await servicio.ObtenerAlertasAsync());

    /// <summary>Festivos del año tal como llegan de la API externa (ya traducidos a nuestro formato).</summary>
    [HttpGet("festivos/{anio:int}")]
    public async Task<ActionResult<FestivosAnioDto>> ObtenerFestivos([Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100.")] int anio)
        => Ok(await servicio.ObtenerFestivosAsync(anio));
}
