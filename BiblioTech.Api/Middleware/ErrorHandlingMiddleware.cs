using BiblioTech.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Api.Middleware;

/// <summary>
/// Envuelve TODA la petición en un try/catch. Si algo lanza una excepción,
/// responde siempre con el mismo formato (ProblemDetails) y el código correcto:
///   NoEncontradoException → 404 · ReglaNegocioException → 409
///   DbUpdateException (UNIQUE, FK, CHECK) → 409 · cualquier otra → 500
/// Así los Controllers no necesitan try/catch.
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    // Nombre de la restricción en SQL Server → mensaje entendible para el usuario.
    // Por eso en el script todas las restricciones tienen nombre (UQ_, FK_, CK_).
    private static readonly Dictionary<string, string> MensajesRestricciones = new()
    {
        ["UQ_Libros_ISBN"] = "Ya existe un libro con ese ISBN.",
        ["UQ_Socios_Documento"] = "Ya existe un socio con ese documento.",
        ["UQ_Socios_Email"] = "Ya existe un socio con ese correo.",
        ["UQ_Usuarios_Email"] = "Ya existe un usuario con ese correo.",
        ["FK_Prestamos_Libros"] = "El libro tiene préstamos asociados.",
        ["FK_Prestamos_Socios"] = "El socio tiene préstamos asociados.",
        ["FK_LibroAutor_Autores"] = "El autor tiene libros asociados.",
        ["CK_Libros_Disponibles"] = "Los ejemplares disponibles no pueden ser negativos ni mayores que los totales.",
        ["CK_Socios_Email"] = "El correo no tiene un formato válido."
    };

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await next(contexto);   // sigue al resto del pipeline (CORS, auth, controllers...)
        }
        catch (Exception ex) when (!contexto.Response.HasStarted)
        {
            await ResponderAsync(contexto, ex);
        }
    }

    private async Task ResponderAsync(HttpContext contexto, Exception ex)
    {
        var (codigo, titulo, detalle) = ex switch
        {
            NoEncontradoException e => (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Message),
            ReglaNegocioException e => (StatusCodes.Status409Conflict, "Regla de negocio", e.Message),
            DbUpdateException e => (StatusCodes.Status409Conflict, "Conflicto con los datos", TraducirErrorBaseDatos(e)),
            _ => (StatusCodes.Status500InternalServerError, "Error interno",
                  "Ocurrió un error inesperado. Intenta de nuevo más tarde.")
        };

        // El detalle técnico va al log; al cliente solo el mensaje seguro
        if (codigo == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
        else
            logger.LogWarning("{Codigo} en {Ruta}: {Mensaje}", codigo, contexto.Request.Path, detalle);

        var problema = new ProblemDetails
        {
            Status = codigo,
            Title = titulo,
            Detail = detalle,
            Instance = contexto.Request.Path
        };

        contexto.Response.StatusCode = codigo;
        await contexto.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json");
    }

    private static string TraducirErrorBaseDatos(DbUpdateException ex)
    {
        var mensajeSql = ex.InnerException?.Message ?? ex.Message;

        foreach (var (restriccion, mensaje) in MensajesRestricciones)
        {
            if (mensajeSql.Contains(restriccion, StringComparison.OrdinalIgnoreCase))
                return mensaje;
        }

        return "La operación viola una restricción de la base de datos.";
    }
}
