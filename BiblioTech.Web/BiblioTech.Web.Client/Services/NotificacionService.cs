namespace BiblioTech.Web.Client.Services;

/// <summary>
/// Guarda un mensaje de éxito para mostrarlo en la SIGUIENTE página ("mensaje flash").
/// Ejemplo: el formulario de autor guarda "Se creó el autor X", navega a /autores
/// y la lista lo toma y lo muestra. Tomar() lo borra para que salga una sola vez.
/// En WebAssembly un servicio Scoped vive mientras la pestaña esté abierta,
/// por eso el mensaje sobrevive al cambio de página.
/// </summary>
public class NotificacionService
{
    private string? pendiente;

    public void Guardar(string mensaje) => pendiente = mensaje;

    public string? Tomar()
    {
        var mensaje = pendiente;
        pendiente = null;
        return mensaje;
    }
}
