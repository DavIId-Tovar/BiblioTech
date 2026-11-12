using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Api.DTOs.Autores;

/// <summary>Lo que la API RECIBE para crear o editar un autor (entrada).</summary>
public class AutorGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "La nacionalidad admite máximo 60 caracteres.")]
    public string? Nacionalidad { get; set; }
}
