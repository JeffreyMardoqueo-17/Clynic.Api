# 🧪 Scripts de Prueba - API Clínicas

## Ejecutar la API

```powershell
cd Clynic.Api
dotnet run
```

La API estará disponible en: `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

---

## 📝 Pruebas con PowerShell

### 1. Obtener todas las clínicas
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Get
$response | ConvertTo-Json -Depth 10
```

### 2. Crear una nueva clínica
```powershell
$body = @{
    nombre = "Clínica Central"
    telefono = "123-456-7890"
    direccion = "Av. Principal 123, Ciudad"
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
}

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers
$response | ConvertTo-Json -Depth 10
```

### 3. Obtener una clínica por ID
```powershell
$id = 1
$response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas/$id" -Method Get
$response | ConvertTo-Json -Depth 10
```

### 4. Crear varias clínicas de prueba
```powershell
$clinicas = @(
    @{
        nombre = "Clínica Norte"
        telefono = "555-1001"
        direccion = "Calle Norte 100"
    },
    @{
        nombre = "Clínica Sur"
        telefono = "555-1002"
        direccion = "Calle Sur 200"
    },
    @{
        nombre = "Clínica Este"
        telefono = "555-1003"
        direccion = "Calle Este 300"
    }
)

foreach ($clinica in $clinicas) {
    $body = $clinica | ConvertTo-Json
    $headers = @{ "Content-Type" = "application/json" }
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers
        Write-Host "✅ Clínica '$($clinica.nombre)' creada con ID: $($response.id)" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error al crear '$($clinica.nombre)': $($_.Exception.Message)" -ForegroundColor Red
    }
}
```

---

## 🧪 Pruebas de Validación

### Intentar crear clínica con nombre corto (debe fallar)
```powershell
$body = @{
    nombre = "Ab"  # Muy corto
    telefono = "123-456-7890"
    direccion = "Av. Principal 123"
} | ConvertTo-Json

$headers = @{ "Content-Type" = "application/json" }

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers
}
catch {
    Write-Host "Validación correcta: $($_.Exception.Message)" -ForegroundColor Yellow
}
```

### Intentar crear clínica sin teléfono (debe fallar)
```powershell
$body = @{
    nombre = "Clínica Test"
    telefono = ""  # Vacío
    direccion = "Av. Principal 123"
} | ConvertTo-Json

$headers = @{ "Content-Type" = "application/json" }

try {
    $response = Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers
}
catch {
    Write-Host "Validación correcta: $($_.Exception.Message)" -ForegroundColor Yellow
}
```

### Intentar crear clínica con nombre duplicado (debe fallar)
```powershell
# Primero crear una clínica
$body = @{
    nombre = "Clínica Única"
    telefono = "123-456-7890"
    direccion = "Av. Principal 123"
} | ConvertTo-Json

$headers = @{ "Content-Type" = "application/json" }
Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers

# Intentar crear otra con el mismo nombre (debe fallar)
try {
    Invoke-RestMethod -Uri "http://localhost:8080/api/clinicas" -Method Post -Body $body -Headers $headers
}
catch {
    Write-Host "Validación de duplicados correcta: $($_.Exception.Message)" -ForegroundColor Yellow
}
```

---

## 🔍 Verificar Base de Datos (SQL)

Si tienes acceso a SQL Server, puedes verificar directamente:

```sql
-- Ver todas las clínicas
SELECT * FROM Clinicas WHERE Activa = 1;

-- Contar clínicas activas
SELECT COUNT(*) AS TotalClinicas FROM Clinicas WHERE Activa = 1;

-- Ver clínicas ordenadas por fecha de creación
SELECT Id, Nombre, Telefono, FechaCreacion 
FROM Clinicas 
WHERE Activa = 1
ORDER BY FechaCreacion DESC;
```

---

## 📊 Script Completo de Prueba

```powershell
# Script completo para probar toda la funcionalidad

Write-Host "🚀 Iniciando pruebas del API de Clínicas..." -ForegroundColor Cyan

# URL base del API
$baseUrl = "http://localhost:8080/api/clinicas"
$headers = @{ "Content-Type" = "application/json" }

# 1. Crear clínicas de prueba
Write-Host "`n📝 Creando clínicas de prueba..." -ForegroundColor Yellow

