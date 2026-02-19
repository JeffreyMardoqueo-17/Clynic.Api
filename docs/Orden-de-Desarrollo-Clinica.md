# 📋 Orden de Desarrollo - Módulo Clínica (Clean Architecture)

Este documento describe el orden correcto para desarrollar un módulo completo en Clean Architecture, usando el módulo de **Clínica** como ejemplo.

---

## 🎯 Flujo de Desarrollo

### **1️⃣ CAPA DE DOMINIO (Domain Layer)**

#### 📝 Paso 1: Crear el Modelo/Entidad
- **Ubicación**: `Clynic.Domain/Models/`
- **Archivo**: `Clinica.cs`
- **Descripción**: Define la entidad del dominio con sus propiedades básicas.
- **Contenido**:
  - Propiedades de la entidad (Id, Nombre, Telefono, Direccion, Activa, FechaCreacion)
  - Sin lógica de negocio
  - Solo propiedades y tipos de datos

```csharp
// Ejemplo: Clinica.cs
public class Clinica
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    // ... otras propiedades
}
```

---

### **2️⃣ CAPA DE INFRAESTRUCTURA (Infrastructure Layer)**

#### 🗄️ Paso 2: Configurar DbContext
- **Ubicación**: `Clynic.Infrastructure/Data/`
- **Archivo**: `ClynicDbContext.cs`
- **Descripción**: Agregar el DbSet y la configuración de la entidad.
- **Acciones**:
  - Agregar `DbSet<Clinica> Clinicas`
  - Configurar la entidad en `OnModelCreating()`
  - Definir:
    - Nombre de tabla
    - Clave primaria
    - Límites de longitud
    - Valores por defecto
    - Relaciones (si hay)

```csharp
// Ejemplo configuración
modelBuilder.Entity<Clinica>(entity =>
{
    entity.ToTable("Clinicas");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
});
```

---

### **3️⃣ CAPA DE APLICACIÓN (Application Layer)**

#### 🔌 Paso 3: Crear Interfaz del Repositorio
- **Ubicación**: `Clynic.Application/Interfaces/Repositories/`
- **Archivo**: `IClinicaRepository.cs`
- **Descripción**: Define el contrato para operaciones de base de datos.
- **Métodos típicos**:
  - `ObtenerTodasAsync()`
  - `ObtenerPorIdAsync(int id)`
  - `CrearAsync(Clinica clinica)`
  - `ActualizarAsync(Clinica clinica)`
  - `EliminarAsync(int id)`
  - `ExisteNombreAsync(string nombre)`
  - `ExisteAsync(int id)`

---

### **4️⃣ CAPA DE INFRAESTRUCTURA (Infrastructure Layer)**

#### 💾 Paso 4: Implementar el Repositorio
- **Ubicación**: `Clynic.Infrastructure/Repositories/`
- **Archivo**: `ClinicaRepository.cs`
- **Descripción**: Implementa la interfaz usando Entity Framework Core.
- **Contenido**:
  - Inyección del DbContext
  - Implementación de todos los métodos de la interfaz
  - Uso de LINQ y Entity Framework
  - Manejo de consultas asíncronas
  - Eliminación lógica (soft delete)

```csharp
// Ejemplo
public async Task<Clinica> CrearAsync(Clinica clinica)
{
    await _context.Clinicas.AddAsync(clinica);
    await _context.SaveChangesAsync();
    return clinica;
}
```

---

### **5️⃣ CAPA DE APLICACIÓN (Application Layer)**

#### 📦 Paso 5: Crear los DTOs (Data Transfer Objects)
- **Ubicación**: `Clynic.Application/DTOs/Clinicas/`
- **Archivos**:
  - `CreateClinicaDto.cs` - Para crear
  - `UpdateClinicaDto.cs` - Para actualizar (opcional)
  - `ClinicaResponseDto.cs` - Para respuestas
- **Descripción**: Objetos para transferencia de datos entre capas.
- **Contenido**: Solo propiedades, sin lógica.

```csharp
// Ejemplo CreateClinicaDto.cs
public class CreateClinicaDto
{
    public string Nombre { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
}
```

---

#### 📏 Paso 6: Crear Reglas de Negocio
- **Ubicación**: `Clynic.Application/Rules/`
- **Archivo**: `ClinicaRules.cs`
- **Descripción**: Contiene la lógica de reglas de negocio reutilizables.
- **Contenido**:
  - Validaciones de negocio
  - Verificaciones complejas
  - Lógica que puede ser compartida
  - Ejemplo: validar que el nombre sea único, formato de teléfono, etc.

