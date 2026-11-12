# BiblioTech

Sistema de préstamos de biblioteca para enseñar el stack .NET full stack:
**SQL Server · EF Core 10 (Database First) · ASP.NET Core Web API · Blazor WebAssembly · GitHub · Azure**.

Un bibliotecario inicia sesión, gestiona libros, autores y socios, registra préstamos y devoluciones
con reglas automáticas, y consulta los vencidos y un dashboard.

## Estructura

```
BiblioTech/
├─ database/
│   ├─ BiblioTech.sql          ← script local (borra y recrea la BD)
│   └─ BiblioTech.Azure.sql    ← mismo script sin la sección 0 (para Azure SQL)
├─ BiblioTech.Api/              ← Web API: reglas de negocio, datos y seguridad (JSON)
├─ BiblioTech.Web/
│   ├─ BiblioTech.Web/          ← Host de Blazor (servidor): App, Routes, Layout, NavMenu
│   └─ BiblioTech.Web.Client/   ← Lo que corre en el navegador (WASM): páginas, servicios, auth
├─ BiblioTech.slnx
└─ BiblioTech.slnLaunch         ← perfil "Api + Web" para arrancar los dos proyectos a la vez
```

## Cómo ejecutarlo (primera vez)

1. **Base de datos:** abrir `database/BiblioTech.sql` en SSMS (servidor local) y pulsar **F5**.


2. **Cadena de conexión:** si tu servidor no es `localhost` (por ejemplo `.\SQLEXPRESS`),
   clic derecho en `BiblioTech.Api` → **Administrar secretos de usuario** y pega:
   ```json
   { "ConnectionStrings": { "BibliotecaDb": "Server=.\\SQLEXPRESS;Database=BiblioTech;Trusted_Connection=True;TrustServerCertificate=True" } }
   ```


3. **Clave JWT (obligatoria, cada persona la suya):** en el mismo `secrets.json` agrega
   `"Jwt": { "Key": "una-clave-larga-de-minimo-32-caracteres-123456" }`, o desde una terminal
   en la carpeta `BiblioTech.Api`:
   ```
   dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-minimo-32-caracteres-123456"
   ```
   Sin esta clave la API no arranca y lo dice en el mensaje de error.


4. **Arrancar:** en Visual Studio, en el selector de inicio elegir **"Api + Web"** y pulsar ▶.
   - La Api abre **Scalar** en `https://localhost:7225/scalar`.
   - La Web abre en `https://localhost:7214`.


5. **Crear el bibliotecario (una sola vez):** en Scalar ejecutar `POST /api/auth/registrar-inicial`
   (o usar `BiblioTech.Api/BiblioTech.Api.http`). Solo funciona mientras la tabla `Usuarios` esté vacía.
   El ejemplo del `.http` usa `bibliotecario@bibliotech.com` / `Biblio2026!` (solo para desarrollo).


6. Iniciar sesión en la Web con ese usuario.

> Si vuelves a ejecutar `BiblioTech.sql`, la tabla `Usuarios` queda vacía: repite el paso 5.

## Reglas de negocio (se demuestran con los datos semilla)

| Caso | Resultado | Regla |
|---|---|---|
| Ana Gómez tiene 3 préstamos activos | 409 máximo 3 préstamos | R2 |
| Carlos Ruiz tiene un préstamo vencido | 409 tiene vencidos | R3 |
| "Test Driven Development" tiene 0 disponibles | 409 no hay ejemplares | R1 |
| Sofía Herrera está inactiva | 409 socio inactivo | R6 |
| Pedro Sánchez no tiene préstamos | 201 préstamo exitoso | camino feliz |

R4 (devolución = préstamo + 15 días) y R5 (transacción que resta/suma ejemplares) se ven en cada préstamo y devolución.

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/registrar-inicial` | Crea el primer bibliotecario (solo con `Usuarios` vacía) |
| POST | `/api/auth/login` | Devuelve el token JWT |
| GET/POST/PUT/DELETE | `/api/autores` | CRUD (409 al borrar un autor con libros) |
| GET/POST/PUT/DELETE | `/api/libros?buscar=` | CRUD con autores N:N y disponibilidad |
| GET/POST/PUT/DELETE | `/api/socios?soloActivos=` | CRUD; DELETE = desactivar (borrado lógico) |
| GET/POST | `/api/prestamos?estado=` | Listar (Activo/Devuelto/Vencido) y registrar (R1–R6) |
| PUT | `/api/prestamos/{id}/devolver` | Registrar devolución |
| GET | `/api/prestamos/vencidos` | Reporte desde la vista `vw_PrestamosVencidos` |
| GET | `/api/dashboard` | Indicadores desde `sp_ResumenDashboard` |

## Etiquetas (puntos seguros)

| Tag | Contenido |
|---|---|
| `esqueleto` | 3 proyectos, NuGet, carpetas, Scalar vacío. **Punto de partida del fork.** |
| `dia2-scaffold` | Scaffold (con pluralizador en español), clase partial, CRUD de Autores |
| `dia3-logica` | Libros, Socios, Préstamos (R1–R6), middleware de errores, JWT, CORS |
| `dia4-front` | Frontend Blazor completo |

## Regenerar el Scaffold

Consola del Administrador de paquetes, proyecto predeterminado `BiblioTech.Api`:

```
Scaffold-DbContext "Name=ConnectionStrings:BibliotecaDb" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context BibliotecaContext -NoOnConfiguring -Force
```

`Data/PluralizadorEspanol.cs` hace que las clases salgan como `Autor` y `Libro.Autores`
(el pluralizador por defecto es en inglés y generaba `Autore` y `Autors`).


