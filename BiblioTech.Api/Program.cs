using BiblioTech.Api.Data;
using BiblioTech.Api.Services;
using BiblioTech.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios (inyección de dependencias) ──────────────────────────────

// DbContext: una instancia por petición HTTP (Scoped). La cadena sale de
// appsettings.json o de User Secrets, nunca escrita en el código.
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDb")));

// Servicios de negocio: se pide la interfaz y .NET entrega la implementación
builder.Services.AddScoped<IAutorService, AutorService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Pipeline (por dónde pasa cada petición, en orden) ──────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // documento /openapi/v1.json
    app.MapScalarApiReference();    // interfaz visual en /scalar
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
