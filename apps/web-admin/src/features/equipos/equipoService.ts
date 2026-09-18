import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../../shared/api/httpClient";

/**
 * Entidad canónica de dominio Equipo (UDL - Ubiquitous Domain Language).
 */
export interface Equipo {
  id: string;
  inventoryKey: string;
  nombreEquipo: string;
  empresa: string | null;
  asignadoA: string | null;
  proveedor: string | null;
  marca: string | null;
  procesador: string | null;
  discoDuro: string | null;
  memoriaRam: string | null;
  departamento: string | null;
  estatusEquipo: string | null;
}

/**
 * Alias temporal de compatibilidad durante la transición arquitectónica.
 */
export type Equipment = Equipo;

export interface CreateEquipoRequest {
  inventoryKey: string;
  nombreEquipo: string;
  empresa?: string | null;
  asignadoA?: string | null;
  proveedor?: string | null;
  marca?: string | null;
  procesador?: string | null;
  discoDuro?: string | null;
  memoriaRam?: string | null;
  departamento?: string | null;
  estatusEquipo?: string | null;
}
export type CreateEquipmentRequest = CreateEquipoRequest;

export interface UpdateEquipoRequest {
  inventoryKey: string;
  nombreEquipo: string;
  empresa?: string | null;
  asignadoA?: string | null;
  proveedor?: string | null;
  marca?: string | null;
  procesador?: string | null;
  discoDuro?: string | null;
  memoriaRam?: string | null;
  departamento?: string | null;
  estatusEquipo?: string | null;
}
export type UpdateEquipmentRequest = UpdateEquipoRequest;

export interface RegistroMantenimiento {
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
export type MaintenanceRecord = RegistroMantenimiento;

export interface DriveSyncResult {
  success: boolean;
  message: string;
  totalProcessed: number;
  created: number;
  updated: number;
  unchanged: number;
  timestamp: string;
}

// =====================================================
// OPERACIONES DE SERVICIO CANÓNICAS (RUTA: /equipos)
// =====================================================

export async function getEquipos(): Promise<Equipo[]> {
  return apiGet<Equipo[]>("/equipos", {
    errorMessage: "No fue posible obtener el inventario de equipos.",
  });
}

export async function getEquipoById(id: string): Promise<Equipo> {
  return apiGet<Equipo>(`/equipos/${id}`, {
    errorMessage: "No fue posible obtener el detalle del equipo.",
  });
}

export async function createEquipo(data: CreateEquipoRequest): Promise<Equipo> {
  return apiPost<Equipo>("/equipos", data, {
    errorMessage: "No fue posible registrar el equipo.",
  });
}

export async function updateEquipo(id: string, data: UpdateEquipoRequest): Promise<Equipo> {
  return apiPut<Equipo>(`/equipos/${id}`, data, {
    errorMessage: "No fue posible actualizar el equipo.",
  });
}

export async function deactivateEquipo(id: string): Promise<void> {
  return apiPatch<void>(`/equipos/${id}/deactivate`, undefined, {
    errorMessage: "No fue posible desactivar el equipo.",
  });
}

export async function activateEquipo(id: string): Promise<void> {
  return apiPatch<void>(`/equipos/${id}/activate`, undefined, {
    errorMessage: "No fue posible activar el equipo.",
  });
}

export async function deleteEquipo(id: string): Promise<void> {
  return apiDelete<void>(`/equipos/${id}`, {
    errorMessage: "No fue posible eliminar el equipo.",
  });
}

export async function getMantenimientos(): Promise<RegistroMantenimiento[]> {
  return apiGet<RegistroMantenimiento[]>("/equipos/mantenimientos", {
    errorMessage: "No fue posible obtener los registros de mantenimiento.",
  });
}

export async function updateEquipoMantenimiento(
  id: string,
  data: {
    tipoMantenimiento: string;
    fechaMantenimiento: string | null;
    mantenimientoRealizado: boolean;
  }
): Promise<void> {
  return apiPatch<void>(`/equipos/${id}/mantenimiento`, data, {
    errorMessage: "No fue posible actualizar el mantenimiento.",
  });
}

export async function syncGoogleDriveInventory(): Promise<DriveSyncResult> {
  return apiPost<DriveSyncResult>("/integrations/google-drive/sync", undefined, {
    errorMessage: "Error al sincronizar con Google Drive.",
  });
}

// =====================================================
// ALIASES DE COMPATIBILIDAD RETROACTIVA
// =====================================================
export const getEquipment = getEquipos;
export const createEquipment = createEquipo;
export const updateEquipment = updateEquipo;
export const deactivateEquipment = deactivateEquipo;
export const activateEquipment = activateEquipo;
export const deleteEquipment = deleteEquipo;
export const getMaintenanceRecords = getMantenimientos;
export const updateEquipmentMaintenance = updateEquipoMantenimiento;
