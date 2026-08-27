import type { Usuario } from "../types/usuario";

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
// PROCESAR ERROR HTTP
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

    if (
      text.trim()
    ) {

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

    // No hacer nada.
  }


  return defaultMessage;

}


// =====================================================
// OBTENER USUARIOS
// =====================================================

export async function getUsuarios():
  Promise<Usuario[]> {

  const response =
    await fetch(
      `${API_URL}/usuarios`,
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
        "No fue posible obtener los usuarios."
      )
    );

  }


  return response.json();

}


// =====================================================
// CREAR USUARIO
// =====================================================

export async function createUsuario(
  data: unknown
) {

  const response =
    await fetch(
      `${API_URL}/usuarios`,
      {
        method: "POST",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify(
            data
          ),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible crear el usuario."
      )
    );

  }


  return response.json();

}


// =====================================================
// ACTUALIZAR USUARIO
// =====================================================

export async function updateUsuario(
  id: number,
  data: unknown
) {

  const response =
    await fetch(
      `${API_URL}/usuarios/${id}`,
      {
        method: "PUT",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify(
            data
          ),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible actualizar el usuario."
      )
    );

  }


  return response.json();

}


// =====================================================
// ASIGNAR ROL A USUARIO
// =====================================================

export async function assignUsuarioRol(
  usuarioId: number,
  rolId: number | null
) {

  const response =
    await fetch(
      `${API_URL}/usuarios/${usuarioId}/rol`,
      {
        method: "PUT",

        headers:
          getAuthHeaders(true),

        body:
          JSON.stringify({
            rolId,
          }),

      }
    );


  if (!response.ok) {

    throw new Error(
      await getErrorMessage(
        response,
        "No fue posible asignar el rol al usuario."
      )
    );

  }


  return response.json();

}


// =====================================================
// ACTIVAR / DESACTIVAR USUARIO
// =====================================================

export async function setUsuarioActivo(
  id: number,
  activo: boolean
) {

  const response =
    await fetch(
      `${API_URL}/usuarios/${id}/estado`,
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
          ? "No fue posible activar el usuario."
          : "No fue posible desactivar el usuario."
      )
    );

  }

}


// =====================================================
// ELIMINAR USUARIO
// =====================================================

export async function deleteUsuario(
  id: number
): Promise<void> {

  const response =
    await fetch(
      `${API_URL}/usuarios/${id}`,
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
        "No fue posible eliminar el usuario."
      )
    );

  }

}