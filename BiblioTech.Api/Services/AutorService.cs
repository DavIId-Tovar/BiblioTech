using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Autores;
using BiblioTech.Api.Exceptions;
using BiblioTech.Api.Models;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Services;

/// <summary>
/// Versión del día 3: en vez de devolver null/false, el servicio LANZA excepciones
/// de negocio y el middleware las convierte en 404 / 409. El Controller queda más limpio.
/// </summary>
public class AutorService(BibliotecaContext db) : IAutorService
{
    public async Task<List<AutorDto>> ListarAsync()
    {
        // Proyección: EF traduce este Select a un SELECT solo con estas columnas
        return await db.Autores
            .OrderBy(a => a.Nombre)
            .Select(a => new AutorDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Nacionalidad = a.Nacionalidad,
                CantidadLibros = a.Libros.Count
            })
            .ToListAsync();
    }

    public async Task<AutorDto> ObtenerAsync(int id)
    {
        return await db.Autores
            .Where(a => a.Id == id)
            .Select(a => new AutorDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Nacionalidad = a.Nacionalidad,
                CantidadLibros = a.Libros.Count
            })
            .FirstOrDefaultAsync()
            ?? throw new NoEncontradoException($"El autor {id} no existe.");
    }

    public async Task<AutorDto> CrearAsync(AutorGuardarDto dto)
    {
        var autor = new Autor
        {
            Nombre = dto.Nombre.Trim(),
            Nacionalidad = string.IsNullOrWhiteSpace(dto.Nacionalidad) ? null : dto.Nacionalidad.Trim()
        };

        db.Autores.Add(autor);
        await db.SaveChangesAsync();   // INSERT; aquí SQL Server asigna el Id

        return new AutorDto { Id = autor.Id, Nombre = autor.Nombre, Nacionalidad = autor.Nacionalidad };
    }

    public async Task ActualizarAsync(int id, AutorGuardarDto dto)
    {
        var autor = await db.Autores.FindAsync(id)
            ?? throw new NoEncontradoException($"El autor {id} no existe.");

        autor.Nombre = dto.Nombre.Trim();
        autor.Nacionalidad = string.IsNullOrWhiteSpace(dto.Nacionalidad) ? null : dto.Nacionalidad.Trim();

        await db.SaveChangesAsync();   // UPDATE solo de las columnas que cambiaron
    }

    public async Task EliminarAsync(int id)
    {
        var autor = await db.Autores.FindAsync(id)
            ?? throw new NoEncontradoException($"El autor {id} no existe.");

        var cantidadLibros = await db.Libros.CountAsync(l => l.Autores.Any(a => a.Id == id));
        if (cantidadLibros > 0)
            throw new ReglaNegocioException($"No se puede eliminar: el autor tiene {cantidadLibros} libro(s) asociado(s).");

        db.Autores.Remove(autor);
        await db.SaveChangesAsync();   // DELETE
    }
}
