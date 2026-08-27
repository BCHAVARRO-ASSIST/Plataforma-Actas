import { PERMISSIONS } from "../../features/auth/utils/permissions";
import type { NavigationItem } from "./navigationTypes";

// =========================================================
// CONFIGURACIÓN DE NAVEGACIÓN UNIFICADA
// =========================================================

export const NAVIGATION_ITEMS: NavigationItem[] = [
  {
    id: "dashboard",
    label: "Dashboard",
    view: "dashboard",
    section: "principal",
    icon: "◈",
  },
  {
    id: "equipment",
    label: "Inventario",
    view: "equipment",
    section: "principal",
    icon: "▦",
    permission: PERMISSIONS.EQUIPOS_CONSULTAR,
  },
  {
    id: "actas",
    label: "Actas",
    view: "actas",
    section: "principal",
    icon: "▤",
    permission: PERMISSIONS.ACTAS_CONSULTAR,
  },
  {
    id: "mantenimientos",
    label: "Mantenimientos",
    view: "mantenimientos",
    section: "principal",
    icon: "⚙",
    permission: PERMISSIONS.EQUIPOS_CONSULTAR,
  },
  {
    id: "reportes",
    label: "Reportes",
    view: "reportes",
    section: "principal",
    icon: "◫",
    permission: PERMISSIONS.REPORTES_CONSULTAR,
  },
  {
    id: "mensajero-actas",
    label: "Mis actas",
    view: "mensajero",
    section: "principal",
    icon: "▣",
    permission: PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR,
  },
  {
    id: "usuarios",
    label: "Usuarios",
    view: "usuarios",
    section: "principal",
    icon: "◉",
    permission: PERMISSIONS.USUARIOS_CONSULTAR,
  },
  {
    id: "roles",
    label: "Roles y permisos",
    view: "roles",
    section: "principal",
    icon: "🔒",
    permission: PERMISSIONS.ROLES_CONSULTAR,
  },
];