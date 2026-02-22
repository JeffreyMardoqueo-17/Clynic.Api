# Guía de Pruebas con xUnit (Módulo Clínicas)

Este documento describe cómo está configurado el testing en Clynic y el flujo recomendado para construir pruebas de **Unit**, **Integration** y **E2E**.

## Estructura de proyectos de prueba

```text
└── test
    ├── Clynic.Domain.Tests         # Unit tests de entidades de dominio
    ├── Clynic.Application.Tests    # Unit + Integration tests de servicios
    ├── Clynic.Infrastructure.Tests # Integration tests de repositorios (EF)
    └── Clynic.Api.Tests            # E2E / Functional tests de endpoints HTTP
```

## Paquetes instalados

Cada proyecto de test usa:
- `xunit`
- `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`
- `coverlet.collector` (coverage)

Adicionalmente:
- `Clynic.Infrastructure.Tests` → `Microsoft.EntityFrameworkCore.InMemory`
- `Clynic.Application.Tests` → `Microsoft.EntityFrameworkCore.InMemory`
- `Clynic.Api.Tests` → `Microsoft.AspNetCore.Mvc.Testing`

## Referencias por proyecto

- `Clynic.Domain.Tests` referencia `Clynic.Domain`
- `Clynic.Application.Tests` referencia `Clynic.Application`, `Clynic.Infrastructure`, `Clynic.Domain`
- `Clynic.Infrastructure.Tests` referencia `Clynic.Infrastructure`, `Clynic.Application`, `Clynic.Domain`
- `Clynic.Api.Tests` referencia `Clynic.Api`

## Flujo recomendado de desarrollo de pruebas

Para el módulo de Clínicas:

1. **Integration (Infrastructure)**
   - Validar persistencia real con EF Core InMemory.
   - Ejemplo: `ClinicaRepository.CrearAsync` y `ObtenerTodasAsync`.

2. **Unit (Domain/Application)**
   - Probar reglas y comportamiento de entidades/servicios sin infraestructura real.
   - Ejemplo: valores por defecto de `Clinica`, validaciones del `ClinicaService`.

3. **Integration (Application)**
   - Probar servicio + reglas + validador real + repositorio InMemory.
   - Ejemplo: `ClinicaService` creando clínica con `CreateClinicaDtoValidator`.

4. **E2E (API)**
   - Probar endpoint HTTP completo con `WebApplicationFactory`.
   - Ejemplo: `GET /api/health/version` devolviendo 200.

## Ejemplo mínimo por tipo

### 1) Unit test (Domain)

- Archivo: `test/Clynic.Domain.Tests/UnitTest1.cs`
- Caso: `NuevaClinica_DebeIniciarConValoresPorDefectoEsperados`
- Valida:
  - `Activa = true`
  - Colecciones inicializadas

### 2) Integration test (Infrastructure)

- Archivo: `test/Clynic.Infrastructure.Tests/UnitTest1.cs`
- Caso: `CrearAsync_DebePersistirClinicaActiva`
- Usa:
  - `ClynicDbContext` con `UseInMemoryDatabase`
  - `ClinicaRepository`

### 3) Integration test (Application)

- Archivo: `test/Clynic.Application.Tests/UnitTest1.cs`
- Caso: `Integracion_ClinicaServiceConValidatorReal_DebePersistirEnInMemory`
- Usa:
  - `ClinicaService`
  - `CreateClinicaDtoValidator`
  - `ClinicaRules`
  - `ClinicaRepository` + InMemory

### 4) E2E test (API)

- Archivo: `test/Clynic.Api.Tests/UnitTest1.cs`
- Caso: `GetVersion_DebeResponder200YNombreApi`
- Usa:
  - `WebApplicationFactory<Program>`
  - Cliente HTTP real contra la API en ambiente `Testing`

## Comandos útiles

Ejecutar todo:

```bash
dotnet test API.slnx
```

Ejecutar por proyecto:

```bash
dotnet test test/Clynic.Domain.Tests/Clynic.Domain.Tests.csproj
dotnet test test/Clynic.Application.Tests/Clynic.Application.Tests.csproj
dotnet test test/Clynic.Infrastructure.Tests/Clynic.Infrastructure.Tests.csproj
dotnet test test/Clynic.Api.Tests/Clynic.Api.Tests.csproj
```

Coverage básico:

```bash
dotnet test API.slnx --collect:"XPlat Code Coverage"
```

## Notas importantes

- Todos los proyectos de test están en `net8.0`, igual que la API.
- Para E2E se agregó `public partial class Program;` al final de `Clynic.Api/Program.cs`.
- El endpoint usado en E2E (`/api/health/version`) no depende de conexión activa a DB, ideal para smoke test inicial.

## Lectura recomendada

- Ver guía complementaria: `docs/Test/BUENAS-PRACTICAS-SOLID-Y-PATRONES.md`
