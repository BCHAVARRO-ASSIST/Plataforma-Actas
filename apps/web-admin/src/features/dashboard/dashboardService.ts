import { API_URL } from "../../shared/config/api";

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

const TOKEN_KEY = "plataforma_actas_token";

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem(TOKEN_KEY);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export async function getDashboardKpis(): Promise<DashboardKpis> {
  const res = await fetch(`${API_URL}/dashboard/kpis`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar los indicadores KPI.");
  return res.json();
}

export async function getRecentActivity(limit: number = 10): Promise<RecentActivityItem[]> {
  const res = await fetch(`${API_URL}/dashboard/recent-activity?limit=${limit}`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar la actividad reciente.");
  return res.json();
}

