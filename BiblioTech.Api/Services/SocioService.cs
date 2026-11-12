using BiblioTech.Api.Constants;
using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Socios;
using BiblioTech.Api.Exceptions;
using BiblioTech.Api.Models;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Services;

public class SocioService(BibliotecaContext db) : ISocioService
{
    public async Task<List<SocioDto>> ListarAsync(bool soloActivos)
    {
        var consulta = db.Socios.AsQueryable();
        if (soloActivos) consulta = consulta.Where(s => s.Activo);

        return await consulta
            .OrderBy(s => s.Nombre)
            .Select(s => new SocioDto
            {
                Id = s.Id,
                Documento = s.Documento,
                Nombre = s.Nombre,
                Email = s.Email,
                FechaRegistro = s.FechaRegistro,
                Activo = s.Activo,
                // Subconsulta: EF la traduce a un COUNT dentro del mismo SELECT
                PrestamosActivos = s.Prestamos.Count(p => p.Estado == EstadosPrestamo.Activo)
            })
            .ToListAsync();
    }

    public async Task<SocioDto> ObtenerAsync(int id)
    {
        return (await ListarPorIdAsync(id)) ?? throw new NoEncontradoException($"El socio {id} no existe.");
    }

    public async Task<SocioDto> CrearAsync(SocioGuardarDto dto)
    {
        var (documento, email) = (dto.Documento.Trim(), dto.Email.Trim().ToLowerInvariant());
        await ValidarDuplicadosAsync(documento, email, idActual: null);

        var socio = new Socio
        {
            Documento = documento,
            Nombre = dto.Nombre.Trim(),
            Email = email,
            Activo = true
            // FechaRegistro: la pone la BD (DEFAULT SYSDATETIME())
        };

        db.Socios.Add(socio);
        await db.SaveChangesAsync();

        return await ObtenerAsync(socio.Id);
    }

    public async Task ActualizarAsync(int id, SocioGuardarDto dto)
    {
        var socio = await db.Socios.FindAsync(id)
            ?? throw new NoEncontradoException($"El socio {id} no existe.");

        var (documento, email) = (dto.Documento.Trim(), dto.Email.Trim().ToLowerInvariant());
        await ValidarDuplicadosAsync(documento, email, idActual: id);

        socio.Documento = documento;
        socio.Nombre = dto.Nombre.Trim();
        socio.Email = email;
        socio.Activo = dto.Activo;

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Borrado LÓGICO: no se elimina la fila (el socio tiene historial de préstamos),
    /// solo se marca Activo = 0. Un socio inactivo no puede pedir préstamos (R6).
    /// </summary>
    public async Task DesactivarAsync(int id)
    {
        var socio = await db.Socios.FindAsync(id)
            ?? throw new NoEncontradoException($"El socio {id} no existe.");

        socio.Activo = false;
        await db.SaveChangesAsync();
    }

    private async Task ValidarDuplicadosAsync(string documento, string email, int? idActual)
    {
        if (await db.Socios.AnyAsync(s => s.Documento == documento && s.Id != idActual))
            throw new ReglaNegocioException("Ya existe un socio con ese documento.");

        if (await db.Socios.AnyAsync(s => s.Email == email && s.Id != idActual))
            throw new ReglaNegocioException("Ya existe un socio con ese correo.");
    }

    private Task<SocioDto?> ListarPorIdAsync(int id) => db.Socios
        .Where(s => s.Id == id)
        .Select(s => new SocioDto
        {
            Id = s.Id,
            Documento = s.Documento,
            Nombre = s.Nombre,
            Email = s.Email,
            FechaRegistro = s.FechaRegistro,
            Activo = s.Activo,
            PrestamosActivos = s.Prestamos.Count(p => p.Estado == EstadosPrestamo.Activo)
        })
        .FirstOrDefaultAsync();
}
