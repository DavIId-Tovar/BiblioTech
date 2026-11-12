using BiblioTech.Api.DTOs.Socios;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SociosController(ISocioService servicio) : ControllerBase
{
    /// <summary>GET api/socios?soloActivos=true (para el combo de "Nuevo préstamo")</summary>
    [HttpGet]
    public async Task<ActionResult<List<SocioDto>>> Listar([FromQuery] bool soloActivos = false)
        => Ok(await servicio.ListarAsync(soloActivos));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SocioDto>> Obtener(int id)
        => Ok(await servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<SocioDto>> Crear(SocioGuardarDto dto)
    {
        var creado = await servicio.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, SocioGuardarDto dto)
    {
        await servicio.ActualizarAsync(id, dto);
        return NoContent();
    }

    /// <summary>DELETE = desactivar (borrado lógico)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        await servicio.DesactivarAsync(id);
        return NoContent();
    }
}
