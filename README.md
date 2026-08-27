# Plataforma de Actas

Solución empresarial para la gestión del ciclo de vida, entrega física, custodia y control de activos tecnológicos mediante actas electrónicas firmadas digitalmente y selladas con hash criptográfico SHA-256.

---

## 📁 Estructura del Repositorio

- **`apps/api`**: Backend en **ASP.NET Core 10** bajo patrón de **Monolito Modular con Vertical Slices** (`Actas`, `Auth`, `Dashboard`, `Equipment`, `Health`, `Reportes`, `Roles`, `Usuarios`), persistencia con **EF Core 10** y generación de PDFs institucionales (**TI-FOR-ACTA-001**) mediante **QuestPDF**.
- **`apps/web-admin`**: Frontend SPA en **React 19 + TypeScript + Vite + Tailwind CSS**, con soporte para firmas manuscritas digitales en canvas táctil, paneles ejecutivos de métricas y gestión de inventario.
- **`database`**: Definición de contenedor **PostgreSQL 17** para despliegues.
- **`tools/migrator`**: Contenedor utilitario para aplicación automatizada de **EF Core Migrations Bundle**.
- **`infra/docker`**: Orquestación con **Docker Compose** (`db` → `migrator` → `api` → `web`) con dependencias estrictas por comprobación de estado de salud.
- **`docs`**: Documentación técnica, diagramas entidad-relación, flujos de secuencia y Architectural Decision Records (**ADR 0001** y **0002**).
- **`tests`**: Proyectos de pruebas automatizadas (`PlataformaActas.Api.UnitTests` y `PlataformaActas.Api.IntegrationTests`).
- **`storage`**: Almacenamiento local estructurado para documentos PDF generados (`storage/documentos/`).

Para una descripción detallada de la arquitectura, consulta [ARQUITECTURA_SISTEMA.md](file:///ARQUITECTURA_SISTEMA.md).

---

## 🚀 Puesta en Marcha y Desarrollo Local

### 1. Requisitos Previos
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js 22+](https://nodejs.org/) y npm / pnpm
- [Docker Desktop](https://www.docker.com/) o instancia local de PostgreSQL 17

---

### 2. Ejecución con Docker Compose (Recomendado)

```bash
# 1. Acceder al directorio de infraestructura
cd infra/docker

# 2. Configurar variables de entorno si es necesario (utiliza .env por defecto)
# cp .env.example .env

# 3. Construir y levantar todos los servicios (db, migrator, api, web)
docker compose up --build -d
```

- **Frontend Web Admin**: [http://localhost](http://localhost) (Puerto 80)
- **API Backend**: [http://localhost:8080](http://localhost:8080)
- **Healthcheck**: [http://localhost:8080/api/health](http://localhost:8080/api/health)
- **OpenAPI / Swagger**: [http://localhost:8080/openapi/v1.json](http://localhost:8080/openapi/v1.json)

---

### 3. Desarrollo Local Independiente

#### Backend (API .NET 10)
```bash
# Restaurar dependencias y compilar solución completa
dotnet build PlataformaActas.sln

# Ejecutar API
cd apps/api
dotnet run
```

#### Frontend (React 19 + Vite)
```bash
cd apps/web-admin
npm install
npm run dev
```

#### Ejecución de Pruebas Automatizadas
```bash
dotnet test PlataformaActas.sln
```
