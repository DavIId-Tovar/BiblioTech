using System.Net.Http.Headers;

namespace BiblioTech.Web.Client.Auth;

/// <summary>
/// "Intermediario" que se ejecuta ANTES de cada petición del HttpClient "Api":
/// le pega el carnet (Authorization: Bearer token). Así ninguna página ni servicio
/// tiene que acordarse de hacerlo.
/// </summary>
public class AuthHeaderHandler(TokenStorage tokenStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStorage.ObtenerAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
