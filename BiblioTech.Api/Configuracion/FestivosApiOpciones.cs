namespace BiblioTech.Api.Configuracion;

/// <summary>
/// Configuración de la API EXTERNA de festivos (Nager.Date, pública y sin clave).
/// Se llena desde la sección "FestivosApi" de appsettings.json.
/// </summary>
public class FestivosApiOpciones
{
    public const string Seccion = "FestivosApi";

    /// <summary>Dirección base de la API externa (termina en /).</summary>
    public string BaseUrl { get; set; } = "https://date.nager.at/api/v3/";

    /// <summary>Código ISO del país: CO = Colombia.</summary>
    public string CodigoPais { get; set; } = "CO";

    /// <summary>Si la API externa tarda más que esto, se cancela la petición.</summary>
    public int TimeoutSegundos { get; set; } = 10;

    /// <summary>Cuántas horas se guardan los festivos en memoria antes de volver a pedirlos.</summary>
    public int HorasCache { get; set; } = 24;
}
