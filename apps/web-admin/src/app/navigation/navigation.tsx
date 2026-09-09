import { PERMISSIONS } from "../../features/auth/permissions";
import type { NavigationItem } from "./navigationTypes";
import {
  DashboardIcon,
  EquipmentIcon,
  ActaIcon,
  MaintenanceIcon,
  ReportsIcon,
  MessengerIcon,
  UsersIcon,
  RolesIcon,
} from "../../components/Icons";

export const NAVIGATION_ITEMS: NavigationItem[] = [
  {
    id: "dashboard",
    label: "Dashboard",
    view: "dashboard",
    section: "principal",
    icon: <DashboardIcon size={18} />,
    permission: PERMISSIONS.DASHBOARD_CONSULTAR,
  },
  {
    id: "equipment",
    label: "Inventario",
    view: "equipment",
    section: "principal",
    icon: <EquipmentIcon size={18} />,
    permission: PERMISSIONS.EQUIPOS_CONSULTAR,
  },
  {
    id: "actas",
    label: "Actas",
    view: "actas",
    section: "principal",
    icon: <ActaIcon size={18} />,
    permission: PERMISSIONS.ACTAS_CONSULTAR,
  },
  {
    id: "mantenimientos",
    label: "Mantenimientos",
    view: "mantenimientos",
    section: "principal",
    icon: <MaintenanceIcon size={18} />,
    permission: PERMISSIONS.EQUIPOS_CONSULTAR,
  },
  {
    id: "reportes",
    label: "Reportes",
    view: "reportes",
    section: "principal",
    icon: <ReportsIcon size={18} />,
    permission: PERMISSIONS.REPORTES_CONSULTAR,
  },
  {
    id: "mensajero-actas",
    label: "Mensajero / Entregas",
    view: "mensajero",
    section: "principal",
    icon: <MessengerIcon size={18} />,
    permission: PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR,
  },
  {
    id: "usuarios",
    label: "Usuarios",
    view: "usuarios",
    section: "principal",
    icon: <UsersIcon size={18} />,
    permission: PERMISSIONS.USUARIOS_CONSULTAR,
  },
  {
    id: "roles",
    label: "Roles y permisos",
    view: "roles",
    section: "principal",
    icon: <RolesIcon size={18} />,
    permission: PERMISSIONS.ROLES_CONSULTAR,
  },
];
