using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Api.DTOs.Prestamos;

/// <summary>
/// Para prestar solo se necesita QUIÉN y QUÉ. Las fechas y el estado
/// los decide el servidor (R4): el cliente no puede inventarlos.
/// </summary>
public class PrestamoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un libro.")]
    public int LibroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un socio.")]
    public int SocioId { get; set; }
}
