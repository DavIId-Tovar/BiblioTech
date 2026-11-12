# BiblioTech

Sistema de préstamos de biblioteca para enseñar el stack .NET full stack:
**SQL Server · EF Core 10 (Database First) · ASP.NET Core Web API · Blazor WebAssembly · GitHub · Azure**.

## Estructura

```
BiblioTech/
├─ database/                 ← BiblioTech.sql (local) y BiblioTech.Azure.sql
├─ BiblioTech.Api/           ← Web API: reglas de negocio, datos y seguridad (JSON)
├─ BiblioTech.Web/
│   ├─ BiblioTech.Web/        ← Host de Blazor (servidor)
│   └─ BiblioTech.Web.Client/ ← Lo que corre en el navegador (WebAssembly)
└─ BiblioTech.slnx
```

## Cómo empezar

1. Ejecutar `database/BiblioTech.sql` en SSMS (F5) contra el servidor local.
2. Si tu instancia no es `localhost`, configurar la cadena en User Secrets de la Api.
3. Abrir `BiblioTech.slnx` en Visual Studio y compilar.

## Participante

- Nombre: _(escribe aquí tu nombre en tu primer commit)_
