namespace BiblioTech.Api.DTOs.Calendario;

/// <summary>Un día festivo, ya en nuestro formato (no el de la API externa).</summary>
public class FestivoDto
{
    public DateOnly Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>Respuesta de GET api/calendario/festivos/{anio}.</summary>
public class FestivosAnioDto
{
    public int Anio { get; set; }

    /// <summary>false si la API externa no respondió: la lista llega vacía pero la app sigue funcionando.</summary>
    public bool FestivosDisponibles { get; set; }

    public List<FestivoDto> Festivos { get; set; } = [];
}

/// <summary>Un préstamo pendiente que se debe entregar en una fecha del calendario.</summary>
public class EntregaDto
{
    public int PrestamoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Socio { get; set; } = string.Empty;
    public DateTime FechaDevolucionEsperada { get; set; }

    /// <summary>Activo o Vencido.</summary>
    public string Estado { get; set; } = string.Empty;
}

/// <summary>Respuesta de GET api/calendario?anio=2026&amp;mes=11.</summary>
public class CalendarioMesDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public bool FestivosDisponibles { get; set; }
    public List<FestivoDto> Festivos { get; set; } = [];
    public List<EntregaDto> Entregas { get; set; } = [];
}

/// <summary>Un préstamo en mora o próximo a entregar.</summary>
public class AlertaPrestamoDto
{
    public int PrestamoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Socio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaDevolucionEsperada { get; set; }

    /// <summary>En mora: días de retraso. Próximo: días HÁBILES que le quedan.</summary>
    public int Dias { get; set; }
}

/// <summary>Respuesta de GET api/calendario/alertas (la campana de notificaciones).</summary>
public class AlertasDto
{
    public bool FestivosDisponibles { get; set; }
    public int DiasHabilesAviso { get; set; }
    public List<AlertaPrestamoDto> EnMora { get; set; } = [];
    public List<AlertaPrestamoDto> ProximosAEntregar { get; set; } = [];
    public FestivoDto? ProximoFestivo { get; set; }
    public int Total => EnMora.Count + ProximosAEntregar.Count;
}
