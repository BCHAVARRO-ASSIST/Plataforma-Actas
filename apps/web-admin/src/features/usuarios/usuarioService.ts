import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "../../shared/api/httpClient";

export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso: string;
  activo: boolean;
  rolId: number | null;
  rolNombre: string | null;
}

export interface UsuarioCreateRequest {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso: string;
}

export interface UsuarioUpdateRequest {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso: string;
  activo: boolean;
}

export async function getUsuarios(): Promise<Usuario[]> {
  return apiGet<Usuario[]>("/usuarios", { errorMessage: "No fue posible obtener los usuarios." });
}

export async function createUsuario(data: unknown) {
  return apiPost("/usuarios", data, { errorMessage: "No fue posible crear el usuario." });
}

export async function updateUsuario(id: number, data: unknown) {
  return apiPut(`/usuarios/${id}`, data, { errorMessage: "No fue posible actualizar el usuario." });
}

export async function assignUsuarioRol(usuarioId: number, rolId: number | null) {
  return apiPut(`/usuarios/${usuarioId}/rol`, { rolId }, { errorMessage: "No fue posible asignar el rol al usuario." });
}

export async function setUsuarioActivo(id: number, activo: boolean): Promise<void> {
  if (activo) {
    await apiPatch(`/usuarios/${id}/activar`, undefined, { errorMessage: "No fue posible activar el usuario." });
  } else {
    await apiDelete(`/usuarios/${id}`, { errorMessage: "No fue posible desactivar el usuario." });
  }
}

export async function deleteUsuario(id: number): Promise<void> {
  return apiDelete(`/usuarios/${id}`, { errorMessage: "No fue posible eliminar el usuario." });
}

export async function resetUsuarioPassword(
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
