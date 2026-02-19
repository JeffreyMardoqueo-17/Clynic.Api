# Arquitectura en Flujo

```
Cliente → API → Application → Domain
                     ↓
               Infrastructure → BD
```

---

# Traducción a Responsabilidades Reales

| Capa            | Rol real        | Qué contiene                     |
|-----------------|-----------------|----------------------------------|
| API             | Entrada / HTTP  | Controllers, Swagger             |
| Application     | Cerebro         | Casos de uso                     |
| Domain          | Negocio puro    | Entidades                        |
| Infrastructure  | Manos           | BD, repositorios                 |
| Docker          | Infra           | Contenedores                     |
| Docs            | Conocimiento    | Decisiones técnicas              |

---

# Descripción General

Esta estructura representa una arquitectura por capas orientada a dominio (DDD‑inspired), donde:

- **API** expone los endpoints HTTP.
- **Application** orquesta los casos de uso.
- **Domain** contiene la lógica de negocio pura.
- **Infrastructure** implementa accesos externos (BD, servicios).
- **Docker** gestiona la infraestructura contenerizada.
- **Docs** centraliza decisiones y conocimiento técnico.


```
└── 📁API
    └── 📁Clynic.Api   → CAPA DE PRESENTACIÓN / ENTRYPOINT
        ├── Responsabilidad:
        │   - Exponer endpoints HTTP
        │   - Manejar requests/responses
        │   - Autenticación, CORS, Swagger
        │   - Orquestar servicios de Application
        │
        └── 📁Controllers
            ├── ClinicasController.cs
            │   → Endpoints REST de Clínicas
            │   → Llama a Services (Application)
            │
            ├── HealthController.cs
            │   → Endpoint /health para monitoreo Docker/K8s
            │
            ├── WeatherForecastController.cs
            │   → Demo default (no pertenece al dominio)

        └── 📁Properties
            ├── launchSettings.json
            │   → Configuración de ejecución local

        ├── appsettings.json
        │   → Configuración (ConnectionStrings, logs, etc.)

        ├── Program.cs
        │   → Configuración global:
        │     - DI (Inyección de dependencias)
        │     - EF Core
        │     - Swagger
        │     - HealthChecks
        │     - Middleware pipeline

        ├── Dockerfile
        │   → Imagen de ejecución de la API

    ─────────────────────────────────────────────

    └── 📁Clynic.Application   → CAPA DE LÓGICA DE NEGOCIO (BUSINESS LOGIC)
        ├── Responsabilidad:
        │   - Casos de uso del sistema
        │   - Reglas de negocio
        │   - Validaciones
        │   - Contratos (interfaces)
        │   - Orquestación del dominio
        │
        │   ⚠ NO accede directo a BD
        │   ⚠ NO sabe de EF ni SQL
        │
        └── 📁DTOs
            └── 📁Clinicas
                ├── CreateClinicaDto.cs
                │   → Datos de entrada (Request)
                │
                ├── ClinicaResponseDto.cs
                │   → Datos de salida (Response)

        └── 📁Interfaces   → CONTRATOS (PUERTOS)
            └── 📁Repositories
                ├── IClinicaRepository.cs
                │   → Contrato de acceso a datos
                │   → Define qué operaciones existen
                │
            └── 📁Services
                ├── IClinicaService.cs
                │   → Contrato de lógica de negocio

        └── 📁Rules
            ├── ClinicaRules.cs
            │   → Reglas de dominio:
            │     - No duplicados
            │     - Validaciones críticas
            │     - Restricciones de negocio

        └── 📁Services
            ├── ClinicaService.cs
            │   → Implementa casos de uso
            │   → Usa repositorios
            │   → Aplica reglas

        └── 📁Validators
            ├── CreateClinicaDtoValidator.cs
            │   → Validaciones FluentValidation
            │   → Capa de entrada (antes de negocio)

    ─────────────────────────────────────────────

    └── 📁Clynic.Domain   → CAPA DE DOMINIO (CORE DEL NEGOCIO)
        ├── Responsabilidad:
        │   - Entidades del sistema
        │   - Modelos de negocio puros
        │   - Reglas invariantes
        │
        │   ⚠ No depende de nadie
        │   ⚠ No sabe de BD ni APIs
        │
        └── 📁Models
            ├── Clinica.cs
            │   → Entidad principal del dominio
            │
            ├── Sucursal.cs
            │   → Relación clínica-sucursal
            │
            ├── Usuario.cs
            │   → Usuarios del sistema

            └── 📁Enums
                → Enumeraciones del dominio

    ─────────────────────────────────────────────

    └── 📁Clynic.Infrastructure   → CAPA DE ACCESO A DATOS
        ├── Responsabilidad:
        │   - Persistencia
        │   - EF Core / SQL Server
        │   - Implementación de repositorios
        │
        │   Implementa interfaces de Application
        │
        └── 📁Data
            ├── ClynicDbContext.cs
            │   → Configuración EF Core
            │   → DbSets
            │   → Mapeos

        └── 📁Repositories
            ├── ClinicaRepository.cs
            │   → Implementa IClinicaRepository
            │   → Queries SQL / LINQ

    ─────────────────────────────────────────────

    └── 📁docker   → INFRAESTRUCTURA DE CONTENEDORES
        └── 📁sqlserver
            ├── Dockerfile
            │   → Imagen personalizada SQL Server
            │
            ├── entrypoint.sh
            │   → Script de arranque
            │   → Espera SQL + ejecuta scripts
            │
            ├── init.sql
            │   → Creación inicial BD
            │
            ├── schema.sql
            │   → Tablas y relaciones
            │
            ├── seed-data.sql
            │   → Datos de prueba

    ─────────────────────────────────────────────

    └── 📁docs   → DOCUMENTACIÓN TÉCNICA
        └── 📁modules
            ├── RESUMEN-IMPLEMENTACION-CLINICA.md
            │   → Explicación del módulo clínico

        ├── Orden-de-Desarrollo-Clinica.md
        │   → Roadmap de implementación

        ├── SCRIPTS-PRUEBA.md
        │   → Queries de testing

        ├── Structure.md
        │   → Explicación de arquitectura

    ─────────────────────────────────────────────

    └── 📁test
        ├── Pruebas unitarias / integración (pendiente o en progreso)

    ─────────────────────────────────────────────

    ├── compose.yaml
    │   → Orquestación de contenedores:
    │     - API
    │     - SQL Server
    │     - Redes
    │     - Volúmenes

    ├── docker.sh
    │   → Scripts de automatización Docker

    ├── API.slnx
    │   → Solución .NET que agrupa proyectos

    ├── README.md
    │   → Documentación general del sistema

    ├── .dockerignore
    │   → Archivos excluidos del build Docker

    ├── .gitignore
    │   → Archivos excluidos de Git

```
