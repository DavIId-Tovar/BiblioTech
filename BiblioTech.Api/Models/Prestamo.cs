using System;
using System.Collections.Generic;

namespace BiblioTech.Api.Models;

public partial class Prestamo
{
    public int Id { get; set; }

    public int LibroId { get; set; }

    public int SocioId { get; set; }

    public DateTime FechaPrestamo { get; set; }

    public DateTime FechaDevolucionEsperada { get; set; }

    public DateTime? FechaDevolucionReal { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Libro Libro { get; set; } = null!;

    public virtual Socio Socio { get; set; } = null!;
}
