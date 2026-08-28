FROM mcr.microsoft.com/dotnet/sdk:10.0-preview
WORKDIR /src

# Copy API project and source
COPY ["apps/api/PlataformaActas.Api.csproj", "apps/api/"]
RUN dotnet restore "apps/api/PlataformaActas.Api.csproj"

COPY apps/api/ apps/api/

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef --version 10.0.11
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src/apps/api
RUN dotnet build "PlataformaActas.Api.csproj" -c Release --no-restore

ENTRYPOINT ["dotnet", "ef", "database", "update", "--no-build", "--configuration", "Release"]
