using BiblioTech.Api.DTOs.Autores;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

/// <summary>
/// El Controller solo recibe la petición, llama al servicio y devuelve el código HTTP.
/// [ApiController] valida el DTO (400 si falla). Los 404/409 los produce el middleware.
/// </summary>
[ApiController]
[Route("api/[controller]")]   // => api/autores
[Authorize]                   // sin token válido → 401
public class AutoresController(IAutorService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AutorDto>>> Listar()
        => Ok(await servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AutorDto>> Obtener(int id)
        => Ok(await servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<AutorDto>> Crear(AutorGuardarDto dto)
    {
        var creado = await servicio.CrearAsync(dto);
        // 201 Created + cabecera Location con la URL del nuevo recurso
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, AutorGuardarDto dto)
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
