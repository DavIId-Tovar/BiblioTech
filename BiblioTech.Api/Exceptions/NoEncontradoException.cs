namespace BiblioTech.Api.Exceptions;

/// <summary>
/// Se lanza cuando el recurso pedido no existe (libro 99, socio 500...).
/// El middleware de errores la convierte en 404 Not Found.
/// </summary>
public class NoEncontradoException(string mensaje) : Exception(mensaje);
