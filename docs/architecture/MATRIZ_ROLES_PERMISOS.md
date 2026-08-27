# 🔐 MATRIZ DE CONTROL DE ACCESO (RBAC)

## 1. Roles del Sistema

| Rol | Descripción | Caso de Uso |
| :--- | :--- | :--- |
| **Administrador** | Acceso total a todos los módulos y operaciones del sistema. | Gestión de usuarios, roles, catálogo, actas y auditoría. |
| **Gestor de Actas** | Creación y administración del inventario y ciclo documental de actas. | Registro de equipos, emisión de actas de entrega/devolución y finalización. |
| **Mensajero** | Perfil móvil para la visualización y firma de entregas asignadas en campo. | Uso exclusivo desde teléfono móvil para recolección de firmas en terreno. |
| **Consulta** | Visualización en modo solo lectura de inventario y actas emitidas. | Auditoría interna, supervisión o consultas generales sin permisos de edición. |

---

## 2. Matriz de Permisos por Rol

| Módulo | Código de Permiso | Administrador | Gestor de Actas | Mensajero | Consulta |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **Equipos** | `EQUIPOS_CONSULTAR` | ✅ | ✅ | ❌ | ✅ |
| | `EQUIPOS_REGISTRAR` | ✅ | ✅ | ❌ | ❌ |
| | `EQUIPOS_EDITAR` | ✅ | ✅ | ❌ | ❌ |
| | `EQUIPOS_DESACTIVAR` | ✅ | ✅ | ❌ | ❌ |
| **Actas** | `ACTAS_CONSULTAR` | ✅ | ✅ | ❌ | ✅ |
| | `ACTAS_REGISTRAR` | ✅ | ✅ | ❌ | ❌ |
| | `ACTAS_EDITAR` | ✅ | ✅ | ❌ | ❌ |
| | `ACTAS_FIRMAR` | ✅ | ✅ | ❌ | ❌ |
| | `ACTAS_EXPORTAR` | ✅ | ✅ | ❌ | ✅ |
| **Mensajero**| `MENSAJERO_ACTAS_CONSULTAR` | ✅ | ❌ | ✅ | ❌ |
| | `MENSAJERO_ACTAS_FIRMAR` | ✅ | ❌ | ✅ | ❌ |
| **Usuarios**| `USUARIOS_CONSULTAR` | ✅ | ❌ | ❌ | ❌ |
| | `USUARIOS_REGISTRAR` | ✅ | ❌ | ❌ | ❌ |
| | `USUARIOS_EDITAR` | ✅ | ❌ | ❌ | ❌ |
| | `USUARIOS_DESACTIVAR`| ✅ | ❌ | ❌ | ❌ |
| **Roles** | `ROLES_CONSULTAR` | ✅ | ❌ | ❌ | ❌ |
| | `ROLES_EDITAR` | ✅ | ❌ | ❌ | ❌ |

---

## 3. Mecanismos de Seguridad y Autorización
1. **Autenticación:** Tokens JWT con algoritmo HMAC-SHA256, tiempo de expiración y claims de usuario e identificación.
2. **Autorización Declarativa:** Protección a nivel de endpoint mediante `RequirePermission(string permission)` y `RequireAnyPermission(params string[] permissions)`.
3. **Protección IDOR:** Validación a nivel de servicio para garantizar que los mensajeros únicamente puedan acceder y firmar las actas asignadas explícitamente a su identificador de usuario (`mensajero_usuario_id == authUsuarioId`).
