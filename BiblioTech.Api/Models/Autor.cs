using System;
using System.Collections.Generic;

namespace BiblioTech.Api.Models;

public partial class Autor
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Nacionalidad { get; set; }

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
