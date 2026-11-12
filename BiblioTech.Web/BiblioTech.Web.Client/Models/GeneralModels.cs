using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Web.Client.Models;

public class DashboardDto
{
    public int TotalLibros { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public int PrestamosActivos { get; set; }
    public int PrestamosVencidos { get; set; }
    public int SociosActivos { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime ExpiraUtc { get; set; }
}

/// <summary>Formato ProblemDetails con el que la API responde los errores.</summary>
public class ProblemaApi
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