$clinicasTest = @(
    @{ nombre = "Clínica Los Angeles"; telefono = "555-0101"; direccion = "Av. Los Angeles 100" },
    @{ nombre = "Clínica San Miguel"; telefono = "555-0102"; direccion = "Calle San Miguel 200" },
    @{ nombre = "Clínica Santa Rosa"; telefono = "555-0103"; direccion = "Av. Santa Rosa 300" }
)

$idsCreados = @()
foreach ($clinica in $clinicasTest) {
    try {
        $body = $clinica | ConvertTo-Json
        $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $body -Headers $headers
        $idsCreados += $response.id
        Write-Host "  ✅ Creada: $($clinica.nombre) - ID: $($response.id)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ❌ Error: $($clinica.nombre)" -ForegroundColor Red
    }
}

# 2. Obtener todas las clínicas
Write-Host "`n📋 Obteniendo todas las clínicas..." -ForegroundColor Yellow
try {
    $todasClinicas = Invoke-RestMethod -Uri $baseUrl -Method Get
    Write-Host "  ✅ Total de clínicas: $($todasClinicas.Count)" -ForegroundColor Green
}
catch {
    Write-Host "  ❌ Error al obtener clínicas" -ForegroundColor Red
}

# 3. Obtener clínicas por ID
Write-Host "`n🔍 Obteniendo clínicas individuales..." -ForegroundColor Yellow
foreach ($id in $idsCreados) {
    try {
        $clinica = Invoke-RestMethod -Uri "$baseUrl/$id" -Method Get
        Write-Host "  ✅ ID $id : $($clinica.nombre)" -ForegroundColor Green
    }
    catch {
        Write-Host "  ❌ No se encontró clínica con ID $id" -ForegroundColor Red
    }
}

# 4. Intentar crear clínica con datos inválidos
Write-Host "`n🧪 Probando validaciones..." -ForegroundColor Yellow

$datosInvalidos = @(
    @{ nombre = "AB"; telefono = "123-456-7890"; direccion = "Direccion valida" },  # Nombre muy corto
    @{ nombre = "Clínica Válida"; telefono = "123"; direccion = "Direccion valida" },  # Teléfono muy corto
    @{ nombre = "Clínica Válida"; telefono = "123-456-7890"; direccion = "ABC" }  # Dirección muy corta
)

foreach ($datos in $datosInvalidos) {
    try {
        $body = $datos | ConvertTo-Json
        Invoke-RestMethod -Uri $baseUrl -Method Post -Body $body -Headers $headers
        Write-Host "  ❌ NO validó correctamente: $($datos.nombre)" -ForegroundColor Red
    }
    catch {
        Write-Host "  ✅ Validación correcta: rechazó datos inválidos" -ForegroundColor Green
    }
}

Write-Host "`n✅ Pruebas completadas!" -ForegroundColor Cyan
```

---

## 📥 Guardar y ejecutar script

Guarda este contenido en un archivo llamado `test-clinicas.ps1` y ejecútalo:

```powershell
# Ejecutar el script de pruebas
.\test-clinicas.ps1
```

---

## 🌐 Probar con Swagger UI

La forma más fácil es usar Swagger UI:

1. Inicia la API: `dotnet run --project Clynic.Api`
2. Abre el navegador: `http://localhost:8080/swagger`
3. Expande `/api/clinicas`
4. Haz clic en "Try it out"
5. Ingresa los datos y ejecuta

---

## 📱 Ejemplo de respuesta exitosa

```json
{
  "id": 1,
  "nombre": "Clínica Central",
  "telefono": "123-456-7890",
  "direccion": "Av. Principal 123, Ciudad",
  "activa": true,
  "fechaCreacion": "2026-02-18T15:30:00Z"
}
```

## ❌ Ejemplo de respuesta con error de validación

```json
{
  "mensaje": "Errores de validación",
  "errores": [
    "El nombre debe tener al menos 3 caracteres.",
    "El teléfono debe tener al menos 7 caracteres."
  ]
}
```
