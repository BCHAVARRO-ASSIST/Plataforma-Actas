import type { Permission } from "../../features/auth/utils/permissions";


// =========================================================
// VISTAS DE LA APLICACIÓN
// =========================================================

export type View =
  | "dashboard"
  | "equipment"
  | "actas"
  | "reportes"
  | "mantenimientos"
  | "usuarios"
  | "roles"
  | "mensajero";




// =========================================================
// SECCIONES DE NAVEGACIÓN
// =========================================================

export type NavigationSection =
  | "principal"
  | "gestion";


// =========================================================
// ELEMENTO DE NAVEGACIÓN
// =========================================================

export interface NavigationItem {

  id: string;

  label: string;

  view: View;

  section: NavigationSection;

  icon: string;

  permission?: Permission;

  disabled?: boolean;

}