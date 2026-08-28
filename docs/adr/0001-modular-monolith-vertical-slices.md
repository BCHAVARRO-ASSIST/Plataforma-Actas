# ADR 0001: Adopción de Monolito Modular con Vertical Slices

## Estado
Aceptado

## Contexto
El backend de la Plataforma de Actas requería una arquitectura mantenible, desacoplada y escalable para gestionar el ciclo de vida de hardware, emisión de actas electrónicas, firmas digitales y control de accesos RBAC con 4 roles.
La arquitectura tradicional en capas técnicas (`Controllers/`, `Services/`, `Models/`, `Interfaces/` separadas) generaba dispersión del código y fricción al modificar una funcionalidad de negocio.

## Decisión
Se adopta una arquitectura de **Monolito Modular con Vertical Slices**:
1. Todo el código perteneciente a un dominio de negocio vive dentro de su módulo en `apps/api/Modules/{Modulo}/` (`Endpoints`, `Models`, `Service+Interfaz`, `Repository+Interfaz`).
2. Se reemplazan los controladores MVC tradicionales por **ASP.NET Core Minimal APIs**, mejorando el rendimiento y reduciendo el código repetitivo.
3. Se mantiene separación estricta entre Entidades de EF Core y DTOs de contrato de API mediante namespaces y mappers explícitos.

## Consecuencias
- **Positivas:** Cohesión alta, bajo acoplamiento entre módulos, facilidad de prueba con inyección de dependencias y navegación rápida para los desarrolladores.
- **Negativas:** Requiere disciplina para no crear dependencias circulares directas entre módulos sin pasar por contratos claros.
