using BiblioTech.Api.Constants;
using BiblioTech.Api.DTOs.Prestamos;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrestamosController(IPrestamoService servicio) : ControllerBase
{
    private static readonly string[] EstadosValidos =
        [EstadosPrestamo.Activo, EstadosPrestamo.Devuelto, EstadosPrestamo.Vencido];

    /// <summary>GET api/prestamos?estado=Activo|Devuelto|Vencido</summary>
    [HttpGet]
    public async Task<ActionResult<List<PrestamoDto>>> Listar([FromQuery] string? estado)
    {
        if (!string.IsNullOrWhiteSpace(estado) && !EstadosValidos.Contains(estado))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                           title: "Filtro no válido",
                           detail: "El estado debe ser Activo, Devuelto o Vencido.");

        return Ok(await servicio.ListarAsync(estado));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrestamoDto>> Obtener(int id)
        => Ok(await servicio.ObtenerAsync(id));

    /// <summary>Registra un préstamo aplicando las reglas R1–R6 (409 si alguna falla).</summary>
    [HttpPost]
    public async Task<ActionResult<PrestamoDto>> Crear(PrestamoCrearDto dto)
    {
        var creado = await servicio.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}/devolver")]
    public async Task<IActionResult> Devolver(int id)
    {
        await servicio.DevolverAsync(id);
        return NoContent();
    }

    /// <summary>Reporte desde la vista vw_PrestamosVencidos.</summary>
    [HttpGet("vencidos")]
    public async Task<ActionResult<List<PrestamoVencidoDto>>> Vencidos()
        => Ok(await servicio.ListarVencidosAsync());
}
