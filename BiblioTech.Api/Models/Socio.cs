using System;
using System.Collections.Generic;

namespace BiblioTech.Api.Models;

public partial class Socio
{
    public int Id { get; set; }

    public string Documento { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
