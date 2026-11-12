using System.Text;
using BiblioTech.Api.Configuracion;
using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Auth;
using BiblioTech.Api.Exceptions;
using BiblioTech.Api.Models;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BiblioTech.Api.Services;

public class AuthService(BibliotecaContext db, IOptions<JwtOpciones> opciones) : IAuthService
{
    private readonly JwtOpciones jwt = opciones.Value;

    public async Task<LoginRespuestaDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        // BCrypt.Verify vuelve a calcular el hash con la sal guardada y compara.
        // Mismo mensaje si no existe el correo o si la contraseña falla: no damos pistas.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return null;

        var expira = DateTime.UtcNow.AddMinutes(jwt.ExpiraMinutos);

        return new LoginRespuestaDto
        {
            Token = GenerarToken(usuario, expira),
            Email = usuario.Email,
            Rol = usuario.Rol,
            ExpiraUtc = expira
        };
    }

    public async Task RegistrarInicialAsync(RegistrarUsuarioDto dto)
    {
        if (await db.Usuarios.AnyAsync())
            throw new ReglaNegocioException(
                "Ya existe un bibliotecario. Este endpoint solo sirve para crear el primer usuario.");

        db.Usuarios.Add(new Usuario
        {
            Email = dto.Email.Trim().ToLowerInvariant(),
            // Nunca se guarda la contraseña: solo su hash (incluye la sal y el costo)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = "Bibliotecario"
        });

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// El token es un JSON firmado con la clave secreta: cualquiera puede LEERLO,
    /// pero nadie puede MODIFICARLO sin invalidar la firma.
    /// </summary>
    private string GenerarToken(Usuario usuario, DateTime expira)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expira,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = usuario.Email,
                ["role"] = usuario.Rol
            },
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
