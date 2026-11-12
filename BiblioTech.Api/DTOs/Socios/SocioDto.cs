namespace BiblioTech.Api.DTOs.Socios;

public class SocioDto
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
    public int PrestamosActivos { get; set; }
}
