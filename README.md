# Plataforma de Actas

## Estructura del repositorio

- `front/web-admin`: aplicacion web administrativa en React y Vite.
- `back/api`: API REST en ASP.NET.
- `bd`: esquema, migraciones, datos semilla y respaldos de PostgreSQL.
- `infra`: configuracion de Docker, Nginx y Redis.
- `docs`: documentacion funcional, tecnica y de datos.
- `tests`: pruebas automatizadas.
- `packages`: contratos o librerias compartidas, solo cuando sean necesarios.
- `tools`: utilidades de desarrollo.

Cada capa conserva sus propias dependencias y configuracion. El frontend consume la API mediante `VITE_API_URL`; el backend recibe sus secretos mediante configuracion local o variables de entorno; la base de datos se versiona exclusivamente con scripts dentro de `bd/migrations`.

## Desarrollo local

1. Copia `infra/docker/.env.example` como `infra/docker/.env` y ajusta los valores locales.
2. Inicia PostgreSQL y Redis con `docker compose --env-file infra/docker/.env -f infra/docker/docker-compose.yml up -d`.
3. Configura los secretos del backend a partir de `back/api/appsettings.Development.example.json` sin versionar el archivo local.
4. Ejecuta la API desde `back/api` con `dotnet run`.
5. Copia `front/web-admin/.env.example` como `front/web-admin/.env.local` y ejecuta `npm.cmd run dev` desde esa carpeta.