```csharp
// Ejemplo
public async Task<bool> NombreEsUnicoAsync(string nombre, int? idExcluir = null)
{
    var existe = await _repository.ExisteNombreAsync(nombre, idExcluir);
    return !existe;
}
```

---

#### ✅ Paso 7: Crear Validadores (FluentValidation)
- **Ubicación**: `Clynic.Application/Validators/`
- **Archivo**: `CreateClinicaDtoValidator.cs`
- **Descripción**: Validaciones de los DTOs usando FluentValidation.
- **Paquete necesario**: `FluentValidation` (instalarlo en Clynic.Application)
- **Contenido**:
  - Hereda de `AbstractValidator<TDto>`
  - Define reglas de validación
  - Puede usar las reglas de negocio

```csharp
// Ejemplo
public class CreateClinicaDtoValidator : AbstractValidator<CreateClinicaDto>
{
    public CreateClinicaDtoValidator(ClinicaRules rules)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MinimumLength(3).WithMessage("Mínimo 3 caracteres");
    }
}
```

**Instalar paquete**:
```bash
cd Clynic.Application
dotnet add package FluentValidation
```

---

#### 🔌 Paso 8: Crear Interfaz del Servicio
- **Ubicación**: `Clynic.Application/Interfaces/Services/`
- **Archivo**: `IClinicaService.cs`
- **Descripción**: Define el contrato para la lógica de negocio.
- **Métodos**: Operaciones de alto nivel que usa el Controller.
- **Trabaja con DTOs**, no con entidades directamente.

```csharp
// Ejemplo
public interface IClinicaService
{
    Task<IEnumerable<ClinicaResponseDto>> ObtenerTodasAsync();
    Task<ClinicaResponseDto?> ObtenerPorIdAsync(int id);
    Task<ClinicaResponseDto> CrearAsync(CreateClinicaDto createDto);
}
```

---

#### 🎯 Paso 9: Implementar el Servicio
- **Ubicación**: `Clynic.Application/Services/`
- **Archivo**: `ClinicaService.cs`
- **Descripción**: Implementa la lógica de negocio.
- **Responsabilidades**:
  - Orquestar llamadas al repositorio
  - Aplicar reglas de negocio
  - Validar con FluentValidation
  - Mapear entre entidades y DTOs
  - Manejar excepciones de negocio

```csharp
// Ejemplo
public async Task<ClinicaResponseDto> CrearAsync(CreateClinicaDto createDto)
{
    // 1. Validar
    var validationResult = await _validator.ValidateAsync(createDto);
    if (!validationResult.IsValid) throw new ValidationException(...);
    
    // 2. Mapear DTO → Entidad
    var clinica = new Clinica { Nombre = createDto.Nombre, ... };
    
    // 3. Guardar
    var clinicaCreada = await _repository.CrearAsync(clinica);
    
    // 4. Mapear Entidad → DTO de respuesta
    return MapToResponseDto(clinicaCreada);
}
```

---

### **6️⃣ CAPA DE PRESENTACIÓN (API Layer)**

#### 🎮 Paso 10: Crear el Controller
- **Ubicación**: `Clynic.Api/Controllers/`
- **Archivo**: `ClinicasController.cs`
- **Descripción**: Expone los endpoints HTTP.
- **Responsabilidades**:
  - Recibir requests HTTP
  - Llamar al servicio correspondiente
  - Retornar respuestas HTTP (200, 201, 400, 404, 500)
  - Logging de errores
  - Manejo de excepciones

```csharp
// Ejemplo
[HttpPost]
public async Task<ActionResult<ClinicaResponseDto>> Crear([FromBody] CreateClinicaDto createDto)
{
    try
    {
        var clinicaCreada = await _clinicaService.CrearAsync(createDto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = clinicaCreada.Id }, clinicaCreada);
    }
    catch (ValidationException ex)
    {
        return BadRequest(new { mensaje = "Errores de validación", errores = ex.Errors });
    }
}
```

---

### **7️⃣ CONFIGURACIÓN DE INYECCIÓN DE DEPENDENCIAS**

#### ⚙️ Paso 11: Registrar Servicios en Program.cs
- **Ubicación**: `Clynic.Api/`
- **Archivo**: `Program.cs`
- **Descripción**: Configurar el contenedor de inyección de dependencias.
- **Orden de registro**:
  1. DbContext (conexión a base de datos)
  2. Repositorios
  3. Servicios
  4. Reglas de negocio
  5. Validadores

