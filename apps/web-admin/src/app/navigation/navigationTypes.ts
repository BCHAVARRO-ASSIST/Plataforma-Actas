import type { ReactNode } from "react";
import type { Permission } from "../../features/auth/permissions";

export type View =
  | "dashboard"
  | "equipment"
  | "actas"
  | "reportes"
  | "mantenimientos"
  | "usuarios"
  | "roles"
  | "mensajero";

export type NavigationSection = "principal" | "gestion";

export interface NavigationItem {
  id: string;
  label: string;
  view: View;
  section: NavigationSection;
  icon?: ReactNode;
  permission?: Permission;
  disabled?: boolean;
}