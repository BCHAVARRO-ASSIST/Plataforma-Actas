# Plataforma de Actas

Sistema corporativo para la gestión integral del ciclo de vida de hardware, emisión de actas de entrega/devolución de activos, control de estados, firma digital manuscrita y sellado criptográfico SHA-256.

---

## 📌 Introducción y Objetivos

La **Plataforma de Actas** digitaliza y audita la trazabilidad de los activos tecnológicos de la organización:
- **Gestión Documental:** Emisión, firma y resguardo de actas de entrega y devolución de equipos.
- **Seguridad Criptográfica:** Sellado de integridad con hash SHA-256 para prevenir alteraciones documentales.
- **Gestión de Roles y Permisos:** Control de acceso granular (RBAC) con protección de cuentas administrativas y auditoría de acciones.
- **Integraciones:** Sincronización y lectura con servicios corporativos (Google Workspace / Directory) y correo transaccional SMTP.

---

## 📁 Arquitectura y Estructura del Proyecto

El repositorio adopta una arquitectura modular monorepo estructurada de la siguiente manera:

- **`apps/api`**: Backend desarrollado en **ASP.NET Core 10** (Minimal APIs, Entity Framework Core 10 con PostgreSQL, QuestPDF para renderizado de actas oficiales, autenticación JWT con cookies HttpOnly y rate limiting anti-fuerza bruta).
- **`apps/web-admin`**: Frontend Single Page Application (SPA) desarrollado en **React 19**, **TypeScript**, **Vite** y **CSS Modular**.
- **`database`**: Esquemas, migraciones y configuración para **PostgreSQL 17**.
- **`tools/migrator`**: Contenedor e instrumentación para la ejecución controlada de migraciones de base de datos.
- **`infra/docker`**: Orquestación y perfiles con **Docker Compose** (`db` → `api` → `web`).
- **`storage`**: Volumen persistente para almacenamiento y resguardo de actas generadas en formato PDF.
- **`tests`**: Suite automatizada de pruebas unitarias y de integración en **xUnit** (.NET 10).

---

## 🚀 Inicio Rápido con Docker Compose

Para desplegar el entorno completo de servicios contenerizados de forma local:

```bash
# 1. Posicionarse en la raíz del proyecto
# 2. Levantar todos los servicios en segundo plano
docker compose up --build -d
```

### Puertos y Endpoints del Sistema

- **Web Admin (Frontend SPA):** `http://localhost:3000` (o `http://localhost`)
- **API Backend (.NET 10):** `http://localhost:8080`
- **Healthcheck de Diagnóstico:** `http://localhost:8080/api/health`
- **Base de Datos PostgreSQL 17:** `127.0.0.1:5432`

---

## 💻 Desarrollo Local (Sin Docker)

### Requisitos Previos
- **.NET 10 SDK** (o preview correspondiente).
- **Node.js 22 LTS** y `npm`.
- Instancia activa de **PostgreSQL 17**.

### 1. Backend (API .NET 10)
```bash
dotnet restore PlataformaActas.sln
dotnet run --project apps/api/PlataformaActas.Api.csproj
```

### 2. Frontend (React 19 + Vite)
```bash
cd apps/web-admin
npm install
npm run dev
```

### 3. Suite de Pruebas Automatizadas
```bash
# Compilación limpia
dotnet build PlataformaActas.sln -c Release

# Ejecutar pruebas unitarias y de integración
dotnet test PlataformaActas.sln -c Release
```

---

## 🛠️ Notas de Refactorización y Buenas Prácticas

- **Persistencia y Cascada:** Manejo explícito de claves foráneas con preservación de pistas de auditoría documental (evita eliminación de usuarios o roles vinculados a actas oficiales legales).
- **Rate Limiting Configurable:** Implementación de limitador de tasa de peticiones con umbrales configurables vía `RateLimiting:AuthPermitLimit` para entornos de pruebas de carga y mitigación OWASP A04/A05.
- **Contribución y Flujo Git:** Todo cambio debe canalizarse mediante Pull Requests hacia la rama `develop` o `main`, asegurando la ejecución limpia del pipeline de integración continua (CI) en Azure DevOps.
