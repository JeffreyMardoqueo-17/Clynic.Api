#!/bin/bash

set -euo pipefail

DB_PASSWORD="${MSSQL_SA_PASSWORD:-${SA_PASSWORD:-}}"

if [ -z "${DB_PASSWORD}" ]; then
    echo "❌ No se encontró contraseña de SA. Define MSSQL_SA_PASSWORD (o SA_PASSWORD)."
    exit 1
fi

echo "=========================================="
echo "🚀 Iniciando SQL Server 2022"
echo "=========================================="

# Iniciar SQL Server en background
echo "⏳ SQL Server iniciando en background..."
/opt/mssql/bin/sqlservr &
SERVER_PID=$!

# Esperar a que SQL Server esté listo
echo "⏳ Esperando a que SQL Server esté disponible..."
for i in {1..60}; do
    if sqlcmd -S localhost -U sa -P "${DB_PASSWORD}" -Q "SELECT 1" > /dev/null 2>&1; then
        echo "✅ SQL Server disponible"
        break
    fi

    if [ "$i" -eq 60 ]; then
        echo "❌ SQL Server no estuvo disponible a tiempo"
        exit 1
    fi

    sleep 2
done

# Ejecutar scripts SQL en orden específico
echo "📄 Ejecutando scripts SQL..."

# 1. Crear base de datos
echo "  [1/3] Ejecutando: init.sql"
sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -i "/usr/scripts/init.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Base de datos creada"
else
    echo "    ⚠️ Error al crear base de datos"
fi

# 2. Crear schema y tablas
echo "  [2/3] Ejecutando: schema.sql"
sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -i "/usr/scripts/schema.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Schema creado"
else
    echo "    ⚠️ Error al crear schema"
fi

# 3. Insertar datos
echo "  [3/3] Ejecutando: seed-data.sql"
sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -i "/usr/scripts/seed-data.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Datos insertados"
else
    echo "    ⚠️ Error al insertar datos"
fi

echo "✅ Inicialización completada"

# Mantener SQL Server en foreground
wait $SERVER_PID



