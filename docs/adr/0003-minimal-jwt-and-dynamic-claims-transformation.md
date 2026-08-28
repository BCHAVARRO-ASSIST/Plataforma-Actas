# ADR 0003: Token JWT Minimalista y Transformación Dinámica de Permisos

## Estado
Aceptado

## Contexto
Incluir roles, permisos y datos personales directamente dentro del payload del JWT provocaba dos problemas:
1. Si se revocaba un permiso o se desactivaba a un usuario en la base de datos, el usuario seguía teniendo acceso hasta la expiración natural del token (ventana de vulnerabilidad).
2. El token aumentaba innecesariamente de tamaño al incluir listas largas de permisos y datos redundantes.

## Decisión
1. El JWT se genera **exclusivamente con el identificador del usuario (`nameid` / `ClaimTypes.NameIdentifier`)** y marcas de tiempo estándar.
2. Se establece la vida útil del access token en **30 minutos** (`Jwt:ExpiresMinutes = 30`) como punto de balance entre seguridad y experiencia de usuario en campo (evitar reautenticación forzada frecuente de mensajeros en conectividad intermitente).
3. Se implementa `UserClaimsTransformation` (`IClaimsTransformation`) en el pipeline de ASP.NET Core:
   - En cada petición autenticada, ejecuta una sola consulta con `Include` a PostgreSQL para verificar que el usuario esté activo (`Activo == true`) e inyectar su rol y permisos en memoria en el `ClaimsPrincipal`.
   - Si el usuario es desactivado o se cambian sus permisos en la BD, la revocación es **inmediata en la siguiente petición**, sin requerir caché ni Redis.
4. El frontend implementa un watchdog de sesión local que detecta la ausencia del token en el almacenamiento del navegador y reacciona de inmediato para UX de sesión.

## Limitaciones Conocidas y Seguridad Residual
- **Ventana ante Robo de Token:** `UserClaimsTransformation` revoca inmediatamente ante *cambios de rol o desactivación de cuenta en BD*. Sin embargo, ante el *robo/filtración del token* mientras la cuenta siga activa, el token seguirá siendo válido hasta su expiración natural (**ventana residual de hasta 30 minutos**).
- **Alcance del Watchdog Frontend:** El watchdog en React es una medida de **UX de sesión en el navegador cliente**, no un mecanismo de revocación en el servidor ante tokens interceptados.
- **Evolución Futura Recomendada:** Para reducir el access token a tiempos muy cortos (15 minutos o menos) sin degradar la UX en campo, se deberá incorporar el ciclo de rotación con `RefreshToken` persistido en base de datos.

## Consecuencias
- **Positivas:** Revocación instantánea ante cambios de permisos, token liviano, cero datos sensibles en el payload del JWT.
- **Negativas:** Realiza una consulta indexada por petición a la tabla `usuarios` y `roles`/`permisos`. Para la escala actual es despreciable; en concurrencia extrema se puede evaluar caché L1 en memoria de 30-60 segundos.
