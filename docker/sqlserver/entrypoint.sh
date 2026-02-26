#!/bin/bash

set -euo pipefail

DB_PASSWORD="${MSSQL_SA_PASSWORD:-${SA_PASSWORD:-}}"
DB_NAME="${SQL_DATABASE:-ClynicDB}"

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
    if sqlcmd -S localhost -U sa -P "${DB_PASSWORD}" -d master -Q "SELECT 1" > /dev/null 2>&1; then
        echo "✅ SQL Server disponible"
        break
    fi

    if [ "$i" -eq 60 ]; then
        echo "❌ SQL Server no estuvo disponible a tiempo"
        exit 1
    fi

    sleep 2
done

# Verificar si la base de datos ya existe para evitar reinicialización
DB_EXISTS=$(sqlcmd -h -1 -W -S localhost -U sa -P "${DB_PASSWORD}" -d master -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'${DB_NAME}') IS NULL THEN 0 ELSE 1 END" | tr -d '\r')

if [ "${DB_EXISTS}" = "1" ]; then
    echo "ℹ️ La base de datos ${DB_NAME} ya existe. Se omite inicialización de scripts."
else
    # Ejecutar scripts SQL en orden específico
    echo "📄 Ejecutando scripts SQL..."

    # 1. Crear base de datos
    echo "  [1/3] Ejecutando: init.sql"
    sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -d master -i "/usr/scripts/init.sql"
    echo "    ✅ Base de datos creada"

    # 2. Crear schema y tablas
    echo "  [2/3] Ejecutando: schema.sql"
    sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -i "/usr/scripts/schema.sql"
    echo "    ✅ Schema creado"

    # 3. Insertar datos
    echo "  [3/3] Ejecutando: seed-data.sql"
    sqlcmd -b -S localhost -U sa -P "${DB_PASSWORD}" -i "/usr/scripts/seed-data.sql"
    echo "    ✅ Datos insertados"

    echo "✅ Inicialización completada"
fi

# Mantener SQL Server en foreground
wait $SERVER_PID



