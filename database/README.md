# Base de Datos — Plataforma de Actas

Este directorio define el contenedor de base de datos PostgreSQL 17 para el entorno local y de producción.

## Esquema y Migraciones

> **IMPORTANTE:** El esquema de base de datos **NO** se gestiona mediante scripts `.sql` manuales.
> El único mecanismo oficial y canónico de versionamiento y aplicación de esquema es **EF Core Migrations** (`apps/api/Infrastructure/Persistence/Migrations`), ejecutado a través del servicio contenedor `migrator` (`tools/migrator/Dockerfile`).

## Variables de Entorno
- `POSTGRES_DB`: Nombre de la base de datos (por defecto `plataforma_actas`).
- `POSTGRES_USER`: Usuario administrador (por defecto `postgres`).
- `POSTGRES_PASSWORD`: Contraseña (por defecto `postgres`).
- `POSTGRES_PORT`: Puerto de escucha (por defecto `5432`).
