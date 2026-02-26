# Flujo de Citas y Pacientes (Backend)

## Objetivo
Definir cómo funciona el módulo de **citas** y **pacientes** para un sistema multi-clínica, diferenciando:
- Cita creada por el **paciente** (canal público, formulario web/chatbot).
- Cita creada/gestionada por **recepcionista** o personal interno.

---

## Enfoque funcional implementado
Se implementó un modelo de **registro progresivo**:

1. **Primera interacción (cita)**: se capturan datos mínimos para agendar.
2. **Atención clínica**: se completa el perfil del paciente y el historial clínico.
3. **Consulta médica**: se registra diagnóstico, tratamiento, receta, exámenes y notas.

Esto evita fricción al agendar y permite construir el expediente completo después.

---

## Flujo A: Paciente agenda cita (público)
Endpoint: `POST /api/Citas/publica`

### Qué valida el backend
- La clínica existe (`IdClinica`).
- La sucursal pertenece a la clínica (`IdSucursal`).
- Los servicios existen y pertenecen a la clínica (`IdsServicios`).
- Fecha/hora de cita válida.

### Qué pasa al guardar
1. Busca paciente por `IdClinica + Correo`.
2. Si no existe, **crea paciente automáticamente**.
3. Si existe, actualiza datos básicos de contacto.
4. Crea la cita en estado `Pendiente`.
5. Calcula duración y montos (`SubTotal`, `TotalFinal`) desde servicios.
6. Envía **correo de confirmación** al paciente con fecha, horario y servicios.

### Datos mínimos actuales del endpoint público
- `IdClinica`
- `IdSucursal`
- `Nombres`
- `Apellidos`
- `Correo`
- `FechaHoraInicioPlan`
- `IdsServicios`

Opcionales: `Telefono`, `Notas`.

### Correo de confirmación automática
Al crear una cita pública, el sistema envía un correo con:
- Clínica y sucursal.
- Fecha y rango horario agendado.
- Lista de servicios a realizar.
- Notas adicionales de la cita.

> Nota: tu idea de “solo nombre, correo, fecha y hora” es válida como UX. En ese caso, el frontend puede mantener formulario mínimo y resolver internamente sucursal/servicio por defecto, o podemos crear un endpoint alterno ultra-minimal.

---

## Flujo B: Recepcionista/Admin gestiona cita
Endpoints principales:
- `GET /api/Citas/clinica/{idClinica}`
- `GET /api/Citas/{id}`
- `POST /api/Citas/interna`
- `PUT /api/Citas/{id}/doctor`

### Qué permite
- Ver agenda por clínica con filtros.
- Crear cita interna para segunda consulta o seguimiento.
- Asignar doctor a una cita.
- Cambiar estado operativo de la agenda por flujo (pendiente/confirmada/completada).

### Crear cita interna (recepción)
`POST /api/Citas/interna`

Uso recomendado:
- Paciente ya registrado en clínica.
- Nueva cita de seguimiento o nueva evaluación.
- Permite definir estado inicial (`Pendiente`, `Confirmada`, `Cancelada`) y notas.

Datos principales:
- `IdClinica`
- `IdSucursal`
- `IdPaciente`
- `IdDoctor` (opcional)
- `FechaHoraInicioPlan`
- `IdsServicios`
- `EstadoInicial`
- `Notas`

Control de tenant:
- Todo se restringe por `IdClinica` del token JWT.
- Se valida que **sucursal, paciente, doctor y servicios** pertenezcan a esa misma clínica.

---

## Flujo C: Atención médica y expediente
### 1) Registrar consulta médica (Doctor/Admin)
Endpoint: `POST /api/Citas/{id}/consulta`

Guarda:
- `Diagnostico`
- `Tratamiento`
- `Receta`
- `ExamenesSolicitados`
- `NotasMedicas`
- `FechaConsulta`
- `IdDoctor` que atendió (trazabilidad)

Efecto:
- Marca cita como `Completada`.
- Guarda tiempos reales si faltaban.

### 2) Completar historial clínico del paciente
Endpoint: `PUT /api/Pacientes/{id}/historial`

Guarda antecedentes persistentes:
- Enfermedades previas
- Medicamentos actuales
- Alergias
- Antecedentes familiares
- Observaciones

### 3) Actualizar datos personales del paciente
Endpoint: `PUT /api/Pacientes/{id}`

Permite actualizar:
- Nombres y apellidos
- Teléfono y correo
- Fecha de nacimiento

---

## Resumen de tablas involucradas
- `Paciente`: datos base del paciente por clínica.
- `Cita`: agenda y estado de la cita.
- `CitaServicio`: detalle de servicios de la cita.
- `HistorialClinico`: antecedentes generales del paciente (1:1).
- `ConsultaMedica`: resultado clínico de una cita (1:1 por cita).

---

## Orden recomendado de uso en producto
1. Formulario público mínimo para generar cita.
2. Recepción confirma y asigna doctor.
3. Doctor registra consulta.
4. Personal clínico completa historial y datos faltantes.
5. Reutilizar paciente en próximas citas por `correo + clínica`.

---

## Vistas frontend implementadas

### 1) Portal público de agendamiento
- Ruta: `/agendar-cita`
- Sin autenticación.
- Flujo:
	1. Cargar catálogo público por clínica (`GET /api/Citas/publica/catalogo/{idClinica}`).
	2. Completar formulario.
	3. Agendar (`POST /api/Citas/publica`).
	4. Recibir correo de confirmación automática.

### 2) Vista interna de citas (roles)
- Ruta: `/appointment`
- **Recepcionista/Admin**:
	- Crear cita interna (`POST /api/Citas/interna`) para seguimiento o nueva consulta.
	- Asignar doctor (`PUT /api/Citas/{id}/doctor`).
- **Doctor/Admin**:
	- Registrar consulta médica (`POST /api/Citas/{id}/consulta`).
- **Recepcionista** no ve formulario de consulta médica.

### 3) Vista de pacientes
- Ruta: `/patients`
- **Recepcionista/Admin/Doctor**:
	- Ver listado y editar datos personales.
- **Doctor/Admin**:
	- Editar historial clínico.
- **Recepcionista** no ve edición de historial clínico.

---

## Controles multi-clínica (tenant)
- En backend, las operaciones internas validan `IdClinica` del token.
- En creación interna de citas se valida consistencia entre clínica y:
	- sucursal,
	- paciente,
	- doctor,
	- servicios.
- Esto evita mezcla de datos entre centros de atención distintos.

---

## Consideraciones para siguiente iteración
- Regla anti-solapamiento de citas por doctor/sucursal.
- Recordatorio previo por correo/WhatsApp (24h y 2h antes).
- Endpoint público ultra-minimal (si deseas exactamente 4 campos).
- Catálogo de motivo de consulta y triage inicial.
