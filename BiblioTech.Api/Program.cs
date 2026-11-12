using System.Text;
using BiblioTech.Api.Configuracion;
using BiblioTech.Api.Data;
using BiblioTech.Api.Middleware;
using BiblioTech.Api.Services;
using BiblioTech.Api.Services.Externos;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ══ 1. SERVICIOS (inyección de dependencias) ═══════════════════════════

// DbContext: una instancia por petición HTTP (Scoped). La cadena sale de
// appsettings.json o de User Secrets, nunca escrita en el código.
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDb")));

// Servicios de negocio: se pide la interfaz y .NET entrega la implementación
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<ILibroService, LibroService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IPrestamoService, PrestamoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();

// ── API EXTERNA de festivos (Nager.Date) ───────────────────────────────
// AddHttpClient<Interfaz, Clase> = "cliente tipado": IHttpClientFactory crea y recicla
// las conexiones (evita agotar sockets) y entrega a FestivosClient un HttpClient
// ya configurado con la URL base y el tiempo máximo de espera.
builder.Services.AddMemoryCache();
builder.Services.Configure<FestivosApiOpciones>(builder.Configuration.GetSection(FestivosApiOpciones.Seccion));
builder.Services.AddHttpClient<IFestivosClient, FestivosClient>((proveedor, cliente) =>
{
    var opciones = proveedor.GetRequiredService<IOptions<FestivosApiOpciones>>().Value;
    cliente.BaseAddress = new Uri(opciones.BaseUrl);
    cliente.Timeout = TimeSpan.FromSeconds(opciones.TimeoutSegundos);
});

// ── JWT ────────────────────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>() ?? new JwtOpciones();
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException(
        "Falta la clave JWT (mínimo 32 caracteres). En la carpeta BiblioTech.Api ejecuta: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<una clave larga y aleatoria>\"");

builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection(JwtOpciones.Seccion));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // dejar los claims con su nombre corto (sub, email, role)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,                 // rechaza tokens vencidos
            ValidateIssuerSigningKey = true,         // rechaza tokens con firma alterada
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = "email",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// ── CORS: solo la aplicación Web puede llamar a la API desde el navegador ──
const string PoliticaFrontend = "Frontend";
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(PoliticaFrontend, policy =>
        policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

// ── Controllers: errores de validación (400) con título en español ──────
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = contexto =>
    {
        var problema = new ValidationProblemDetails(contexto.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Hay datos inválidos en la solicitud."
        };
        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    };
});

// ── OpenAPI + botón de token en Scalar ──────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((documento, contexto, cancelacion) =>
    {
        documento.Components ??= new OpenApiComponents();
        documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Pega el token que devuelve POST /api/auth/login"
        };
        documento.Security =
        [
            new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", documento)] = [] }
        ];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// ══ 2. PIPELINE (por dónde pasa cada petición, EN ORDEN) ═══════════════

app.UseMiddleware<ErrorHandlingMiddleware>();   // 1º: atrapa los errores de todo lo que sigue

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // documento /openapi/v1.json
    app.MapScalarApiReference();    // interfaz visual en /scalar
}

app.UseHttpsRedirection();
app.UseCors(PoliticaFrontend);      // antes de la autenticación
app.UseAuthentication();            // ¿quién eres? (lee y valida el token)
app.UseAuthorization();             // ¿puedes entrar? ([Authorize])
app.MapControllers();

app.Run();
