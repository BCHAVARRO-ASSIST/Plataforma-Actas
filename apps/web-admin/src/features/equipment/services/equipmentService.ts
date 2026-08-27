import type {
  Equipment,
  CreateEquipmentRequest,
  UpdateEquipmentRequest,
} from "../types/equipment";

import { API_URL } from "../../../shared/config/api";

const TOKEN_KEY = "plataforma_actas_token";

function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function getAuthHeaders(): HeadersInit {
  const token = getToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

// =====================================================
// OBTENER INVENTARIO
// =====================================================

export async function getEquipment(): Promise<Equipment[]> {
  const response = await fetch(`${API_URL}/equipment`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para consultar el inventario.");
  }

  if (!response.ok) {
    throw new Error("No fue posible obtener el inventario.");
  }

  return response.json();
}

// =====================================================
// CREAR EQUIPO
// =====================================================

export async function createEquipment(
  data: CreateEquipmentRequest
): Promise<Equipment> {
  const response = await fetch(`${API_URL}/equipment`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(data),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para registrar equipos.");
  }

  if (response.status === 409) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "Ya existe un equipo con ese código de inventario.");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible registrar el equipo.");
  }

  return response.json();
}

// =====================================================
// ACTUALIZAR EQUIPO
// =====================================================

export async function updateEquipment(
  id: string,
  data: UpdateEquipmentRequest
): Promise<Equipment> {
  const response = await fetch(`${API_URL}/equipment/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(data),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para editar equipos.");
  }

  if (response.status === 409) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "Ya existe otro equipo con ese código de inventario.");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible actualizar el equipo.");
  }

  return response.json();
}

// =====================================================
// DESACTIVAR EQUIPO
// =====================================================

export async function deactivateEquipment(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/equipment/${id}/deactivate`, {
    method: "PATCH",
    headers: getAuthHeaders(),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para desactivar equipos.");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible desactivar el equipo.");
  }
}

// =====================================================
// ACTIVAR EQUIPO
// =====================================================

export async function activateEquipment(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/equipment/${id}/activate`, {
    method: "PATCH",
    headers: getAuthHeaders(),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para activar equipos.");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible activar el equipo.");
  }
}

// =====================================================
// ELIMINAR EQUIPO (INACTIVOS)
// =====================================================

export async function deleteEquipment(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/equipment/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

  if (response.status === 401) {
    throw new Error("La sesión no es válida o ha expirado.");
  }

  if (response.status === 403) {
    throw new Error("No tienes permisos para eliminar equipos.");
  }

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible eliminar el equipo.");
  }
}

// =====================================================
// MANTENIMIENTOS
// =====================================================

export interface MaintenanceRecord {
  id: string;
  inventoryKey: string;
  nombreEquipo: string;
  marca: string | null;
  asignadoA: string | null;
  departamento: string | null;
  estatusEquipo: string | null;
  tipoMantenimiento: string | null;
  fechaMantenimiento: string | null;
  mantenimientoRealizado: boolean;
}

export async function getMaintenanceRecords(): Promise<MaintenanceRecord[]> {
  const response = await fetch(`${API_URL}/equipment/mantenimientos`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error("No fue posible obtener los registros de mantenimiento.");
  }

  return response.json();
}

export async function updateEquipmentMaintenance(
  id: string,
  data: {
    tipoMantenimiento: string;
    fechaMantenimiento: string | null;
    mantenimientoRealizado: boolean;
  }
): Promise<void> {
  const response = await fetch(`${API_URL}/equipment/${id}/mantenimiento`, {
    method: "PATCH",
    headers: getAuthHeaders(),
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.message || "No fue posible actualizar el mantenimiento.");
  }
}