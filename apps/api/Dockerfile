FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["apps/api/PlataformaActas.Api.csproj", "apps/api/"]
RUN dotnet restore "apps/api/PlataformaActas.Api.csproj"

# Copiar código fuente
COPY apps/api/ apps/api/

# Compilar y publicar Release aprovechando la restauración previa
WORKDIR "/src/apps/api"
RUN dotnet publish "PlataformaActas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Etapa final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Instalar dependencias nativas para QuestPDF (SkiaSharp) y curl para healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    fonts-dejavu-core \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Crear carpeta de almacenamiento con ownership para el usuario no-root 'app' (UID 1654)
RUN mkdir -p /app/storage/documentos \
    && chown -R app:app /app/storage \
    && chmod -R 775 /app/storage

# HARDENING: Ejecutar bajo el usuario sin privilegios 'app'
USER app

ENTRYPOINT ["dotnet", "PlataformaActas.Api.dll"]
