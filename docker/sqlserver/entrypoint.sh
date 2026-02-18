#!/bin/bash
set -e

echo "=========================================="
echo "🚀 Iniciando SQL Server 2022"
echo "=========================================="

# Iniciar SQL Server en background
/opt/mssql/bin/sqlservr &

# Esperar 30 segundos a que SQL Server inicie completamente
echo "⏳ Esperando 30 segundos para que SQL Server inicie..."
sleep 30

# Usar /opt/mssql-tools18 si existe, sino /opt/mssql-tools
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -f "$SQLCMD" ]; then
    SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

echo ""
echo "📦 Creando base de datos..."
$SQLCMD -S localhost -U sa -P "${SA_PASSWORD}" -C -i /usr/config/init.sql

echo ""
echo "📦 Creando esquema de tablas..."
$SQLCMD -S localhost -U sa -P "${SA_PASSWORD}" -C -d SistemaCitasClinicasSaaS -i /usr/config/schema.sql

echo ""
echo "📦 Insertando datos iniciales..."
$SQLCMD -S localhost -U sa -P "${SA_PASSWORD}" -C -d SistemaCitasClinicasSaaS -i /usr/config/seed-data.sql

# Marcar como saludable
touch /tmp/healthy

echo ""
echo "=========================================="
echo "✅ Inicialización completada"
echo "📊 Base de datos: SistemaCitasClinicasSaaS"
echo "🔐 Usuario: sa"
echo "🌐 Puerto: 1433"
echo "=========================================="

# Mantener el contenedor ejecutándose
echo ""
echo "SQL Server está listo para recibir conexiones..."
wait