```csharp
// Ejemplo Program.cs
// DbContext
builder.Services.AddDbContext<ClynicDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositorios
builder.Services.AddScoped<IClinicaRepository, ClinicaRepository>();

// Servicios
builder.Services.AddScoped<IClinicaService, ClinicaService>();

// Reglas de negocio
builder.Services.AddScoped<ClinicaRules>();

// Validadores
builder.Services.AddScoped<IValidator<CreateClinicaDto>, CreateClinicaDtoValidator>();
```

---

### **8️⃣ MIGRACIONES DE BASE DE DATOS**

#### 🔄 Paso 12: Crear y Aplicar Migraciones
- **Comandos**:

```bash
# Desde la carpeta raíz del proyecto
cd Clynic.Infrastructure

# Crear migración
dotnet ef migrations add AgregarClinicas --startup-project ../Clynic.Api

# Aplicar migración
dotnet ef database update --startup-project ../Clynic.Api
```

---

## 📊 Resumen del Flujo

```
1. Modelo (Domain)
   ↓
2. DbContext (Infrastructure)
   ↓
3. IRepository (Application - Interface)
   ↓
4. Repository (Infrastructure - Implementación)
   ↓
5. DTOs (Application)
   ↓
6. Rules (Application)
   ↓
7. Validators (Application)
   ↓
8. IService (Application - Interface)
   ↓
9. Service (Application - Implementación)
   ↓
10. Controller (API)
    ↓
11. Program.cs - DI (API)
    ↓
12. Migraciones (Infrastructure + API)
```

---

## 🔍 Verificación

Después de completar todos los pasos:

1. ✅ Compilar el proyecto: `dotnet build`
2. ✅ Verificar que no haya errores
3. ✅ Ejecutar la API: `dotnet run --project Clynic.Api`
4. ✅ Probar con Swagger: `http://localhost:8080/swagger`
5. ✅ Probar endpoints:
   - GET `/api/clinicas` - Obtener todas
   - GET `/api/clinicas/{id}` - Obtener por ID
   - POST `/api/clinicas` - Crear nueva

---

## 📚 Estructura de Carpetas Final

```
Clynic.Domain/
  └── Models/
      └── Clinica.cs

Clynic.Infrastructure/
  ├── Data/
  │   └── ClynicDbContext.cs
  └── Repositories/
      └── ClinicaRepository.cs

Clynic.Application/
  ├── DTOs/
  │   └── Clinicas/
  │       ├── CreateClinicaDto.cs
  │       └── ClinicaResponseDto.cs
  ├── Interfaces/
  │   ├── Repositories/
  │   │   └── IClinicaRepository.cs
  │   └── Services/
  │       └── IClinicaService.cs
  ├── Rules/
  │   └── ClinicaRules.cs
  ├── Services/
  │   └── ClinicaService.cs
  └── Validators/
      └── CreateClinicaDtoValidator.cs

Clynic.Api/
  ├── Controllers/
  │   └── ClinicasController.cs
  └── Program.cs
```

---

## 🎓 Conceptos Clave

### **Separación de Responsabilidades**

- **Domain**: Solo modelos/entidades
- **Infrastructure**: Acceso a datos (EF Core, SQL)
- **Application**: Lógica de negocio, DTOs, validaciones, reglas
- **API**: Controllers, configuración HTTP

### **Flujo de Datos**

```
Request HTTP → Controller → Service → Repository → Database
                                ↓
                            Validations
                                ↓
                          Business Rules
                                ↓
                           DTO Mapping
```

### **Principios SOLID**

- **S**: Single Responsibility (cada clase tiene una responsabilidad)
- **O**: Open/Closed (abierto a extensión, cerrado a modificación)
- **L**: Liskov Substitution (usar interfaces)
- **I**: Interface Segregation (interfaces específicas)
- **D**: Dependency Inversion (depender de abstracciones)

---

## ✨ Buenas Prácticas

1. ✅ **Siempre usar DTOs** en controllers y services
2. ✅ **Validar en múltiples niveles**: FluentValidation + Rules
3. ✅ **Usar async/await** para operaciones de base de datos
4. ✅ **Inyección de dependencias** para todo
5. ✅ **Logging** de errores y operaciones importantes
6. ✅ **Manejo de excepciones** apropiado
7. ✅ **Eliminación lógica** (soft delete) en lugar de física
8. ✅ **Documentar** con comentarios XML (///)

---

**Fecha de creación**: 18 de febrero de 2026  
**Versión**: 1.0  
**Proyecto**: Clynic API - Clean Architecture
