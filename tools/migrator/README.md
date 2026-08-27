# Migrador de Base de Datos — EF Core Bundle

Contenedor de utilidad para aplicar las migraciones de Entity Framework Core de forma determinista y reproducible antes de iniciar el servicio `api`.

## Uso en Docker Compose

El servicio `migrator` se ejecuta como un contenedor `service_completed_successfully` previo a la inicialización de la API:

```yaml
migrator:
  build:
    context: ../..
    dockerfile: tools/migrator/Dockerfile
  environment:
    - ConnectionStrings__Postgres=Host=db;Port=5432;Database=plataforma_actas;Username=postgres;Password=postgres
  depends_on:
    db:
      condition: service_healthy
```
