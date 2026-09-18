// =====================================================
// AUTORIZACIÓN DE PERMISOS
// Plataforma de Actas
// =====================================================

import type { AuthenticatedUser } from "./authService";

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

// =====================================================
// OBTENER PERMISOS DEL USUARIO EN MEMORIA
// =====================================================

export function getUserPermissions(
  user: AuthenticatedUser | null | undefined
): string[] {
  if (!user || !Array.isArray(user.permisos)) {
    return [];
  }
  return user.permisos;
}

// =====================================================
// VALIDAR UN PERMISO
// =====================================================

export function hasPermission(
  user: AuthenticatedUser | null | undefined,
  permission: Permission
): boolean {
  const permissions = getUserPermissions(user);
  return permissions.includes(permission);
}

// =====================================================
// VALIDAR CUALQUIER PERMISO
// =====================================================

export function hasAnyPermission(
  user: AuthenticatedUser | null | undefined,
  permissions: Permission[]
): boolean {
  const userPermissions = getUserPermissions(user);
  return permissions.some((permission) => userPermissions.includes(permission));
}

// =====================================================
// VALIDAR TODOS LOS PERMISOS
// =====================================================

export function hasAllPermissions(
  user: AuthenticatedUser | null | undefined,
  permissions: Permission[]
): boolean {
  const userPermissions = getUserPermissions(user);
  return permissions.every((permission) => userPermissions.includes(permission));
}