namespace BiblioTech.Api.DTOs.Libros;

/// <summary>Libro con sus autores (N:N) y su disponibilidad ("2 de 5").</summary>
public class LibroDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int? AnioPublicacion { get; set; }
    public int EjemplaresTotales { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public List<AutorResumenDto> Autores { get; set; } = [];
}

/// <summary>Solo lo necesario del autor para mostrarlo dentro de un libro.</summary>
public class AutorResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
