#!/bin/bash
set -e

echo "=========================================="
echo "🚀 Iniciando SQL Server"
echo "=========================================="

# Levantar SQL Server en background
/opt/mssql/bin/sqlservr &
SQL_PID=$!

# Esperar a que SQL Server esté listo (aumentado a 60 segundos)
echo "⏳ Esperando que SQL Server inicie..."
sleep 30

# Intentar conectarse
for i in {1..30}; do
    if /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null; then
        echo "✅ SQL Server está listo!"
        break
    fi
    echo "⏳ Intento $i/30..."
    sleep 2
done

# Verificar si SQL Server respondió
if ! /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null; then
    echo "❌ ERROR: SQL Server no respondió"
    kill $SQL_PID 2>/dev/null || true
    exit 1
fi

echo ""
echo "📦 Ejecutando init.sql..."
if ! /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i /scripts/init.sql; then
    echo "❌ Error en init.sql"
    exit 1
fi

echo ""
echo "📦 Ejecutando schema.sql..."
if ! /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d SistemaCitasClinicasSaaS -i /scripts/schema.sql; then
    echo "❌ Error en schema.sql"
    exit 1
fi

echo ""
echo "📦 Ejecutando seed-data.sql..."
if ! /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d SistemaCitasClinicasSaaS -i /scripts/seed-data.sql; then
    echo "❌ Error en seed-data.sql"
    exit 1
fi

echo ""
echo "=========================================="
echo "✅ Base de datos inicializada correctamente"
echo "📊 Base de datos: SistemaCitasClinicasSaaS"
echo "🔐 Usuario: sa"
echo "=========================================="

# Mantener SQL Server corriendo
wait $SQL_PID
