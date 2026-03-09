# Concurrencia de Citas: Unit of Work + capacidad por doctores

## Problema
En escenarios de alta concurrencia (dos o más usuarios reservando al mismo tiempo en la misma clínica/sucursal), podía ocurrir una carrera:

1. Proceso A valida disponibilidad.
2. Proceso B valida disponibilidad casi al mismo tiempo.
3. Ambos intentan guardar y terminan reservando el mismo horario.

## Objetivo
Garantizar que la agenda respete la capacidad real de cada sucursal según la cantidad de doctores activos, devolviendo horarios disponibles claros para pacientes y recepción.

## Patrones aplicados

### 1) Unit of Work
Se agregó `IUnitOfWork` para encapsular operaciones críticas dentro de una transacción explícita:

- Archivo: `Clynic.Application/Interfaces/Repositories/IUnitOfWork.cs`
- Implementación: `Clynic.Infrastructure/Data/UnitOfWork.cs`

La implementación usa:
- `Database.CreateExecutionStrategy()` (reintentos seguros de EF Core).
- `BeginTransactionAsync(...)` con nivel de aislamiento configurable.
- `Commit`/`Rollback` centralizados.

### 2) Verificación optimista (pre-check)
Antes de insertar una cita, se ejecuta una consulta de solape de rango horario:

`inicioExistente < finNueva && finExistente > inicioNueva`

Ignorando estados no activos:
- `Cancelada`
- `Completada`

Método agregado en repositorio:
- `ICitaRepository.ExisteTraslapeHorarioAsync(...)`
- Implementación en `CitaRepository`.

### 3) Capacidad dinámica por sucursal (doctores activos)
La validación de disponibilidad pasó de “1 cita por horario” a “hasta N citas por solape”, donde `N = doctores activos de la sucursal`.

Regla aplicada en creación (pública e interna):

- Se calcula duración estimada total de la cita en base a servicios.
- Se cuenta cuántas citas activas se solapan con la ventana solicitada.
- Si `citasSolapadas >= doctoresActivos`, se rechaza con conflicto.

Para citas internas con doctor seleccionado, además se valida que ese doctor no tenga solape en el mismo rango.

> Estados que **ocupan capacidad**: todos excepto `Cancelada` y `Completada`.

## Flujo final de creación de cita
Aplica tanto para:
- `CrearPublicaAsync`
- `CrearInternaAsync`

Pasos:
1. Inicia `UnitOfWork` con `IsolationLevel.Serializable`.
2. Calcula ventana de cita (inicio/fin).
3. Calcula capacidad disponible de la sucursal (`doctoresActivos`).
4. Cuenta solapes activos para la ventana solicitada.
5. Si no hay cupo, lanza `InvalidOperationException` con mensaje de negocio.
6. Inserta cita.
7. Commit.

## Endpoint de horarios disponibles

Se agregó endpoint público para que frontend no use hora libre manual:

- `GET /api/Citas/publica/horarios-disponibles`
- Query params:
	- `idClinica`
	- `idSucursal`
	- `fecha` (día objetivo)
	- `idsServicios` (repetible en query)
	- `intervaloMin` (opcional, default `30`)

Respuesta principal:

- `duracionEstimadaMin`
- `cantidadDoctoresActivos`
- `horarios[]` con:
	- `fechaHoraInicioPlan`
	- `fechaHoraFinPlan`
	- `horaLabel` (ej. `08:30`, `09:00`)

El cálculo usa:

- Horario de sucursal por día de semana.
- Citas existentes de ese día en esa sucursal.
- Ventanas discretas por intervalo (`30 min` por defecto).

## Mensaje devuelto al frontend
Cuando no hay cupo para el rango solicitado, se retorna:

`No hay cupos disponibles para ese horario en esta sucursal. Selecciona otro horario.`

El middleware de excepciones lo mapea como `409 Conflict` (por `InvalidOperationException`).

## Beneficio
Con esta combinación:
- **Unit of Work transaccional**
- **validación por capacidad de doctores**
- **endpoint de horarios sugeridos**

se evita sobre-reserva de agenda, se permite paralelismo real cuando hay más doctores, y se simplifica la selección de horario para pacientes/recepción.