# Build stage for EF Core migration bundle
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine AS build
WORKDIR /src

# Copy project files and restore
COPY ["apps/api/PlataformaActas.Api.csproj", "apps/api/"]
RUN dotnet restore "apps/api/PlataformaActas.Api.csproj"

# Copy full solution source code
COPY . .

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef --version 10.0.11
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create self-contained migration bundle
WORKDIR /src/apps/api
RUN dotnet ef migrations bundle --output /app/bundle --self-contained -r linux-musl-x64

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-preview-alpine AS final
WORKDIR /app
COPY --from=build /app/bundle /app/bundle
RUN chmod +x /app/bundle

ENTRYPOINT ["/app/bundle"]
