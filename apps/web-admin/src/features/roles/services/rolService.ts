import type {
  Permiso,
  Rol,
  RolDetalle,
} from "../types/rol";

import { API_URL } from "../../../shared/config/api";


// =====================================================
// AUTENTICACIÓN
// =====================================================

const TOKEN_KEY =
  "plataforma_actas_token";


// =====================================================
// OBTENER TOKEN
// =====================================================

function getToken(): string | null {

  return localStorage.getItem(
    TOKEN_KEY
  );

}


// =====================================================
// HEADERS AUTENTICADOS
// =====================================================

function getAuthHeaders(
  includeJson: boolean = false
): HeadersInit {

  const token =
    getToken();

  const headers: Record<
    string,
    string
  > = {};

  if (token) {

    headers.Authorization =
      `Bearer ${token}`;

  }

  if (includeJson) {

    headers["Content-Type"] =
      "application/json";

  }

  return headers;

}


// =====================================================
// PROCESAR ERRORES
// =====================================================

async function getErrorMessage(
  response: Response,
  defaultMessage: string
): Promise<string> {

  if (
    response.status === 401
  ) {

    return (
      "La sesión no es válida o ha expirado."
    );

  }


  if (
    response.status === 403
  ) {

    return (
      "No tienes permisos para realizar esta operación."
    );

  }


  if (
    response.status === 404
  ) {

    return (
      "El recurso solicitado no existe."
    );

  }


  try {

    const text =
      await response.text();

    if (text.trim()) {

      try {

        const data =
          JSON.parse(text);

        if (
          typeof data === "string"
        ) {

          return data;

        }

        if (
          data.message
        ) {

          return data.message;

        }

        if (
          data.title
        ) {

          return data.title;

        }

      } catch {

        return text;

      }

    }

  } catch {

    // Ignorar error de lectura
  }


  return defaultMessage;

}


// =====================================================
// OBTENER ROLES
// =====================================================

export async function getRoles():
  Promise<Rol[]> {

  const response =
    await fetch(
      `${API_URL}/roles`,
      {
        method: "GET",

        headers:
          getAuthHeaders(),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible obtener los roles."
      )
    );

  }


  return response.json();

}


// =====================================================
// OBTENER PERMISOS
// =====================================================

export async function getPermisos():
  Promise<Permiso[]> {

  const response =
    await fetch(
      `${API_URL}/permisos`,
      {
        method: "GET",

        headers:
          getAuthHeaders(),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible obtener los permisos."
      )
    );

  }


  return response.json();

}


// =====================================================
// OBTENER DETALLE DE ROL
// =====================================================

export async function getRol(
  id: number
):
  Promise<RolDetalle> {

  const response =
    await fetch(
      `${API_URL}/roles/${id}`,
      {
        method: "GET",

        headers:
          getAuthHeaders(),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible obtener el detalle del rol."
      )
    );

  }


  return response.json();

}


// =====================================================
// CREAR ROL
// =====================================================

export interface CreateRolRequest {

  nombre: string;

  descripcion: string;

}


export async function createRol(
  request: CreateRolRequest
):
  Promise<Rol> {

  const response =
    await fetch(
      `${API_URL}/roles`,
      {
        method: "POST",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify(
            request
          ),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible crear el rol."
      )
    );

  }


  return response.json();

}


// =====================================================
// ACTUALIZAR ROL
// =====================================================

export interface UpdateRolRequest {

  nombre: string;

  descripcion: string;

}


export async function updateRol(
  id: number,
  request: UpdateRolRequest
):
  Promise<Rol> {

  const response =
    await fetch(
      `${API_URL}/roles/${id}`,
      {
        method: "PUT",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify(
            request
          ),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible actualizar el rol."
      )
    );

  }


  return response.json();

}


// =====================================================
// ACTUALIZAR PERMISOS DEL ROL
// =====================================================

export async function updateRolPermisos(
  rolId: number,
  permisoIds: number[]
):
  Promise<void> {

  const response =
    await fetch(
      `${API_URL}/roles/${rolId}/permisos`,
      {
        method: "PUT",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify({
            permisoIds,
          }),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible actualizar los permisos del rol."
      )
    );

  }

}


// =====================================================
// ACTIVAR / DESACTIVAR ROL
// =====================================================

export async function setRolActivo(
  id: number,
  activo: boolean
):
  Promise<void> {

  const response =
    await fetch(
      `${API_URL}/roles/${id}/estado`,
      {
        method: "PATCH",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify({
            activo,
          }),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        activo
          ? "No fue posible activar el rol."
          : "No fue posible desactivar el rol."
      )
    );

  }

}


// =====================================================
// ELIMINAR ROL
// =====================================================

export async function deleteRol(
  id: number
):
  Promise<void> {

  const response =
    await fetch(
      `${API_URL}/roles/${id}`,
      {
        method: "DELETE",

        headers:
          getAuthHeaders(),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible eliminar el rol."
      )
    );

  }

}