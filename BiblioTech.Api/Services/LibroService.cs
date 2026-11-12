using BiblioTech.Api.Data;
using BiblioTech.Api.DTOs.Libros;
using BiblioTech.Api.Exceptions;
using BiblioTech.Api.Models;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Services;

public class LibroService(BibliotecaContext db) : ILibroService
{
    public async Task<List<LibroDto>> ListarAsync(string? buscar)
    {
        var consulta = db.Libros.AsQueryable();

        // Búsqueda por título, ISBN o nombre de autor (EF lo traduce a WHERE ... LIKE)
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(l =>
                l.Titulo.Contains(texto) ||
                l.Isbn.Contains(texto) ||
                l.Autores.Any(a => a.Nombre.Contains(texto)));
        }

        return await consulta
            .OrderBy(l => l.Titulo)
            .Select(ProyectarDto())
            .ToListAsync();
    }

    public async Task<LibroDto> ObtenerAsync(int id)
    {
        return await db.Libros
            .Where(l => l.Id == id)
            .Select(ProyectarDto())
            .FirstOrDefaultAsync()
            ?? throw new NoEncontradoException($"El libro {id} no existe.");
    }

    public async Task<LibroDto> CrearAsync(LibroGuardarDto dto)
    {
        var isbn = dto.Isbn.Trim();
        if (await db.Libros.AnyAsync(l => l.Isbn == isbn))
            throw new ReglaNegocioException("Ya existe un libro con ese ISBN.");

        var libro = new Libro
        {
            Titulo = dto.Titulo.Trim(),
            Isbn = isbn,
            AnioPublicacion = dto.AnioPublicacion,
            EjemplaresTotales = dto.EjemplaresTotales,
            EjemplaresDisponibles = dto.EjemplaresTotales,   // libro nuevo: todos disponibles
            Autores = await ObtenerAutoresAsync(dto.AutorIds)  // N:N: EF llena LibroAutor solo
        };

        db.Libros.Add(libro);
        await db.SaveChangesAsync();

        return await ObtenerAsync(libro.Id);
    }

    public async Task ActualizarAsync(int id, LibroGuardarDto dto)
    {
        // Include: se cargan los autores actuales para poder cambiarlos
        var libro = await db.Libros
            .Include(l => l.Autores)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NoEncontradoException($"El libro {id} no existe.");

        var isbn = dto.Isbn.Trim();
        if (await db.Libros.AnyAsync(l => l.Isbn == isbn && l.Id != id))
            throw new ReglaNegocioException("Ya existe otro libro con ese ISBN.");

        // Los ejemplares prestados no pueden "desaparecer" al editar el total
        var prestados = libro.EjemplaresTotales - libro.EjemplaresDisponibles;
        if (dto.EjemplaresTotales < prestados)
            throw new ReglaNegocioException(
                $"Hay {prestados} ejemplar(es) prestado(s); el total no puede ser menor que eso.");

        libro.Titulo = dto.Titulo.Trim();
        libro.Isbn = isbn;
        libro.AnioPublicacion = dto.AnioPublicacion;
        libro.EjemplaresTotales = dto.EjemplaresTotales;
        libro.EjemplaresDisponibles = dto.EjemplaresTotales - prestados;

        // Reemplazar autores: EF calcula qué filas de LibroAutor borrar e insertar
        var nuevosAutores = await ObtenerAutoresAsync(dto.AutorIds);
        libro.Autores.Clear();
        foreach (var autor in nuevosAutores) libro.Autores.Add(autor);

        await db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var libro = await db.Libros.FindAsync(id)
            ?? throw new NoEncontradoException($"El libro {id} no existe.");

        if (await db.Prestamos.AnyAsync(p => p.LibroId == id))
            throw new ReglaNegocioException("No se puede eliminar: el libro tiene préstamos registrados.");

        db.Libros.Remove(libro);   // LibroAutor se borra solo (ON DELETE CASCADE)
        await db.SaveChangesAsync();
    }

    private async Task<List<Autor>> ObtenerAutoresAsync(List<int> autorIds)
    {
        var ids = autorIds.Distinct().ToList();
        var autores = await db.Autores.Where(a => ids.Contains(a.Id)).ToListAsync();

        if (autores.Count != ids.Count)
            throw new NoEncontradoException("Uno o más de los autores seleccionados no existen.");

        return autores;
    }

    // Una sola proyección reutilizada por Listar y Obtener
    private static System.Linq.Expressions.Expression<Func<Libro, LibroDto>> ProyectarDto() => l => new LibroDto
    {
        Id = l.Id,
        Titulo = l.Titulo,
        Isbn = l.Isbn,
        AnioPublicacion = l.AnioPublicacion,
        EjemplaresTotales = l.EjemplaresTotales,
        EjemplaresDisponibles = l.EjemplaresDisponibles,
        Autores = l.Autores
            .OrderBy(a => a.Nombre)
            .Select(a => new AutorResumenDto { Id = a.Id, Nombre = a.Nombre })
            .ToList()
    };
}
