using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Web.Client.Models;

public class SocioDto
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
    public int PrestamosActivos { get; set; }
}

public class SocioGuardarDto
{
    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El documento admite máximo 20 caracteres.")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre admite máximo 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo admite máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
