import { API_URL } from "../../../shared/config/api";
import type {
  DashboardKpis,
  StatusCountItem,
  MessengerDeliveryItem,
  TimelinePointItem,
  MaintenanceStats,
  RecentActivityItem,
} from "../types/dashboard";

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
  const res = await fetch(`${API_URL}/dashboard/equipment-status`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar el estado de equipos.");
  return res.json();
}

export async function getActasStatusChart(): Promise<StatusCountItem[]> {
  const res = await fetch(`${API_URL}/dashboard/actas-status`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar el estado de actas.");
  return res.json();
}

export async function getDeliveriesByMessengerChart(): Promise<MessengerDeliveryItem[]> {
  const res = await fetch(`${API_URL}/dashboard/deliveries-by-messenger`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar las entregas por mensajero.");
  return res.json();
}

export async function getTimelineChart(range: string = "30d"): Promise<TimelinePointItem[]> {
  const res = await fetch(`${API_URL}/dashboard/timeline?range=${encodeURIComponent(range)}`, {
    headers: getAuthHeaders(),
  });
  if (!res.ok) throw new Error("No fue posible cargar la línea temporal.");
  return res.json();
}

export async function getMaintenanceStats(): Promise<MaintenanceStats> {
  const res = await fetch(`${API_URL}/dashboard/maintenance`, {
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
