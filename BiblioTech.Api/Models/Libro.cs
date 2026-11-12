using System;
using System.Collections.Generic;

namespace BiblioTech.Api.Models;

public partial class Libro
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string Isbn { get; set; } = null!;

    public int? AnioPublicacion { get; set; }

    public int EjemplaresTotales { get; set; }

    public int EjemplaresDisponibles { get; set; }

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();

    public virtual ICollection<Autor> Autores { get; set; } = new List<Autor>();
}
