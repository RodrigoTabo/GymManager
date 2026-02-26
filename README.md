# GymManager

Aplicación web para la gestión básica de un gimnasio (V1 en progreso), con arquitectura separada:
**Client (Blazor WebAssembly + MudBlazor)** + **API REST (ASP.NET Core)** + **Shared (contratos/DTOs)**.

---

## Stack

- .NET / ASP.NET Core Web API
- Blazor WebAssembly (Client)
- MudBlazor (UI)
- Swagger / OpenAPI
- (Próximamente) EF Core + SQL Server
- (Próximamente) Docker + Azure

---

## Estructura del repositorio

- `GymManager.Client` → UI (Blazor WASM + MudBlazor)
- `GymManager.Api` → API REST (ASP.NET Core)
- `GymManager.Shared` → Contratos compartidos (DTOs, requests/responses, modelos comunes)

---

## URLs (Development)

- **Client (UI):** `https://localhost:7083/` (HTTP: `http://localhost:5269/`)
- **API:** `https://localhost:7093/` (HTTP: `http://localhost:5239/`)
- **Swagger:** `https://localhost:7093/swagger`

---

## Ejecutar en local (2 terminales)

> Se levantan **dos apps distintas**: el Client (UI) y la API (backend).  
> Cada `dotnet run` queda ejecutándose, por eso se usan dos terminales.

### Terminal 1 — API

```bash
cd GymManager.Api
dotnet run --launch-profile https
```

- **Verificación rápida:**
- Swagger: https://localhost:7093/swagger
- Endpoint de ejemplo: https://localhost:7093/WeatherForecast

### Terminal 2 — Client (Frontend)

```bash
cd GymManager.Client
dotnet run --launch-profile https
```

**Verificación rápida:**

- `https://localhost:7083/`

### Prueba rápida de conectividad

El Client consume el endpoint de ejemplo del API:

- `GET /WeatherForecast`

Si todo está OK:

- Swagger: `https://localhost:7093/swagger`
- Navegador (directo): `https://localhost:7093/WeatherForecast`

### Notas de arquitectura

- `GymManager.Shared` contiene **DTOs/contratos** compartidos entre Client y API.
- Las **entidades** y la **persistencia** (EF Core, DbContext, migraciones) viven solo en `GymManager.Api`.

### Roadmap (V1)

- Socios: alta / edición / listado + búsqueda
- Pagos: registrar pago + cálculo de vencimiento
- Asistencias: check-in + validación de estado
- Dashboard básico
