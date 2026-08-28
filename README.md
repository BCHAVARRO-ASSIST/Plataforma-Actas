# Plataforma de Actas

Sistema para la gestión del ciclo de vida de hardware, emisión de actas de entrega/devolución con firma digital y sellado criptográfico SHA-256.

---

## 📁 Estructura del Proyecto

- **`apps/api`**: Backend en ASP.NET Core 10 (Minimal APIs, EF Core 10, QuestPDF).
- **`apps/web-admin`**: Frontend SPA en React 19 + TypeScript + Vite + Tailwind CSS.
- **`database`**: Contenedor PostgreSQL 17.
- **`tools/migrator`**: Contenedor para ejecución automática de migraciones de base de datos.
- **`infra/docker`**: Docker Compose para orquestación de servicios (`db` → `migrator` → `api` → `web`).
- **`storage`**: Directorio de persistencia de documentos PDF oficiales.
- **`tests`**: Pruebas unitarias e integración.

---

## 🚀 Inicio Rápido con Docker Compose

```bash
# 1. Entrar a la carpeta de infraestructura
cd infra/docker

# 2. Levantar todos los servicios
docker compose up --build -d
```

- **Web Admin**: http://localhost
- **API Backend**: http://localhost:8080
- **Healthcheck**: http://localhost:8080/api/health

---

## 💻 Desarrollo Local

### Backend (API .NET 10)
```bash
dotnet run --project apps/api/PlataformaActas.Api.csproj
```

### Frontend (React 19)
```bash
cd apps/web-admin
npm install
npm run dev
```

### Pruebas
```bash
dotnet test PlataformaActas.sln
```
