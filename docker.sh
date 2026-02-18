#!/bin/bash

# ===============================================
# Script para gestionar Docker Compose
# ===============================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
DOCKER_DIR="$SCRIPT_DIR/docker"

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Funciones
print_info() {
    echo -e "${GREEN}ℹ️ $1${NC}"
}

print_warn() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Verificar que existe .env
check_env() {
    if [ ! -f "$PROJECT_DIR/.env" ]; then
        print_warn ".env no existe"
        if [ -f "$PROJECT_DIR/.env.example" ]; then
            print_info "Copiando .env.example a .env..."
            cp "$PROJECT_DIR/.env.example" "$PROJECT_DIR/.env"
            print_info ".env creado. ¡Edítalo si necesitas cambiar valores!"
        fi
    fi
}

# Casos de uso
case "${1:-help}" in
    build)
        print_info "Construyendo imágenes Docker..."
        check_env
        cd "$DOCKER_DIR"
        docker compose build
        print_info "✅ Build completado"
        ;;
    up)
        print_info "Levantando contenedores..."
        check_env
        cd "$DOCKER_DIR"
        docker compose up
        ;;
    up-bg)
        print_info "Levantando contenedores en background..."
        check_env
        cd "$DOCKER_DIR"
        docker compose up -d
        print_info "✅ Contenedores en ejecución"
        print_info "API disponible en: http://localhost:8080"
        ;;
    down)
        print_info "Deteniendo contenedores..."
        cd "$DOCKER_DIR"
        docker compose down
        print_info "✅ Contenedores detenidos"
        ;;
    restart)
        print_info "Reiniciando contenedores..."
        cd "$DOCKER_DIR"
        docker compose restart
        print_info "✅ Contenedores reiniciados"
        ;;
    logs)
        print_info "Mostrando logs..."
        cd "$DOCKER_DIR"
        docker compose logs -f
        ;;
    clean)
        print_warn "Eliminando contenedores y volúmenes..."
        cd "$DOCKER_DIR"
        docker compose down -v
        print_info "✅ Limpieza completada"
        ;;
    status)
        print_info "Estado de los contenedores:"
        cd "$DOCKER_DIR"
        docker compose ps
        ;;
    *)
        echo "Uso: $0 {build|up|up-bg|down|restart|logs|clean|status}"
        echo ""
        echo "Comandos:"
        echo "  build       - Construir imágenes Docker"
        echo "  up          - Levantar contenedores (foreground)"
        echo "  up-bg       - Levantar contenedores (background)"
        echo "  down        - Detener contenedores"
        echo "  restart     - Reiniciar contenedores"
        echo "  logs        - Ver logs en vivo"
        echo "  clean       - Eliminar contenedores y volúmenes"
        echo "  status      - Ver estado de contenedores"
        exit 1
        ;;
esac
