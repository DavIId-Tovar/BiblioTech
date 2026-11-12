using BiblioTech.Api.DTOs.Dashboard;

namespace BiblioTech.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> ObtenerResumenAsync();
}
