# Clynic - Agentes y Flujos del Sistema

## Descripción General

**Clynic** es un sistema de gestión de citas para clínicas y centros de salud privados (odontólogos, dermatólogos, etc.) diseñado como **SaaS multi-tenant**.

---

## Modelo de Negocio

- Se vende a múltiples clínicas/centros de salud
- Cada clínica tiene sus propios datos (usuarios, pacientes, citas, servicios)
- Los dueños/administradores gestionan su clínica
- Los pacientes pueden agendar citas (futuro)

---

## Roles del Sistema

| Rol | Descripción |
|-----|-------------|
| **Admin** | Dueño/administrador de la clínica. Puede gestionar usuarios, ver todas las citas, configurar servicios. |
| **Doctor** | Profesional de salud. Puede ver sus citas, historial de pacientes. |
| **Recepcionista** | Personal administrativo. Puede agendar citas, gestionar pacientes. |
| **Paciente** | (Futuro) Puede agendar sus propias citas, ver su historial. |

---

## Flujo de Autenticación

### 1. Registro Inicial
```
El sistema crea una clínica → Se crea el usuario Admin (dueño)
```

### 2. El Admin crea usuarios
```
Admin → Crea Doctores, Recepcionistas para su clínica
```

### 3. Login
```
Usuario → Ingresa credenciales → Recibe JWT → Accede al sistema
```

### 4. Recuperación de Contraseña
```
Usuario solicita cambio → Recibe código por email → Ingresa código → Nueva contraseña
```

---

## Módulos Implementados

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| Clínicas | ✅ Completo | CRUD de clínicas |
| Sucursales | ✅ Completo | CRUD de sucursales por clínica |
| Horarios | ✅ Completo | Horarios de atención por sucursal |
| Usuarios | ✅ Completo | CRUD con autenticación JWT |
| Auth | ✅ Completo | Login, Registro, JWT |
| Email | 🔄 En progreso | Envío de códigos de verificación |
| Pacientes | ⏳ Pendiente | Registro y gestión de pacientes |
| Citas | ⏳ Pendiente | Agendamiento y gestión |

---

## Políticas del Sistema

### Eliminación
- **SIEMPRE eliminación lógica** (Activo = false)
- Nunca se borran registros físicamente

### Contraseñas
- Mínimo 6 caracteres
- Hash con BCrypt (work factor 12)
- Recuperación mediante código de verificación por email

### JWT
- Expiración: 24 horas (configurable)
- Claims: Id, Nombre, Email, Rol, IdClinica

---

## Estructura de Base de Datos

### Entidades Principales
```
Clinica (1) ─── (N) Sucursal
     │
     ├── (N) Usuario
     ├── (N) Servicio
     ├── (N) Cita
     │
Sucursal (1) ─── (N) HorarioSucursal
              └── (N) Cita

Usuario (1) ─── (N) Cita (como Doctor)

Paciente (1) ─── (N) Cita

Cita (N) ─── (N) Servicio (a través de CitaServicio)
```

---

## Variables de Entorno y Secretos

### 🔐 User Secrets (Desarrollo Local)
Los secretos se almacenan localmente y **NO se suben a Git**:

```bash
# Configurar secrets (solo una vez por entorno)
cd Clynic.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;User Id=...;Password=...;"
dotnet user-secrets set "Jwt:SecretKey" "TuClaveSecretaMinimo32Caracteres"
dotnet user-secrets set "Email:Sender" "tuemail@gmail.com"
dotnet user-secrets set "Email:Password" "tu-app-password"

# Ver secrets configurados
dotnet user-secrets list
```

### Variables de Entorno (Producción/Docker)
```
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;
Jwt__SecretKey=TuClaveSecretaMinimo32Caracteres
Email__Sender=tuemail@gmail.com
Email__Password=tu-app-password
```

### Configuración No Sensible (appsettings.json)
```json
{
  "Jwt": { "Issuer": "ClynicAPI", "Audience": "ClynicClients", "ExpirationHours": 24 },
  "Email": { "Host": "smtp.gmail.com", "Port": 587 }
}
```

---

## Endpoints API

### Autenticación (Público)
```
POST /api/auth/register   → Registro de usuario
POST /api/auth/login      → Login
POST /api/auth/forgot-password → Solicitar código de recuperación
POST /api/auth/reset-password  → Cambiar contraseña con código
```

### Usuarios (Requiere Auth)
```
GET    /api/usuarios              → Listar (Admin)
GET    /api/usuarios/clinica/{id} → Por clínica
GET    /api/usuarios/{id}         → Obtener por ID
POST   /api/usuarios              → Crear (Admin)
PUT    /api/usuarios/{id}         → Actualizar
DELETE /api/usuarios/{id}         → Desactivar (Admin)
```

---

## Próximos Pasos

1. ✅ Completar servicio de email
2. ⏳ Implementar módulo de pacientes
3. ⏳ Implementar módulo de citas
4. ⏳ Dashboard y reportes
