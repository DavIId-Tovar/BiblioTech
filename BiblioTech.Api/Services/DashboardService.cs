using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Dashboard;
using BiblioTech.Api.Services.Interfaces;

namespace BiblioTech.Api.Services;

/// <summary>Los indicadores salen del procedimiento almacenado sp_ResumenDashboard.</summary>
public class DashboardService(BibliotecaContext db) : IDashboardService
{
    public Task<DashboardDto> ObtenerResumenAsync() => db.ObtenerResumenDashboardAsync();
}
