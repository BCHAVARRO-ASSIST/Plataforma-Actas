# ADR 0001: Arquitectura de Monolito Modular con Vertical Slices

## Estado
Aceptado

## Contexto
La Plataforma de Actas originalmente presentaba una organización por capas tradicionales y carpetas de características con acoplamientos difusos y duplicación de modelos entre controladores y servicios. Para soportar el crecimiento del dominio (gestión de activos, generación y firma de actas oficiales, auditoría RBAC, generación de reportes y KPIs en tiempo real), se requería una arquitectura modular limpia, escalable y mantenible.

## Decisión
Adoptar un enfoque de **Monolito Modular con Vertical Slices** en el backend ASP.NET Core 10:
1. Cada módulo de negocio (`Modules/{Modulo}/`) es autocontenido y agrupa:
   - **Data**: Entidades EF Core, mapeos Fluent (`IEntityTypeConfiguration<T>`) y repositorios con interfaces (`I{Modulo}Repository`).
   - **Services**: Lógica de negocio encapsulada con interfaces (`I{Modulo}Service`).
   - **Models**: DTOs inmutables para Request/Response y mappers explícitos en extensiones estáticas.
   - **Endpoints**: Minimal APIs organizadas por módulo con requerimientos explícitos de permisos (`RequirePermission`).
   - **{Modulo}Module.cs**: Métodos de extensión para registro de Inyección de Dependencias (`Add{Modulo}Module()`) y mapeo de rutas (`Map{Modulo}Module()`).
2. Módulos triviales sin estado complejo (p. ej., `Health`) se simplifican a un único archivo plano sin subcarpetas innecesarias.

## Consecuencias
- **Positivas**:
  - Alta cohesión y bajo acoplamiento entre dominios.
  - Facilidad de pruebas unitarias y de integración al contar con interfaces explícitas.
  - Navegación clara del código fuente: toda la funcionalidad de un dominio reside en una única carpeta.
  - Eliminación de controladores MVC pesados en favor de Minimal APIs optimizadas.
- **Mitigaciones**:
  - Referencias cruzadas entre módulos se realizan mediante IDs y DbSets tipados, previniendo referencias circulares.
