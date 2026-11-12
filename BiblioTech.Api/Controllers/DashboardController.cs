using BiblioTech.Api.DTOs.Dashboard;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Obtener()
        => Ok(await servicio.ObtenerResumenAsync());
}
