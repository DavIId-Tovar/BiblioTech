using Microsoft.EntityFrameworkCore.Design;

namespace BiblioTech.Api.Data;

/// <summary>
/// El Scaffold de EF Core pluraliza en INGLÉS: la tabla "Autores" generaba la clase
/// "Autore" y la navegación "Libro.Autors". Esta clase enseña al Scaffold las reglas
/// básicas del español. Solo se usa al ejecutar Scaffold-DbContext (tiempo de diseño),
/// no cuando la API está corriendo.
/// </summary>
public class PluralizadorEspanol : IPluralizer
{
    private const string Vocales = "aeiouáéíóú";

    // Libro -> Libros, Autor -> Autores, Prestamo -> Prestamos
    public string Pluralize(string nombre)
    {
        if (string.IsNullOrEmpty(nombre) || nombre.EndsWith('s')) return nombre;
        if (nombre.EndsWith('z')) return nombre[..^1] + "ces";
        return Vocales.Contains(char.ToLowerInvariant(nombre[^1])) ? nombre + "s" : nombre + "es";
    }

    // Libros -> Libro, Autores -> Autor, Socios -> Socio
    public string Singularize(string nombre)
    {
        if (string.IsNullOrEmpty(nombre) || !nombre.EndsWith('s')) return nombre;

        // "...es" precedido de consonante: Autores -> Autor
        if (nombre.Length > 3 && nombre.EndsWith("es") && !Vocales.Contains(char.ToLowerInvariant(nombre[^3])))
            return nombre[..^2];

        return nombre[..^1];
    }
}

/// <summary>
/// EF Core busca esta clase en el proyecto cuando se ejecuta el Scaffold
/// y registra nuestro pluralizador en lugar del de inglés.
/// </summary>
public class DisenoEfCore : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
        => services.AddSingleton<IPluralizer, PluralizadorEspanol>();
}
