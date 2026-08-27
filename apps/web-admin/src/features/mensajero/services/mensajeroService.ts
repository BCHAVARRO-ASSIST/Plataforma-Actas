import type {
  MensajeroActa,
  MensajeroActaDetail,
} from "../types/mensajero";

import { API_URL } from "../../../shared/config/api";


const TOKEN_KEY =
  "plataforma_actas_token";


function getToken(): string | null {

  return localStorage.getItem(
    TOKEN_KEY
  );

}


function getAuthHeaders(): HeadersInit {

  const token =
    getToken();

  if (!token) {
    return {};
  }

  return {
    Authorization:
      `Bearer ${token}`,
  };

}


// =====================================================
// OBTENER MIS ACTAS
// =====================================================

export async function getMisActas():
  Promise<MensajeroActa[]> {

  const response =
    await fetch(
      `${API_URL}/actas/mis-actas`,
      {
        method: "GET",

        headers: {
          ...getAuthHeaders(),
        },
      }
    );


  if (!response.ok) {

    if (response.status === 401) {

      throw new Error(
        "La sesión no es válida o ha expirado."
      );

    }


    if (response.status === 403) {

      throw new Error(
        "No tienes permisos para consultar tus actas."
      );

    }


    throw new Error(
      "No fue posible obtener las actas asignadas."
    );

  }


  return response.json();

}


// =====================================================
// OBTENER DETALLE DE MI ACTA
// =====================================================

export async function getMiActaById(
  id: string
): Promise<MensajeroActaDetail> {

  const response = await fetch(
    `${API_URL}/actas/${id}`,
    {
      method: "GET",

      headers: {
        ...getAuthHeaders(),
      },
    }
  );
  
  // ---------------------------------------------------
  // RESPUESTA NO EXITOSA
  // ---------------------------------------------------

  if (!response.ok) {

    const responseText =
      await response.text();


    console.error(
      "ERROR DETALLE ACTA",
      {
        id,
        url: `${API_URL}/actas/${id}`,
        status: response.status,
        statusText: response.statusText,
        response: responseText,
      }
    );


    if (response.status === 401) {

      throw new Error(
        "La sesión no es válida o ha expirado."
      );

    }


    if (response.status === 403) {

      throw new Error(
        "El usuario no tiene permiso para consultar el detalle de esta acta."
      );

    }


    if (response.status === 404) {

      throw new Error(
        "El acta solicitada no existe."
      );

    }


    throw new Error(
      `No fue posible obtener el detalle del acta. Código HTTP: ${response.status}`
    );

  }


  // ---------------------------------------------------
  // RESPUESTA EXITOSA
  // ---------------------------------------------------

  const data =
    await response.json();


  console.log(
    "DETALLE DEL ACTA RECIBIDO",
    data
  );


  return data;
}