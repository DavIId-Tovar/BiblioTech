using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BiblioTech.Web.Client.Shared;

/// <summary>
/// Clase base para las páginas que exigen sesión. Antes de cargar datos revisa si hay
/// token válido; si no, manda al login. Cada página solo implementa CargarAsync().
/// (Es solo experiencia de usuario: la API igual rechaza con 401 sin token.)
/// </summary>
public abstract class PaginaProtegida : ComponentBase
{
    [Inject] protected AuthenticationStateProvider Autenticacion { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected bool SesionVerificada { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var estado = await Autenticacion.GetAuthenticationStateAsync();
        if (estado.User.Identity?.IsAuthenticated != true)
        {
            Nav.NavigateTo("login");
            return;
        }

        SesionVerificada = true;
        await CargarAsync();
    }

    protected virtual Task CargarAsync() => Task.CompletedTask;
}
