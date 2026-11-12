using BiblioTech.Api.DTOs.Libros;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibrosController(ILibroService servicio) : ControllerBase
{
    /// <summary>GET api/libros?buscar=garcia</summary>
    [HttpGet]
    public async Task<ActionResult<List<LibroDto>>> Listar([FromQuery] string? buscar)
        => Ok(await servicio.ListarAsync(buscar));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibroDto>> Obtener(int id)
        => Ok(await servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<LibroDto>> Crear(LibroGuardarDto dto)
    {
        var creado = await servicio.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, LibroGuardarDto dto)
    {
        await servicio.ActualizarAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
