#!/bin/bash

echo "=========================================="
echo "🚀 Iniciando SQL Server 2022"
echo "=========================================="

# Iniciar SQL Server en background
echo "⏳ SQL Server iniciando en background..."
/opt/mssql/bin/sqlservr &
SERVER_PID=$!

# Esperar a que SQL Server esté listo
echo "⏳ Esperando a que SQL Server esté disponible..."
sleep 30

# Ejecutar scripts SQL en orden específico
echo "📄 Ejecutando scripts SQL..."

# 1. Crear base de datos
echo "  [1/3] Ejecutando: init.sql"
sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -i "/usr/scripts/init.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Base de datos creada"
else
    echo "    ⚠️ Error al crear base de datos"
fi

# 2. Crear schema y tablas
echo "  [2/3] Ejecutando: schema.sql"
sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -i "/usr/scripts/schema.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Schema creado"
else
    echo "    ⚠️ Error al crear schema"
fi

# 3. Insertar datos
echo "  [3/3] Ejecutando: seed-data.sql"
sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -i "/usr/scripts/seed-data.sql"
if [ $? -eq 0 ]; then
    echo "    ✅ Datos insertados"
else
    echo "    ⚠️ Error al insertar datos"
fi

echo "✅ Inicialización completada"

# Mantener SQL Server en foreground
wait $SERVER_PID



