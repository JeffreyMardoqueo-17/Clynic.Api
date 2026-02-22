# Buenas prácticas, SOLID y patrones en Clynic (Testing + Arquitectura)

Este documento resume **las buenas prácticas aplicadas**, **su justificación técnica** y **su relación con SOLID y patrones de diseño** dentro de la solución.

## 1) Buenas prácticas que ya se aplicaron

### 1.1 Estructura de pruebas por capa

Se separaron los tests por responsabilidad:

- `test/Clynic.Domain.Tests` → pruebas unitarias de entidades/reglas puras.
- `test/Clynic.Application.Tests` → unitarias e integración del caso de uso/servicios.
- `test/Clynic.Infrastructure.Tests` → integración con persistencia (EF Core InMemory).
- `test/Clynic.Api.Tests` → E2E/funcionales de endpoints HTTP.

**Beneficio:** reduce acoplamiento, facilita mantenimiento y localización de errores.

---

### 1.2 Alineación de framework y dependencias

Todos los proyectos de test usan `net8.0` para estar alineados con la API y evitar incompatibilidades.

**Beneficio:** builds más estables y menos errores por versión.

---

### 1.3 Pruebas representativas por tipo

- **Unit:** validan comportamiento de una clase aislada.
- **Integration:** validan colaboración real entre componentes (servicio + repositorio + EF).
- **E2E:** validan comportamiento observable desde HTTP.

**Beneficio:** cobertura equilibrada del sistema, desde lo micro hasta lo macro.

---

### 1.4 Configuración de E2E sin dependencia dura de entorno

Se usó `WebApplicationFactory<Program>` y se inyectó configuración de testing para levantar la API en pruebas.

**Beneficio:** pruebas E2E reproducibles y ejecutables en local/CI.

---

### 1.5 Documentación viva

Se documentó el flujo, comandos y ejemplos en `docs/Test/README.md` y ahora este complemento de buenas prácticas.

**Beneficio:** onboarding más rápido y estandarización del equipo.

---

## 2) Principios SOLID aterrizados al proyecto

## S — Single Responsibility Principle

Cada clase debe tener una sola razón de cambio.

Ejemplos en Clynic:
- `ClinicasController` se enfoca en HTTP.
- `ClinicaService` se enfoca en casos de uso.
- `ClinicaRepository` se enfoca en persistencia.
- `CreateClinicaDtoValidator` se enfoca en validar entrada.

---

## O — Open/Closed Principle

El código debe abrirse a extensión, cerrarse a modificación.

Ejemplo en Clynic:
- Es posible agregar nuevas reglas o validadores sin reescribir la lógica base del controlador.
- Es posible introducir otro repositorio (por ejemplo, otro motor de DB) implementando interfaces existentes.

---

## L — Liskov Substitution Principle

Una implementación concreta debe poder reemplazar su abstracción sin romper comportamiento esperado.

Ejemplo en Clynic:
- `IClinicaRepository` puede ser reemplazado por un fake/in-memory en tests o por uno real en producción.

---

## I — Interface Segregation Principle

Mejor interfaces pequeñas y específicas que interfaces gigantes.

Ejemplo en Clynic:
- `IClinicaService` y `IClinicaRepository` exponen contratos concretos por módulo en lugar de una interfaz global monolítica.

---

## D — Dependency Inversion Principle

Las capas de alto nivel dependen de abstracciones, no de detalles concretos.

Ejemplo en Clynic:
- `ClinicaService` depende de `IClinicaRepository` e `IValidator<CreateClinicaDto>`, no de `ClinicaRepository` directamente.
- La implementación concreta se decide en DI (`Program.cs`).

---

## 3) Cómo funciona el flujo de pruebas

## 3.1 Pirámide de tests (recomendación)

- Muchas **unit tests** (rápidas y baratas).
- Menos **integration tests** (más realistas, más lentas).
- Pocas **E2E tests** (críticas de punta a punta).

Idea práctica: mantener un balance aproximado 70/20/10 (unit/integration/E2E).

---

## 3.2 Flujo operativo sugerido

1. Crear/ajustar unit tests del caso de uso.
2. Agregar integration tests del repositorio/servicio.
3. Cerrar con E2E de endpoints críticos.
4. Ejecutar `dotnet test API.slnx` en cada cambio importante.

---

## 4) Patrones de diseño relevantes

## 4.1 Singleton

### Qué es
Una única instancia compartida durante toda la vida de la aplicación.

### Dónde aparece en .NET
Con DI, cuando se registra un servicio como `AddSingleton<T>()`.

### Cuándo usarlo
- Configuración inmutable
- Cachés compartidos
- Servicios sin estado costoso de construir

### Riesgos
- Estado global mutable (puede generar bugs por concurrencia)
- Dificulta pruebas si guarda estado entre ejecuciones

### Recomendación
Preferir servicios `Scoped` para lógica de negocio y usar Singleton solo cuando haya motivo claro de performance/compartición.

---

## 4.2 Factory

### Qué es
Un patrón para centralizar/encapsular la creación de objetos.

### Dónde aparece en este proyecto
- `WebApplicationFactory<Program>` en tests E2E es un ejemplo concreto de Factory para construir el host HTTP de prueba.
- El contenedor de DI también actúa como fábrica de dependencias.

### Beneficios
- Evita `new` dispersos por todo el código.
- Permite cambiar construcción sin tocar consumidores.
- Mejora testabilidad.

---

## 4.3 Repository

### Qué es
Abstrae acceso a datos detrás de un contrato.

### Aplicación en Clynic
- `IClinicaRepository` + `ClinicaRepository`.

### Beneficio
- Aísla SQL/EF de Application.
- Permite reemplazo por fakes en pruebas.

---

## 5) Reglas prácticas para mantener calidad

- Usar nombres de prueba descriptivos: `Metodo_Escenario_ResultadoEsperado`.
- Una prueba debe verificar un comportamiento concreto.
- Evitar datos mágicos repetidos: usar builders/factories de objetos de prueba cuando el módulo crezca.
- No mezclar responsabilidades: el controlador no debe contener lógica de negocio.
- Mantener los tests deterministas (sin depender de hora/zona/estado externo no controlado).
- Ejecutar tests en CI en cada pull request.

---

## 6) Checklist rápido para nuevos módulos (ej. Sucursales)

1. Crear interfaces de repositorio/servicio en Application.
2. Implementar servicio con validación y reglas.
3. Implementar repositorio en Infrastructure.
4. Exponer endpoints en API.
5. Añadir unit tests (Domain/Application).
6. Añadir integration tests (Infrastructure/Application).
7. Añadir E2E mínimos de endpoints críticos.
8. Documentar casos clave en `docs/Test`.

---

## 7) Resumen ejecutivo

La implementación actual de tests en Clynic ya sigue una base sólida:
- Separación por capas
- Uso de contratos (interfaces)
- Pruebas por nivel de confianza
- Configuración de E2E reproducible

Esto está alineado con SOLID y patrones clásicos (Repository + Factory, con posible uso controlado de Singleton), y deja al proyecto listo para crecer por módulos sin perder mantenibilidad.
