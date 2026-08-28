// =====================================================
// AUTORIZACIÓN DE PERMISOS
// Plataforma de Actas
// =====================================================

import { AUTH_PERMISSIONS_KEY } from "./authService";

export const PERMISSIONS = {
  // DASHBOARD
  DASHBOARD_CONSULTAR: "DASHBOARD_CONSULTAR",

  // USUARIOS
  USUARIOS_CONSULTAR: "USUARIOS_CONSULTAR",
  USUARIOS_REGISTRAR: "USUARIOS_REGISTRAR",
  USUARIOS_EDITAR: "USUARIOS_EDITAR",
  USUARIOS_DESACTIVAR: "USUARIOS_DESACTIVAR",

  // EQUIPOS
  EQUIPOS_CONSULTAR: "EQUIPOS_CONSULTAR",
  EQUIPOS_REGISTRAR: "EQUIPOS_REGISTRAR",
  EQUIPOS_EDITAR: "EQUIPOS_EDITAR",
  EQUIPOS_DESACTIVAR: "EQUIPOS_DESACTIVAR",

  // ACTAS
  ACTAS_CONSULTAR: "ACTAS_CONSULTAR",
  ACTAS_REGISTRAR: "ACTAS_REGISTRAR",
  ACTAS_EDITAR: "ACTAS_EDITAR",
  ACTAS_FIRMAR: "ACTAS_FIRMAR",
  ACTAS_EXPORTAR: "ACTAS_EXPORTAR",

  // MENSAJERO
  MENSAJERO_ACTAS_CONSULTAR: "MENSAJERO_ACTAS_CONSULTAR",
  MENSAJERO_ACTAS_FIRMAR: "MENSAJERO_ACTAS_FIRMAR",
  MENSAJERO_ACTAS_ENTREGAR: "MENSAJERO_ACTAS_ENTREGAR",
  MENSAJERO_ACTAS_DEVOLVER: "MENSAJERO_ACTAS_DEVOLVER",

  // REPORTES
  REPORTES_CONSULTAR: "REPORTES_CONSULTAR",
  REPORTES_EXPORTAR: "REPORTES_EXPORTAR",

  // ROLES
  ROLES_CONSULTAR: "ROLES_CONSULTAR",
  ROLES_CREAR: "ROLES_CREAR",
  ROLES_EDITAR: "ROLES_EDITAR",
  ROLES_ELIMINAR: "ROLES_ELIMINAR",
} as const;

export type Permission =
  (typeof PERMISSIONS)[keyof typeof PERMISSIONS];

interface JwtPayload {
  permiso?: string | string[];
  exp?: number;
  [key: string]: unknown;
}

// =====================================================
// DECODIFICAR PAYLOAD DEL JWT
// =====================================================

function decodeToken(
  token: string
): JwtPayload | null {
  try {
    const parts = token.split(".");

    if (parts.length !== 3) {
      return null;
    }

    const payload = parts[1];

    const normalizedPayload =
      payload
        .replace(/-/g, "+")
        .replace(/_/g, "/");

    const paddedPayload =
      normalizedPayload.padEnd(
        normalizedPayload.length +
          ((4 - (normalizedPayload.length % 4)) % 4),
        "="
      );

    const decoded =
      atob(paddedPayload);

    return JSON.parse(
      decoded
    ) as JwtPayload;

  } catch {
    return null;
  }
}

// =====================================================
// OBTENER PERMISOS
// =====================================================

export function getPermissionsFromToken(
  token: string | null
): string[] {
  // 1. Prioridad: Permisos autorizados guardados en sesión/almacenamiento
  try {
    const raw = localStorage.getItem(AUTH_PERMISSIONS_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      if (Array.isArray(parsed) && parsed.length > 0) {
        return parsed;
      }
    }
  } catch {
    // Si falla el parseo, intentar desde el token
  }

  // 2. Fallback: Si el token contiene claims de permiso
  if (!token) {
    return [];
  }

  const payload = decodeToken(token);
  if (!payload?.permiso) {
    return [];
  }

  if (Array.isArray(payload.permiso)) {
    return payload.permiso;
  }

  return [payload.permiso];
}

// =====================================================
// VALIDAR UN PERMISO
// =====================================================

export function hasPermission(
  token: string | null,
  permission: Permission
): boolean {
  const permissions = getPermissionsFromToken(token);
  return permissions.includes(permission);
}

// =====================================================
// VALIDAR CUALQUIER PERMISO
// =====================================================

export function hasAnyPermission(
  token: string | null,
  permissions: Permission[]
): boolean {
  const userPermissions = getPermissionsFromToken(token);
  return permissions.some((permission) => userPermissions.includes(permission));
}

// =====================================================
// VALIDAR TODOS LOS PERMISOS
// =====================================================

export function hasAllPermissions(
  token: string | null,
  permissions: Permission[]
): boolean {
  const userPermissions = getPermissionsFromToken(token);
  return permissions.every((permission) => userPermissions.includes(permission));
}