using BiblioTech.Api.Constants;
using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Prestamos;
using BiblioTech.Api.Exceptions;
using BiblioTech.Api.Models;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Services;

/// <summary>
/// El corazón del proyecto: aquí viven las reglas de negocio R1–R6.
///   R1 Solo se presta si hay ejemplares disponibles
///   R2 Máximo 3 préstamos activos por socio
///   R3 Un socio con préstamos vencidos no puede pedir más
///   R4 Fecha de devolución = fecha del préstamo + 15 días (la calcula el servidor)
///   R5 Prestar resta 1 ejemplar y devolver suma 1, en una TRANSACCIÓN
///   R6 Un socio inactivo no puede pedir préstamos
/// </summary>
public class PrestamoService(BibliotecaContext db) : IPrestamoService
{
    public async Task<List<PrestamoDto>> ListarAsync(string? estado)
    {
        var ahora = DateTime.Now;
        var consulta = db.Prestamos.AsQueryable();

        // "Vencido" no existe en la tabla: se calcula con la fecha esperada
        consulta = estado switch
        {
            EstadosPrestamo.Activo => consulta.Where(p => p.Estado == EstadosPrestamo.Activo && p.FechaDevolucionEsperada >= ahora),
            EstadosPrestamo.Vencido => consulta.Where(p => p.Estado == EstadosPrestamo.Activo && p.FechaDevolucionEsperada < ahora),
            EstadosPrestamo.Devuelto => consulta.Where(p => p.Estado == EstadosPrestamo.Devuelto),
            _ => consulta
        };

        return await consulta
            .OrderByDescending(p => p.FechaPrestamo)
            .Select(ProyectarDto(ahora))
            .ToListAsync();
    }

    public async Task<PrestamoDto> ObtenerAsync(int id)
    {
        return await db.Prestamos
            .Where(p => p.Id == id)
            .Select(ProyectarDto(DateTime.Now))
            .FirstOrDefaultAsync()
            ?? throw new NoEncontradoException($"El préstamo {id} no existe.");
    }

    public async Task<PrestamoDto> CrearAsync(PrestamoCrearDto dto)
    {
        var ahora = DateTime.Now;

        // ── 1. El socio existe y está activo ──────────────────────────────
        var socio = await db.Socios
            .Where(s => s.Id == dto.SocioId)
            .Select(s => new { s.Nombre, s.Activo })
            .FirstOrDefaultAsync()
            ?? throw new NoEncontradoException($"El socio {dto.SocioId} no existe.");

        if (!socio.Activo)
            throw new ReglaNegocioException($"El socio {socio.Nombre} está inactivo y no puede pedir préstamos.");   // R6

        // ── 2. Préstamos activos del socio (una sola consulta para R3 y R2) ─
        var fechasActivos = await db.Prestamos
            .Where(p => p.SocioId == dto.SocioId && p.Estado == EstadosPrestamo.Activo)
            .Select(p => p.FechaDevolucionEsperada)
            .ToListAsync();

        if (fechasActivos.Any(f => f < ahora))
            throw new ReglaNegocioException($"{socio.Nombre} tiene préstamos vencidos y no puede pedir más.");   // R3

        if (fechasActivos.Count >= ReglasPrestamo.MaximoPrestamosActivos)
            throw new ReglaNegocioException(
                $"{socio.Nombre} ya tiene {ReglasPrestamo.MaximoPrestamosActivos} préstamos activos (máximo permitido).");   // R2

        // ── 3. El libro existe y tiene ejemplares ─────────────────────────
        var libro = await db.Libros
            .Where(l => l.Id == dto.LibroId)
            .Select(l => new { l.Titulo, l.EjemplaresDisponibles })
            .FirstOrDefaultAsync()
            ?? throw new NoEncontradoException($"El libro {dto.LibroId} no existe.");

        if (libro.EjemplaresDisponibles <= 0)
            throw new ReglaNegocioException($"No hay ejemplares disponibles de \"{libro.Titulo}\".");   // R1

        // ── 4. Transacción: o se hacen las dos cosas o ninguna (R5) ───────
        await using var transaccion = await db.Database.BeginTransactionAsync();

        var prestamo = new Prestamo
        {
            LibroId = dto.LibroId,
            SocioId = dto.SocioId,
            FechaPrestamo = ahora,
            FechaDevolucionEsperada = ahora.AddDays(ReglasPrestamo.DiasPrestamo),   // R4
            Estado = EstadosPrestamo.Activo
        };
        db.Prestamos.Add(prestamo);
        await db.SaveChangesAsync();                                            // INSERT Prestamos

        // UPDATE con condición: si otro préstamo se llevó el último ejemplar
        // entre la validación y este punto, no se actualiza ninguna fila.
        var filas = await db.Libros
            .Where(l => l.Id == dto.LibroId && l.EjemplaresDisponibles > 0)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.EjemplaresDisponibles, l => l.EjemplaresDisponibles - 1));

        if (filas == 0)
            // Al salir sin CommitAsync, la transacción hace ROLLBACK y el INSERT se deshace
            throw new ReglaNegocioException($"No hay ejemplares disponibles de \"{libro.Titulo}\".");   // R1

        await transaccion.CommitAsync();                                        // COMMIT

        return await ObtenerAsync(prestamo.Id);
    }

    public async Task DevolverAsync(int id)
    {
        var prestamo = await db.Prestamos.FindAsync(id)
            ?? throw new NoEncontradoException($"El préstamo {id} no existe.");

        if (prestamo.Estado == EstadosPrestamo.Devuelto)
            throw new ReglaNegocioException("Este préstamo ya fue devuelto.");

        await using var transaccion = await db.Database.BeginTransactionAsync();

        prestamo.Estado = EstadosPrestamo.Devuelto;
        prestamo.FechaDevolucionReal = DateTime.Now;
        await db.SaveChangesAsync();                                            // UPDATE Prestamos

        await db.Libros
            .Where(l => l.Id == prestamo.LibroId)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.EjemplaresDisponibles, l => l.EjemplaresDisponibles + 1));   // R5

        await transaccion.CommitAsync();
    }

    /// <summary>Reporte que sale directamente de la vista SQL vw_PrestamosVencidos.</summary>
    public async Task<List<PrestamoVencidoDto>> ListarVencidosAsync()
    {
        return await db.VwPrestamosVencidos
            .OrderByDescending(v => v.DiasRetraso)
            .Select(v => new PrestamoVencidoDto
            {
                PrestamoId = v.PrestamoId,
                Titulo = v.Titulo,
                Socio = v.Socio,
                Email = v.Email,
                FechaPrestamo = v.FechaPrestamo,
                FechaDevolucionEsperada = v.FechaDevolucionEsperada,
                DiasRetraso = v.DiasRetraso ?? 0
            })
            .ToListAsync();
    }

    private static System.Linq.Expressions.Expression<Func<Prestamo, PrestamoDto>> ProyectarDto(DateTime ahora) => p => new PrestamoDto
    {
        Id = p.Id,
        LibroId = p.LibroId,
        Titulo = p.Libro.Titulo,
        SocioId = p.SocioId,
        Socio = p.Socio.Nombre,
        FechaPrestamo = p.FechaPrestamo,
        FechaDevolucionEsperada = p.FechaDevolucionEsperada,
        FechaDevolucionReal = p.FechaDevolucionReal,
        // EF lo traduce a un CASE WHEN en SQL
        Estado = p.Estado == EstadosPrestamo.Devuelto
            ? EstadosPrestamo.Devuelto
            : p.FechaDevolucionEsperada < ahora ? EstadosPrestamo.Vencido : EstadosPrestamo.Activo
    };
}
