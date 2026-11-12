using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Api.DTOs.Socios;

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

    /// <summary>Solo se usa al editar (permite reactivar un socio).</summary>
    public bool Activo { get; set; } = true;
}
