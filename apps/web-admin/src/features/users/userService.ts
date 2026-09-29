import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../../shared/api/httpClient";

export interface User {
  id: number;
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  numeroDocumento?: string | null;
  proceso: string;
  activo: boolean;
  rolId: number | null;
  rolNombre: string | null;
}
export type Usuario = User;

export interface UserCreateRequest {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  numeroDocumento?: string | null;
  proceso: string;
}
export type UsuarioCreateRequest = UserCreateRequest;

export interface UserUpdateRequest {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  numeroDocumento?: string | null;
  proceso: string;
  activo: boolean;
}
export type UsuarioUpdateRequest = UserUpdateRequest;

export async function getUsers(): Promise<User[]> {
  return apiGet<User[]>("/usuarios", { errorMessage: "No fue posible obtener los usuarios." });
}
export const getUsuarios = getUsers;

export async function createUser(data: unknown) {
  return apiPost("/usuarios", data, { errorMessage: "No fue posible crear el usuario." });
}
export const createUsuario = createUser;

export async function updateUser(id: number, data: unknown) {
  return apiPut(`/usuarios/${id}`, data, { errorMessage: "No fue posible actualizar el usuario." });
}
export const updateUsuario = updateUser;

export async function assignUserRole(usuarioId: number, rolId: number | null) {
  return apiPut(`/usuarios/${usuarioId}/rol`, { rolId }, { errorMessage: "No fue posible asignar el rol al usuario." });
}
export const assignUsuarioRol = assignUserRole;

export async function setUserActive(id: number, activo: boolean): Promise<void> {
  if (activo) {
    await apiPatch(`/usuarios/${id}/activar`, undefined, { errorMessage: "No fue posible activar el usuario." });
  } else {
    await apiDelete(`/usuarios/${id}`, { errorMessage: "No fue posible desactivar el usuario." });
  }
}
export const setUsuarioActivo = setUserActive;

export async function deleteUser(id: number): Promise<void> {
  return apiDelete(`/usuarios/${id}`, { errorMessage: "No fue posible eliminar el usuario." });
}
export const deleteUsuario = deleteUser;

export async function resetUserPassword(
  id: number,
  newPassword: string,
  confirmPassword: string
): Promise<{ message: string }> {
  return apiPut<{ message: string }>(
    `/usuarios/${id}/password`,
    { newPassword, confirmPassword },
    { errorMessage: "No fue posible restablecer la contraseña del usuario." }
  );
}
export const resetUsuarioPassword = resetUserPassword;
