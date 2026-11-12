using System.ComponentModel.DataAnnotations;

namespace BiblioTech.Web.Client.Models;

public class PrestamoDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int SocioId { get; set; }
    public string Socio { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class PrestamoCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un libro.")]
    public int LibroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un socio.")]
    public int SocioId { get; set; }
}

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

public static class EstadosPrestamo
{
    public const string Activo = "Activo";
    public const string Devuelto = "Devuelto";
    public const string Vencido = "Vencido";
}
