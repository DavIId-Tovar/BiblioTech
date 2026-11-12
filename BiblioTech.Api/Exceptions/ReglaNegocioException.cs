namespace BiblioTech.Api.Exceptions;

/// <summary>
/// Se lanza cuando una operación viola una regla del negocio (R1–R6, ISBN repetido...).
/// El middleware de errores la convierte en 409 Conflict con el mensaje para el usuario.
/// </summary>
public class ReglaNegocioException(string mensaje) : Exception(mensaje);
