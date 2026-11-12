using Microsoft.JSInterop;

namespace BiblioTech.Web.Client.Auth;

/// <summary>
/// Guarda el token JWT en el localStorage del navegador (sobrevive a recargar la página).
/// C# no puede tocar localStorage directamente: lo hace a través de JavaScript (IJSRuntime).
/// </summary>
public class TokenStorage(IJSRuntime js)
{
    private const string Clave = "bibliotech_token";

    public ValueTask<string?> ObtenerAsync() => js.InvokeAsync<string?>("localStorage.getItem", Clave);

    public ValueTask GuardarAsync(string token) => js.InvokeVoidAsync("localStorage.setItem", Clave, token);

    public ValueTask BorrarAsync() => js.InvokeVoidAsync("localStorage.removeItem", Clave);
}
