using BiblioTech.Api.Constants;
using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Calendario;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Services;

/// <summary>
/// Junta DOS fuentes de datos:
///  · La API externa de festivos (a través de IFestivosClient).
///  · Nuestra base de datos (préstamos pendientes de entrega).
/// El frontend solo habla con NUESTRA API: nunca llama directamente a la API externa.
/// </summary>
public class CalendarioService(BibliotecaContext db, IFestivosClient festivosClient) : ICalendarioService
{
    public async Task<FestivosAnioDto> ObtenerFestivosAsync(int anio)
    {
        var (disponible, festivos) = await festivosClient.ObtenerAsync(anio);
        return new FestivosAnioDto { Anio = anio, FestivosDisponibles = disponible, Festivos = festivos };
    }

    public async Task<CalendarioMesDto> ObtenerMesAsync(int anio, int mes)
    {
        var inicio = new DateTime(anio, mes, 1);
        var fin = inicio.AddMonths(1);
        var ahora = DateTime.Now;

        var (disponible, festivos) = await festivosClient.ObtenerAsync(anio);

        // Préstamos sin devolver cuya fecha de entrega cae en el mes
        var entregas = await db.Prestamos
            .Where(p => p.Estado == EstadosPrestamo.Activo
                     && p.FechaDevolucionEsperada >= inicio
                     && p.FechaDevolucionEsperada < fin)
            .OrderBy(p => p.FechaDevolucionEsperada)
            .Select(p => new EntregaDto
            {
                PrestamoId = p.Id,
                Titulo = p.Libro.Titulo,
                Socio = p.Socio.Nombre,
                FechaDevolucionEsperada = p.FechaDevolucionEsperada,
                Estado = p.FechaDevolucionEsperada < ahora ? EstadosPrestamo.Vencido : EstadosPrestamo.Activo
            })
            .ToListAsync();

        return new CalendarioMesDto
        {
            Anio = anio,
            Mes = mes,
            FestivosDisponibles = disponible,
            Festivos = festivos.Where(f => f.Fecha.Month == mes).ToList(),
            Entregas = entregas
        };
    }

    public async Task<AlertasDto> ObtenerAlertasAsync()
    {
        var ahora = DateTime.Now;
        var hoy = DateOnly.FromDateTime(ahora);

        // Festivos de este año y del siguiente (por si una entrega cae en enero)
        var (disponibleActual, festivosActual) = await festivosClient.ObtenerAsync(hoy.Year);
        var (disponibleSiguiente, festivosSiguiente) = await festivosClient.ObtenerAsync(hoy.Year + 1);
        var festivos = festivosActual.Concat(festivosSiguiente).ToList();
        var diasFestivos = festivos.Select(f => f.Fecha).ToHashSet();

        var pendientes = await db.Prestamos
            .Where(p => p.Estado == EstadosPrestamo.Activo)
            .OrderBy(p => p.FechaDevolucionEsperada)
            .Select(p => new AlertaPrestamoDto
            {
                PrestamoId = p.Id,
                Titulo = p.Libro.Titulo,
                Socio = p.Socio.Nombre,
                Email = p.Socio.Email,
                FechaDevolucionEsperada = p.FechaDevolucionEsperada
            })
            .ToListAsync();

        var alertas = new AlertasDto
        {
            FestivosDisponibles = disponibleActual && disponibleSiguiente,
            DiasHabilesAviso = ReglasPrestamo.DiasHabilesAviso,
            ProximoFestivo = festivos.FirstOrDefault(f => f.Fecha >= hoy)
        };

        foreach (var prestamo in pendientes)
        {
            if (prestamo.FechaDevolucionEsperada < ahora)
            {
                // En mora: días calendario desde la fecha esperada
                prestamo.Dias = hoy.DayNumber - DateOnly.FromDateTime(prestamo.FechaDevolucionEsperada).DayNumber;
                alertas.EnMora.Add(prestamo);
                continue;
            }

            var habiles = ContarDiasHabiles(hoy, DateOnly.FromDateTime(prestamo.FechaDevolucionEsperada), diasFestivos);
            if (habiles <= ReglasPrestamo.DiasHabilesAviso)
            {
                prestamo.Dias = habiles;
                alertas.ProximosAEntregar.Add(prestamo);
            }
        }

        alertas.EnMora = alertas.EnMora.OrderByDescending(a => a.Dias).ToList();
        return alertas;
    }

    /// <summary>
    /// Días hábiles DESPUÉS de hoy y hasta la fecha (incluida): lunes a viernes que no sean festivo.
    /// Aquí es donde la API externa aporta a la regla: sin festivos, el conteo sería incorrecto.
    /// </summary>
    private static int ContarDiasHabiles(DateOnly hoy, DateOnly hasta, HashSet<DateOnly> festivos)
    {
        var dias = 0;
        for (var dia = hoy.AddDays(1); dia <= hasta; dia = dia.AddDays(1))
        {
            var finDeSemana = dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            if (!finDeSemana && !festivos.Contains(dia)) dias++;
        }
        return dias;
    }
}
