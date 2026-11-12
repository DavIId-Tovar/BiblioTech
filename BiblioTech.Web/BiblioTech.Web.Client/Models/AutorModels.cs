using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Web.Client.Models;

// Copia de los DTOs de la API. Si se agrega una propiedad allá y se olvida aquí,
// compila igual pero el dato no llega (por eso se menciona el proyecto Shared como mejora).

public class AutorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public int CantidadLibros { get; set; }
}

/// <summary>Las mismas validaciones de la API: el front avisa rápido, la API vuelve a validar.</summary>
public class AutorGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "La nacionalidad admite máximo 60 caracteres.")]
    public string? Nacionalidad { get; set; }
}
