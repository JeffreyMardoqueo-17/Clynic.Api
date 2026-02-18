FROM mcr.microsoft.com/mssql/server:2022-latest

# Variables de entorno
ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433

# Crear directorio para scripts
USER root
RUN mkdir -p /usr/config
WORKDIR /usr/config

# Copiar scripts SQL
COPY docker/sqlserver/init.sql /usr/config/
COPY docker/sqlserver/schema.sql /usr/config/
COPY docker/sqlserver/seed-data.sql /usr/config/
COPY docker/sqlserver/entrypoint.sh /usr/config/

# Convertir line endings y dar permisos
RUN sed -i 's/\r$//' /usr/config/entrypoint.sh && \
    chmod +x /usr/config/entrypoint.sh

EXPOSE 1433

# Usar script personalizado
ENTRYPOINT ["/bin/bash", "/usr/config/entrypoint.sh"]
