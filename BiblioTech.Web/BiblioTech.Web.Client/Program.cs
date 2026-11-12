using BiblioTech.Web.Client.Auth;
using BiblioTech.Web.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Este Program.cs se ejecuta EN EL NAVEGADOR (WebAssembly), no en el servidor.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// La URL de la API está en wwwroot/appsettings.json (es pública: ningún secreto ahí)
var apiUrl = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException("Falta \"ApiUrl\" en wwwroot/appsettings.json");

// ── Sesión: guardar el token y decirle a Blazor si hay alguien conectado ──
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// ── HttpClient con nombre "Api": cada petición pasa por AuthHeaderHandler,
//    que le agrega "Authorization: Bearer <token>" ──
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services
    .AddHttpClient(ApiServiceBase.NombreCliente, client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();

// ── Servicios HTTP: las páginas nunca usan HttpClient directamente ──
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<DashboardApiService>();
builder.Services.AddScoped<AutorApiService>();
builder.Services.AddScoped<LibroApiService>();
builder.Services.AddScoped<SocioApiService>();
builder.Services.AddScoped<PrestamoApiService>();

await builder.Build().RunAsync();
