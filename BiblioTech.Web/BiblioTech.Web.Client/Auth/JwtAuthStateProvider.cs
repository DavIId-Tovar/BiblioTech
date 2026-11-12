using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace BiblioTech.Web.Client.Auth;

/// <summary>
/// Le responde a Blazor la pregunta "¿hay alguien con sesión iniciada y quién es?".
/// Lee el token guardado, revisa que no esté vencido y saca los datos (claims) de su contenido.
/// OJO: aquí NO se valida la firma (eso solo lo puede hacer la API, que tiene la clave).
/// Esto es solo para la experiencia de usuario: la seguridad real está en la API.
/// </summary>
public class JwtAuthStateProvider(TokenStorage tokenStorage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorage.ObtenerAsync();
        if (string.IsNullOrWhiteSpace(token)) return Anonimo;

        var claims = LeerClaims(token);
        if (claims is null || TokenVencido(claims))
        {
            await tokenStorage.BorrarAsync();
            return Anonimo;
        }

        // "email" será el Name y "role" el Rol (los mismos nombres que pone la API)
        var identidad = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "email", roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    public async Task IniciarSesionAsync(string token)
    {
        await tokenStorage.GuardarAsync(token);
        // Avisa a todos los componentes (menú, AuthorizeView...) que la sesión cambió
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task CerrarSesionAsync()
    {
        await tokenStorage.BorrarAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
    }

    /// <summary>Un JWT son 3 partes en Base64Url separadas por puntos: encabezado.CONTENIDO.firma</summary>
    private static List<Claim>? LeerClaims(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;

            var json = Encoding.UTF8.GetString(DecodificarBase64Url(partes[1]));
            using var documento = JsonDocument.Parse(json);

            var claims = new List<Claim>();
            foreach (var propiedad in documento.RootElement.EnumerateObject())
            {
                if (propiedad.Value.ValueKind == JsonValueKind.Array)
                    claims.AddRange(propiedad.Value.EnumerateArray().Select(v => new Claim(propiedad.Name, v.ToString())));
                else
                    claims.Add(new Claim(propiedad.Name, propiedad.Value.ToString()));
            }
            return claims;
        }
        catch (FormatException) { return null; }
        catch (JsonException) { return null; }
    }

    private static bool TokenVencido(List<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        return exp is null
            || !long.TryParse(exp, out var segundos)
            || DateTimeOffset.FromUnixTimeSeconds(segundos) <= DateTimeOffset.UtcNow;
    }

    private static byte[] DecodificarBase64Url(string texto)
    {
        var base64 = texto.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
