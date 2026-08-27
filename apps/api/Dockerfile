FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project file and restore
COPY ["apps/api/PlataformaActas.Api.csproj", "apps/api/"]
RUN dotnet restore "apps/api/PlataformaActas.Api.csproj"

# Copy full API source code
COPY apps/api/ apps/api/

# Build and publish release
WORKDIR "/src/apps/api"
RUN dotnet publish "PlataformaActas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Install font libraries for QuestPDF rendering
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    fonts-dejavu-core \
    curl \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Create storage directory with appropriate permissions
RUN mkdir -p /app/Storage/Documentos

ENTRYPOINT ["dotnet", "PlataformaActas.Api.dll"]
