import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../../shared/api/httpClient";

export interface Rol {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
  fechaCreacion: string;
}

export interface Permiso {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
  modulo: string;
}

export interface RolDetalle extends Rol {
  permisos: Permiso[];
}

export interface CreateRolRequest {
  nombre: string;
  descripcion: string;
}

export interface UpdateRolRequest {
  nombre: string;
  descripcion: string;
}

export async function getRoles(): Promise<Rol[]> {
  return apiGet<Rol[]>("/roles", { errorMessage: "No fue posible obtener los roles." });
}

export async function getPermisos(): Promise<Permiso[]> {
  return apiGet<Permiso[]>("/roles/permisos", { errorMessage: "No fue posible obtener los permisos." });
}

export async function getRol(id: number): Promise<RolDetalle> {
  return apiGet<RolDetalle>(`/roles/${id}`, { errorMessage: "No fue posible obtener el detalle del rol." });
}

export async function createRol(request: CreateRolRequest): Promise<Rol> {
  return apiPost<Rol>("/roles", request, { errorMessage: "No fue posible crear el rol." });
}

export async function updateRol(id: number, request: UpdateRolRequest): Promise<Rol> {
  return apiPut<Rol>(`/roles/${id}`, request, { errorMessage: "No fue posible actualizar el rol." });
}

export async function updateRolPermisos(rolId: number, permisoIds: number[]): Promise<void> {
  return apiPut<void>(`/roles/${rolId}/permisos`, { permisoIds }, {
    errorMessage: "No fue posible actualizar los permisos del rol.",
  });
}

export async function setRolActivo(id: number, activo: boolean): Promise<void> {
  const endpoint = activo ? `/roles/${id}/activar` : `/roles/${id}/desactivar`;
  return apiPatch<void>(endpoint, undefined, {
    errorMessage: activo ? "No fue posible activar el rol." : "No fue posible desactivar el rol.",
  });
}

export async function deleteRol(id: number): Promise<void> {
  return apiDelete<void>(`/roles/${id}`, { errorMessage: "No fue posible eliminar el rol." });
}
