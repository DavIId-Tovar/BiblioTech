namespace BiblioTech.Api.Configuracion;

/// <summary>
/// Sección "Jwt" de la configuración.
/// Issuer, Audience y ExpiraMinutos van en appsettings.json.
/// Key (la clave secreta) va en User Secrets en local y en el App Service en Azure: NUNCA en Git.
/// </summary>
public class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiraMinutos { get; set; } = 120;
}
