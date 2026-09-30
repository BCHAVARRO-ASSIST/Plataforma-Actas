# syntax=docker/dockerfile:1.4
# ==============================================================================
# Multi-stage Build para PlataformaActas API (ASP.NET Core 10)
# ==============================================================================

# ------------------------------------------------------------------------------
# Etapa 1: Build y Restauración
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Aislar la capa de restauración para maximizar la caché de Docker
COPY ["apps/api/PlataformaActas.Api.csproj", "apps/api/"]
RUN dotnet restore "apps/api/PlataformaActas.Api.csproj"

# Copiar el código fuente completo
COPY apps/api/ apps/api/

# Compilar y empaquetar artefacto optimizado para producción
WORKDIR "/src/apps/api"
RUN dotnet publish "PlataformaActas.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ------------------------------------------------------------------------------
# Etapa 2: Runtime Seguro (Hardened Production Image)
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

LABEL maintainer="Assist Consultores de Sistemas <soporte@assist.com.co>"
LABEL description="API Backend de Plataforma de Actas - ASP.NET Core 10"

# Instalar dependencias nativas para generación PDF (QuestPDF/SkiaSharp) y curl para healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    fonts-dejavu-core \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Copiar binarios compilados
COPY --from=build /app/publish .

# Crear volumen de almacenamiento de documentos con permisos para usuario 'app' (UID 1654)
RUN mkdir -p /app/storage/documentos \
    && chown -R app:app /app/storage \
    && chmod -R 775 /app/storage

# HARDENING: Ejecutar bajo el usuario sin privilegios 'app'
USER app

# Healthcheck interno del contenedor
HEALTHCHECK --interval=15s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "PlataformaActas.Api.dll"]
