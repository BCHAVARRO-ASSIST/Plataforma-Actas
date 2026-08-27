# ADR 0002: EF Core Migrations como Única Fuente de Verdad del Esquema de Base de Datos

## Estado
Aceptado

## Contexto
Anteriormente, el proyecto mantenía scripts SQL manuales dispersos en carpetas `bd/init`, `bd/migrations` y `bd/seeds`, junto con llamadas directas mediante consultas SQL embebidas (`NpgsqlDataSource`, `cmd.CommandText`). Esto provocaba discrepancias entre el modelo relacional real y el código C#, dificultando la reproducibilidad de despliegues y la trazabilidad de cambios.

## Decisión
1. Adoptar **Entity Framework Core 10 con Npgsql** como la **única fuente de verdad** para la definición y evolución del esquema relacional.
2. Todas las tablas, llaves foráneas, índices y tipos de datos (incluyendo JSONB) se declaran mediante Fluent API (`IEntityTypeConfiguration<T>`) en cada módulo.
3. El esquema se versiona exclusivamente a través de **EF Core Migrations** (`apps/api/Infrastructure/Persistence/Migrations/`).
4. Los despliegues en contenedores ejecutan un bundle de migración reproducible (`tools/migrator/Dockerfile`) antes de iniciar la API.
5. Se eliminan definitivamente los scripts `.sql` manuales paralelos de `bd/`.

## Consecuencias
- **Positivas**:
  - Determinismo total en entornos locales, de staging y de producción.
  - Generación automática de diffs de esquema y scripts idempotentes.
  - Validación de tipos en tiempo de compilación para consultas LINQ.
  - Reducción drástica de errores por desincronización de columnas o nombres de índices.
- **Mitigaciones**:
  - Para consultas analíticas de reportes o KPIs complejos se utiliza `AsNoTracking()` y proyecciones directas optimizadas con EF Core.
