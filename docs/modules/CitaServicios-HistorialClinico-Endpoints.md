# Módulo CitaServicio + Historial Clínico (Backend)

## Objetivo
Agregar endpoints dedicados para:
- Gestionar detalle de servicios por cita (`CitaServicio`).
- Gestionar historial clínico del paciente (`HistorialClinico`).

> Nota: Se mantienen los endpoints existentes de `Citas` y `Pacientes` para compatibilidad.

---

## 1) CitaServiciosController
Base route: `api/CitaServicios`

### `GET /api/CitaServicios/cita/{idCita}`
Obtiene todos los servicios asociados a una cita.

**Roles:** `Admin`, `Doctor`, `Recepcionista`

**Validaciones principales:**
- La cita existe.
- El `IdClinica` del token coincide con el de la cita.
- Si no es `Admin`, también debe coincidir `IdSucursal` del token.

### `POST /api/CitaServicios`
Agrega un servicio a una cita existente.

**Roles:** `Admin`, `Recepcionista`

**Body:**
```json
{
  "idCita": 0,
  "idServicio": 0,
  "duracionMin": 0,
  "precio": 0
}
```

**Reglas de negocio:**
- No permite agregar servicios a citas `Cancelada` o `Completada`.
- El servicio debe existir, estar activo y pertenecer a la misma clínica de la cita.
- Evita duplicar el mismo servicio dentro de la cita.
- Recalcula automáticamente `SubTotal`, `TotalFinal` y `FechaHoraFinPlan` de la cita.

### `DELETE /api/CitaServicios/{id}`
Elimina un detalle de servicio de la cita.

**Roles:** `Admin`, `Recepcionista`

**Reglas de negocio:**
- No permite eliminar servicios en citas `Cancelada` o `Completada`.
- Recalcula automáticamente montos y duración planificada al eliminar.

---

## 2) HistorialesClinicosController
Base route: `api/HistorialesClinicos`

### `GET /api/HistorialesClinicos/paciente/{idPaciente}`
Obtiene el historial clínico del paciente.

**Roles:** `Admin`, `Doctor`

**Validaciones principales:**
- El paciente existe.
- El `IdClinica` del token coincide con la clínica del paciente.

### `PUT /api/HistorialesClinicos/paciente/{idPaciente}`
Crea o actualiza (upsert) el historial clínico del paciente.

**Roles:** `Admin`, `Doctor`

**Body:**
```json
{
  "enfermedadesPrevias": "",
  "medicamentosActuales": "",
  "alergias": "",
  "antecedentesFamiliares": "",
  "observaciones": ""
}
```

**Reglas de negocio:**
- Si no existe historial, lo crea.
- Si ya existe, lo actualiza manteniendo el registro.
- Actualiza `FechaActualizacion` en cada modificación.

---

## Flujo recomendado
1. Crear cita (`/api/Citas/publica` o `/api/Citas/interna`).
2. Cambiar estado operativo según atención (`/api/Citas/{id}/estado`).
3. Agregar o ajustar servicios de la cita (`/api/CitaServicios`).
4. Registrar consulta médica (`/api/Citas/{id}/consulta`).
5. Actualizar historial clínico (`/api/HistorialesClinicos/paciente/{idPaciente}`).
