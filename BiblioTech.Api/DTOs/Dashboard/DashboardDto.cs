namespace BiblioTech.Api.DTOs.Dashboard;

/// <summary>Indicadores de la pantalla de inicio. Sale del SP sp_ResumenDashboard.</summary>
public class DashboardDto
{
    public int TotalLibros { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public int PrestamosActivos { get; set; }
    public int PrestamosVencidos { get; set; }
    public int SociosActivos { get; set; }
}
