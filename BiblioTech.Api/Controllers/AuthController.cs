using BiblioTech.Api.DTOs.Auth;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]   // es la puerta de entrada: no puede exigir token
public class AuthController(IAuthService servicio) : ControllerBase
{
    /// <summary>POST api/auth/login → 200 con el token, o 401.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
    {
        var respuesta = await servicio.LoginAsync(dto);

        return respuesta is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized,
                      title: "No autorizado",
                      detail: "Correo o contraseña incorrectos.")
            : Ok(respuesta);
    }

    /// <summary>
    /// POST api/auth/registrar-inicial → crea el primer bibliotecario (día 3).
    /// Solo funciona mientras la tabla Usuarios esté vacía; después responde 409.
    /// </summary>
    [HttpPost("registrar-inicial")]
    public async Task<IActionResult> RegistrarInicial(RegistrarUsuarioDto dto)
    {
        await servicio.RegistrarInicialAsync(dto);
        return StatusCode(StatusCodes.Status201Created);
    }
}
