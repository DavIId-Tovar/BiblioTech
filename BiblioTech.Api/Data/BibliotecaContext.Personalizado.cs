using BiblioTech.Api.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Data;

/// <summary>
/// Personalizaciones del DbContext. BibliotecaContext.cs lo GENERA el Scaffold y no se edita:
/// si la base de datos cambia, se regenera con -Force y este archivo no se pierde,
/// porque las dos partes forman una sola clase (partial).
/// </summary>
public partial class BibliotecaContext
{
    /// <summary>
    /// El Scaffold no genera los procedimientos almacenados: se ejecutan a mano.
    /// SqlQuery mapea cada columna del resultado a la propiedad con el mismo nombre.
    /// </summary>
    public async Task<DashboardDto> ObtenerResumenDashboardAsync()
    {
        // ToListAsync no "envuelve" el EXEC en otra consulta (un SP no se puede componer)
        var filas = await Database
            .SqlQuery<DashboardDto>($"EXEC dbo.sp_ResumenDashboard")
            .ToListAsync();

        return filas.Single();
    }

    // Aquí iría configuración extra del modelo que no exista en la base de datos.
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
