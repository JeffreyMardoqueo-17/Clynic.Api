# Clynic API

Sistema de gestion de citas para clinicas desarrollado con ASP.NET Core 8 y SQL Server 2022.

---

## 📋 Requisitos

- Docker Desktop instalado y corriendo
- Git

---

## 🚀 Arrancar el Proyecto

### 1. Clonar el repositorio
```bash
git clone <url-del-repositorio>
cd API
```

### 2. Configurar variables de entorno
```bash
copy .env.example .env
```

Edita `.env` y define una contrasena fuerte en `SQL_SA_PASSWORD`.

### 3. Levantar los contenedores
```bash
cd "C:\Users\Admin\Desktop\norvian.tech\Clynic\API"
docker compose up -d
```

### 4. Verificar que esta corriendo
```bash
docker compose ps
```

Deberias ver:
- `clynic-sql` -> **healthy**
- `clynic-api` -> **running**

### 5. Acceder a la aplicacion
- **Swagger (API)**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/api/health/status

---

## 🛑 Detener el Proyecto

```bash
docker compose down
```

Para **borrar tambien los datos**:
```bash
docker compose down -v
```

---

## 🔧 Solucion de Problemas

### ❌ Error: "SQL Server no inicia"

**Ver logs del contenedor SQL:**
```bash
docker logs clynic-sql
```

**Solucion:** Espera 60 segundos. SQL Server tarda en inicializar la base de datos.

---

### ❌ Error: "API no se conecta a la BD"

**Verificar estado:**
```bash
docker compose ps
```

**Ver logs de la API:**
```bash
docker logs clynic-api
```

**Solucion:** Asegurate que `clynic-sql` este **healthy** antes de que la API inicie.

---

### ❌ Error: "Puerto 8080 o 1433 ya en uso"

**Ver que proceso usa el puerto:**
```powershell
netstat -ano | findstr :8080
netstat -ano | findstr :1433
```

**Solucion:** Deten el proceso o cambia el puerto en [compose.yaml](compose.yaml).

---

### 🔄 Reconstruir desde cero

Si algo no funciona, **reconstruye todo**:
```bash
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

---

## 📊 Conexion a la Base de Datos

- **Host**: `localhost`
- **Puerto**: `1433`
- **Usuario**: `sa`
- **Contrasena**: la de tu `.env`
- **Base de datos**: `SistemaCitasClinicasSaaS`

**Connection String**:
```
Server=localhost,1433;Database=SistemaCitasClinicasSaaS;User Id=sa;Password=<SQL_SA_PASSWORD>;TrustServerCertificate=True;
```

---

## 📝 Ver Logs en Tiempo Real

```bash
# Todos los contenedores
docker compose logs -f

# Solo SQL Server
docker logs clynic-sql -f

# Solo API
docker logs clynic-api -f
```

---

## 📚 Documentacion Adicional

- **Arquitectura**: [docs/Structure.md](docs/Structure.md)
- **Docker Compose**: [compose.yaml](compose.yaml)

---

## ⚙️ Cambiar la Contrasena de SQL Server

1. Edita `.env`
2. Cambia `SQL_SA_PASSWORD`
3. Borra volumenes y reconstruye:
```bash
docker compose down -v
docker compose up -d
```

---

## 🧩 Notas de Compose

Docker Compose usa por defecto `compose.yaml`. El archivo [docker/docker-compose.yml](docker/docker-compose.yml) se mantiene como legacy.
