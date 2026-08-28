import { API_URL } from "../../shared/config/api";

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

const TOKEN_KEY = "plataforma_actas_token";

function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function getAuthHeaders(includeJson: boolean = false): HeadersInit {
  const token = getToken();
  const headers: Record<string, string> = {};
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  if (includeJson) {
    headers["Content-Type"] = "application/json";
  }
  return headers;
}

async function getErrorMessage(response: Response, defaultMessage: string): Promise<string> {
  if (response.status === 401) {
    return "La sesión no es válida o ha expirado.";
  }
  if (response.status === 403) {
    return "No tienes permisos para realizar esta operación.";
  }
  if (response.status === 404) {
    return "El recurso solicitado no existe.";
  }
  try {
    const text = await response.text();
    if (text.trim()) {
      try {
        const data = JSON.parse(text);
        if (typeof data === "string") return data;
        if (data.message) return data.message;
        if (data.title) return data.title;
      } catch {
        return text;
      }
    }
  } catch {
    // Ignorar error de lectura
  }
  return defaultMessage;
}

export async function getRoles(): Promise<Rol[]> {
  const response = await fetch(`${API_URL}/roles`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible obtener los roles."));
  }
  return response.json();
}

export async function getPermisos(): Promise<Permiso[]> {
  const response = await fetch(`${API_URL}/roles/permisos`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible obtener los permisos."));
  }
  return response.json();
}

export async function getRol(id: number): Promise<RolDetalle> {
  const response = await fetch(`${API_URL}/roles/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible obtener el detalle del rol."));
  }
  return response.json();
}

export async function createRol(request: CreateRolRequest): Promise<Rol> {
  const response = await fetch(`${API_URL}/roles`, {
    method: "POST",
    headers: getAuthHeaders(true),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible crear el rol."));
  }
  return response.json();
}

export async function updateRol(id: number, request: UpdateRolRequest): Promise<Rol> {
  const response = await fetch(`${API_URL}/roles/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(true),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible actualizar el rol."));
  }
  return response.json();
}

export async function updateRolPermisos(rolId: number, permisoIds: number[]): Promise<void> {
  const response = await fetch(`${API_URL}/roles/${rolId}/permisos`, {
    method: "PUT",
    headers: getAuthHeaders(true),
    body: JSON.stringify({ permisoIds }),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible actualizar los permisos del rol."));
  }
}

export async function setRolActivo(id: number, activo: boolean): Promise<void> {
  const url = activo ? `${API_URL}/roles/${id}/activar` : `${API_URL}/roles/${id}/desactivar`;
  const response = await fetch(url, {
    method: "PATCH",
    headers: getAuthHeaders(true),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, activo ? "No fue posible activar el rol." : "No fue posible desactivar el rol."));
  }
}

export async function deleteRol(id: number): Promise<void> {
  const response = await fetch(`${API_URL}/roles/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response, "No fue posible eliminar el rol."));
  }
}
