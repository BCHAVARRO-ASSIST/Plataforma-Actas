import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../../shared/api/httpClient";

export interface Role {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
  fechaCreacion: string;
}
export type Rol = Role;

export interface Permission {
  id: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
  modulo: string;
}
export type Permiso = Permission;

export interface RoleDetail extends Role {
  permisos: Permission[];
}
export type RolDetalle = RoleDetail;

export interface CreateRoleRequest {
  nombre: string;
  descripcion: string;
}
export type CreateRolRequest = CreateRoleRequest;

export interface UpdateRoleRequest {
  nombre: string;
  descripcion: string;
}
export type UpdateRolRequest = UpdateRoleRequest;

export async function getRoles(): Promise<Role[]> {
  return apiGet<Role[]>("/roles", { errorMessage: "No fue posible obtener los roles." });
}

export async function getPermissions(): Promise<Permission[]> {
  return apiGet<Permission[]>("/roles/permisos", { errorMessage: "No fue posible obtener los permisos." });
}
export const getPermisos = getPermissions;

export async function getRole(id: number): Promise<RoleDetail> {
  return apiGet<RoleDetail>(`/roles/${id}`, { errorMessage: "No fue posible obtener el detalle del rol." });
}
export const getRol = getRole;

export async function createRole(request: CreateRoleRequest): Promise<Role> {
  return apiPost<Role>("/roles", request, { errorMessage: "No fue posible crear el rol." });
}
export const createRol = createRole;

export async function updateRole(id: number, request: UpdateRoleRequest): Promise<Role> {
  return apiPut<Role>(`/roles/${id}`, request, { errorMessage: "No fue posible actualizar el rol." });
}
export const updateRol = updateRole;

export async function updateRolePermissions(rolId: number, permisoIds: number[]): Promise<void> {
  return apiPut<void>(`/roles/${rolId}/permisos`, { permisoIds }, {
    errorMessage: "No fue posible actualizar los permisos del rol.",
  });
}
export const updateRolPermisos = updateRolePermissions;

export async function setRoleActive(id: number, activo: boolean): Promise<void> {
  const endpoint = activo ? `/roles/${id}/activar` : `/roles/${id}/desactivar`;
  return apiPatch<void>(endpoint, undefined, {
    errorMessage: `No fue posible ${activo ? "activar" : "desactivar"} el rol.`,
  });
}
export const setRolActivo = setRoleActive;

export async function deleteRole(id: number): Promise<void> {
  return apiDelete<void>(`/roles/${id}`, { errorMessage: "No fue posible eliminar el rol." });
}
export const deleteRol = deleteRole;
