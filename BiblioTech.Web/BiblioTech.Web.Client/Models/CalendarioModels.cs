namespace BiblioTech.Web.Client.Models;

// Copia de los DTOs de api/calendario (mismos nombres: el JSON se convierte solo).

public class FestivoDto
{
    public DateOnly Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class EntregaDto
{
    public int PrestamoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Socio { get; set; } = string.Empty;
    public DateTime FechaDevolucionEsperada { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class CalendarioMesDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool FestivosDisponibles { get; set; }
    public List<FestivoDto> Festivos { get; set; } = [];
    public List<EntregaDto> Entregas { get; set; } = [];
}

public class AlertaPrestamoDto
{
    public int PrestamoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Socio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaDevolucionEsperada { get; set; }
    public int Dias { get; set; }
}

public class AlertasDto
{
    public bool FestivosDisponibles { get; set; }
    public int DiasHabilesAviso { get; set; }
    public List<AlertaPrestamoDto> EnMora { get; set; } = [];
    public List<AlertaPrestamoDto> ProximosAEntregar { get; set; } = [];
    public FestivoDto? ProximoFestivo { get; set; }
    public int Total { get; set; }
}
