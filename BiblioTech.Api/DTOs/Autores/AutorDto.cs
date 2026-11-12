namespace BiblioTech.Api.DTOs.Autores;

/// <summary>Lo que la API DEVUELVE de un autor (salida).</summary>
public class AutorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
    public int CantidadLibros { get; set; }
}
