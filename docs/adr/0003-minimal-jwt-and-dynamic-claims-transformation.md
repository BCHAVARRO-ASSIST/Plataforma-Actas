# ADR 0003: Token JWT Minimalista y Transformación Dinámica de Permisos

## Estado
Aceptado

## Contexto
Incluir roles, permisos y datos personales directamente dentro del payload del JWT provocaba dos problemas:
1. Si se revocaba un permiso o se desactivaba a un usuario en la base de datos, el usuario seguía teniendo acceso hasta la expiración natural del token (ventana de vulnerabilidad).
2. El token aumentaba innecesariamente de tamaño al incluir listas largas de permisos y datos redundantes.

## Decisión
1. El JWT se genera **exclusivamente con el identificador del usuario (`nameid` / `ClaimTypes.NameIdentifier`)** y marcas de tiempo estándar.
2. Se implementa `UserClaimsTransformation` (`IClaimsTransformation`) en el pipeline de ASP.NET Core:
   - En cada petición autenticada, ejecuta una sola consulta con `Include` a PostgreSQL para verificar que el usuario esté activo (`Activo == true`) e inyectar su rol y permisos en memoria en el `ClaimsPrincipal`.
   - Si el usuario es desactivado o se cambian sus permisos en la BD, la revocación es **inmediata en la siguiente petición**, sin requerir caché ni Redis.
3. El frontend implementa un watchdog de sesión local que detecta la ausencia del token en el almacenamiento y reacciona de inmediato para la experiencia de usuario.

## Consecuencias
- **Positivas:** Revocación instantánea sin ventana de tiempo de espera, token liviano, cero datos sensibles en el payload del JWT.
- **Negativas:** Realiza una consulta indexada por petición a la tabla `usuarios` y `roles`/`permisos`. Para la escala actual es despreciable; en concurrencia extrema de millones de peticiones se podría añadir caché L1 en memoria de 30-60 segundos.
