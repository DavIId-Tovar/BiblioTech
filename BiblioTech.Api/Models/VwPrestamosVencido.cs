using System;
using System.Collections.Generic;

namespace BiblioTech.Api.Models;

public partial class VwPrestamosVencido
{
    public int PrestamoId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Socio { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime FechaPrestamo { get; set; }

    public DateTime FechaDevolucionEsperada { get; set; }

    public int? DiasRetraso { get; set; }
}
