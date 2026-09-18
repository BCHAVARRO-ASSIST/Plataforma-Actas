import { apiGet } from "../../shared/api/httpClient";

export interface DashboardKpis {
  totalEquipos: number;
  equiposActivos: number;
  equiposInactivos: number;
  equiposAsignados: number;
  equiposMantenimiento: number;
  totalActas: number;
  actasBorrador: number;
  actasFirmadas: number;
  actasFinalizadas: number;
  actasAnuladas: number;
  entregasPendientes: number;
  devolucionesPendientes: number;
}

export interface RecentActivityItem {
  id: string;
  tipoEvento: string;
  titulo: string;
  descripcion: string;
  responsable: string | null;
  fecha: string;
  estado: string;
  badgeTipo: string | null;
}

export async function getDashboardKpis(): Promise<DashboardKpis> {
  return apiGet<DashboardKpis>("/dashboard/kpis", {
    errorMessage: "No fue posible cargar los indicadores KPI.",
  });
}

export async function getRecentActivity(limit: number = 10): Promise<RecentActivityItem[]> {
  return apiGet<RecentActivityItem[]>(`/dashboard/recent-activity?limit=${limit}`, {
    errorMessage: "No fue posible cargar la actividad reciente.",
  });
}
