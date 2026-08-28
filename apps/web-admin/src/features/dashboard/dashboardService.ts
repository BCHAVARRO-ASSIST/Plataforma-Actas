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

export interface StatusCountItem {
  estado: string;
  cantidad: number;
  porcentaje: number;
  color?: string | null;
}

export interface MessengerDeliveryItem {
  mensajeroId: number | null;
  mensajeroNombre: string;
  totalAsignadas: number;
  totalEntregadas: number;
  totalPendientes: number;
}

export interface TimelinePointItem {
  fecha: string;
  totalCreadas: number;
  totalFirmadas: number;
  totalEntregas: number;
  totalDevoluciones: number;
}

export interface MaintenanceStats {
  totalMantenimientos: number;
  preventivos: number;
  correctivos: number;
  concluidos: number;
  pendientes: number;
  mantenimientosPorTipo: StatusCountItem[];
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

export async function getEquipmentStatusChart(): Promise<StatusCountItem[]> {
  const res = await fetch(`${API_URL}/dashboard/charts/equipment-status`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar el estado de equipos.");
  return res.json();
}

export async function getActasStatusChart(): Promise<StatusCountItem[]> {
  const res = await fetch(`${API_URL}/dashboard/charts/actas-status`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar el estado de actas.");
  return res.json();
}

export async function getDeliveriesByMessengerChart(): Promise<MessengerDeliveryItem[]> {
  const res = await fetch(`${API_URL}/dashboard/charts/deliveries-messenger`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar las entregas por mensajero.");
  return res.json();
}

export async function getTimelineChart(range: string = "30d"): Promise<TimelinePointItem[]> {
  const res = await fetch(`${API_URL}/dashboard/charts/timeline?range=${encodeURIComponent(range)}`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar la línea temporal.");
  return res.json();
}

export async function getMaintenanceStats(): Promise<MaintenanceStats> {
  const res = await fetch(`${API_URL}/dashboard/maintenance-stats`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar las estadísticas de mantenimiento.");
  return res.json();
}

export async function getRecentActivity(limit: number = 10): Promise<RecentActivityItem[]> {
  const res = await fetch(`${API_URL}/dashboard/recent-activity?limit=${limit}`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar la actividad reciente.");
  return res.json();
}
