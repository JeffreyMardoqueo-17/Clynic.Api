# ========== STAGE 1: RUNTIME ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ========== STAGE 2: BUILD ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto (layer cacheable)
COPY ["Clynic.Api/Clynic.Api.csproj", "Clynic.Api/"]
COPY ["Clynic.Application/Clynic.Application.csproj", "Clynic.Application/"]
COPY ["Clynic.Domain/Clynic.Domain.csproj", "Clynic.Domain/"]
COPY ["Clynic.Infrastructure/Clynic.Infrastructure.csproj", "Clynic.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "Clynic.Api/Clynic.Api.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/Clynic.Api"
RUN dotnet build "Clynic.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ========== STAGE 3: PUBLISH ==========
FROM build AS publish
RUN dotnet publish "Clynic.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ========== STAGE 4: RUNTIME FINAL ==========
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check simple
HEALTHCHECK --interval=10s --timeout=5s --retries=3 \
  CMD dotnet /app/Clynic.Api.dll --version > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "Clynic.Api.dll"]
