namespace BiblioTech.Api.DTOs.Prestamos;

/// <summary>Una fila del reporte de vencidos (sale de la vista vw_PrestamosVencidos).</summary>
public class PrestamoVencidoDto
{
    public int PrestamoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Socio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public int DiasRetraso { get; set; }
}
