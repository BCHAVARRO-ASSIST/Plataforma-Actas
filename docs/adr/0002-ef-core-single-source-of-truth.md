# ADR 0002: Entity Framework Core como Única Fuente de Verdad del Esquema

## Estado
Aceptado

## Contexto
El esquema de PostgreSQL 17 se mantenía previamente mediante scripts SQL manuales dispersos en carpetas de inicialización, migraciones y datos semilla (`bd/`), con consultas ejecutadas mediante Npgsql/ADO.NET directo. Esto ocasionaba discrepancias entre el código C# y la base de datos, además de dificultar el control de versiones del modelo relacional.

## Decisión
1. Se adopta **Entity Framework Core 10 con Npgsql** como la única fuente de verdad para el esquema de la base de datos.
2. Se eliminan los scripts SQL manuales en `bd/` en favor de **EF Core Migrations** en `Infrastructure/Persistence/Migrations/`.
3. Todas las entidades se configuran mediante Fluent API (`IEntityTypeConfiguration<T>`).
4. Las migraciones se aplican automáticamente al encender la aplicación mediante `await dbContext.Database.MigrateAsync()`.

## Consecuencias
- **Positivas:** Esquema fuertemente tipado, migraciones versionadas y reproducibles en CI/CD y despliegues sin scripts manuales.
- **Negativas:** En escenarios futuros de escalado horizontal con múltiples instancias concurrentes de la API, las migraciones automáticas en el arranque deberán delegarse a un Job/InitContainer antes de levantar las réplicas.
