FROM mcr.microsoft.com/mssql/server:2022-latest

# Variables de entorno
ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433

# Instalar dependencias y herramientas SQL
USER root
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    apt-transport-https \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list | tee /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools \
    && rm -rf /var/lib/apt/lists/* \
    && ln -s /opt/mssql-tools/bin/sqlcmd /usr/local/bin/sqlcmd

# Crear directorio para scripts
RUN mkdir -p /usr/config /usr/scripts
WORKDIR /usr/config

# Copiar scripts SQL
COPY docker/sqlserver/init.sql /usr/scripts/
COPY docker/sqlserver/schema.sql /usr/scripts/
COPY docker/sqlserver/seed-data.sql /usr/scripts/

# Copiar y preparar entrypoint
COPY docker/sqlserver/entrypoint.sh /usr/config/

# Convertir line endings y dar permisos
RUN sed -i 's/\r$//' /usr/config/entrypoint.sh /usr/scripts/*.sql && \
    chmod +x /usr/config/entrypoint.sh

EXPOSE 1433

# Usar script personalizado
ENTRYPOINT ["/bin/bash", "/usr/config/entrypoint.sh"]

