using BiblioTech.Api.DTOs.Auth;

namespace BiblioTech.Api.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Devuelve el token, o null si el correo o la contraseña no coinciden.</summary>
    Task<LoginRespuestaDto?> LoginAsync(LoginDto dto);

    /// <summary>Crea el primer bibliotecario. Falla (409) si ya existe algún usuario.</summary>
    Task RegistrarInicialAsync(RegistrarUsuarioDto dto);
}
