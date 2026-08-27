import type {
  Acta,
  ActaDetail,
} from "../types/acta";

import { API_URL } from "../../../shared/config/api";


// =====================================================
// CONFIGURACIÓN DE AUTENTICACIÓN
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

function getAuthHeaders(): HeadersInit {
  const token =
    getToken();

  return token
    ? {
        Authorization:
          `Bearer ${token}`,
      }
    : {};
}


// =====================================================
// REQUEST PARA CREAR ACTA
// =====================================================

export interface CreateActaRequest {
  equipmentId: string;
  usuarioId: number;
  destinatarioUsuarioId: number | null;
  mensajeroUsuarioId: number | null;
  actaEntregada: boolean;
  deviceId: string | null;
  formDataJson: string;
  tipoActa: "Entrega" | "Devolución";
  actaOrigenId: string | null;
}


// =====================================================
// REQUEST PARA FIRMAR ACTA
// =====================================================

export interface FirmarActaRequest {
  signatureDataJson: string;
}


// =====================================================
// OBTENER TODAS LAS ACTAS
// =====================================================

export async function getActas(): Promise<Acta[]> {
  const response = await fetch(`${API_URL}/actas`, {
    method: "GET",
    headers: {
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para consultar las actas.");
    }
    throw new Error("No fue posible obtener las actas.");
  }

  return response.json();
}


// =====================================================
// OBTENER MIS ACTAS COMO MENSAJERO
// =====================================================

export async function getMisActas(): Promise<Acta[]> {
  const response = await fetch(`${API_URL}/actas/mis-actas`, {
    method: "GET",
    headers: {
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para consultar las actas del mensajero.");
    }
    throw new Error("No fue posible obtener las actas del mensajero.");
  }

  return response.json();
}


// =====================================================
// OBTENER DETALLE DE ACTA
// =====================================================

export async function getActaById(id: string): Promise<ActaDetail> {
  const response = await fetch(`${API_URL}/actas/${id}`, {
    method: "GET",
    headers: {
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para consultar esta acta.");
    }
    if (response.status === 404) {
      throw new Error("El acta no existe.");
    }
    throw new Error("No fue posible obtener el acta.");
  }

  return response.json();
}


// =====================================================
// CREAR ACTA
// =====================================================

export async function createActa(request: CreateActaRequest): Promise<Acta> {
  const response = await fetch(`${API_URL}/actas`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const errorJson = await response.json().catch(() => ({}));
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para registrar actas.");
    }
    if (response.status === 409) {
      throw new Error(errorJson.message || "El acta no puede ser creada en su estado actual.");
    }
    throw new Error(errorJson.message || "No fue posible crear el acta.");
  }

  return response.json();
}


// =====================================================
// FIRMAR ACTA
// =====================================================

export async function firmarActa(
  id: string,
  request: FirmarActaRequest
): Promise<Acta> {
  const response = await fetch(`${API_URL}/actas/${id}/firmar`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const errorJson = await response.json().catch(() => ({}));
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para firmar el acta.");
    }
    if (response.status === 404) {
      throw new Error("El acta no existe.");
    }
    if (response.status === 409) {
      throw new Error(errorJson.message || "El acta no puede ser firmada en su estado actual.");
    }
    throw new Error(errorJson.message || "No fue posible firmar el acta.");
  }

  return response.json();
}


// =====================================================
// FINALIZAR ACTA
// =====================================================

export async function finalizarActa(id: string): Promise<Acta> {
  const response = await fetch(`${API_URL}/actas/${id}/finalizar`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    const errorJson = await response.json().catch(() => ({}));
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para finalizar el acta.");
    }
    if (response.status === 409) {
      throw new Error(errorJson.message || "El acta no puede ser finalizada en su estado actual.");
    }
    throw new Error(errorJson.message || "No fue posible finalizar el acta.");
  }

  return response.json();
}


// =====================================================
// ANULAR ACTA
// =====================================================

export async function anularActa(id: string, motivo: string): Promise<Acta> {
  const response = await fetch(`${API_URL}/actas/${id}/anular`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify({ motivo }),
  });

  if (!response.ok) {
    const errorJson = await response.json().catch(() => ({}));
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para anular el acta.");
    }
    if (response.status === 409) {
      throw new Error(errorJson.message || "El acta no puede ser anulada en su estado actual.");
    }
    throw new Error(errorJson.message || "No fue posible anular el acta.");
  }

  return response.json();
}


// =====================================================
// OBTENER VISTA PREVIA DEL DOCUMENTO OFICIAL (BLOB PDF)
// =====================================================

export async function previewActaDocumento(id: string): Promise<Blob> {
  const response = await fetch(`${API_URL}/actas/${id}/documento/preview`, {
    method: "GET",
    headers: {
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para visualizar este documento.");
    }
    if (response.status === 404) {
      throw new Error("El documento o acta solicitada no existe.");
    }
    throw new Error("No fue posible generar la vista previa del documento.");
  }

  return response.blob();
}


// =====================================================
// DESCARGAR DOCUMENTO OFICIAL (PDF)
// =====================================================

export async function descargarActaDocumento(
  id: string,
  numeroActa?: string
): Promise<void> {
  const response = await fetch(`${API_URL}/actas/${id}/documento/descargar`, {
    method: "GET",
    headers: {
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para descargar este documento.");
    }
    if (response.status === 404) {
      throw new Error("El documento o acta solicitada no existe.");
    }
    throw new Error("No fue posible descargar el documento oficial.");
  }

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `Acta_${numeroActa || id}.pdf`;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}


// =====================================================
// GENERAR Y PERSISTIR DOCUMENTO OFICIAL
// =====================================================

export interface GenerarDocumentoResponse {
  actaId: string;
  numeroActa: string;
  rutaDocumento: string;
  hashSha256: string;
  fechaGeneracion: string;
  tamanoBytes: number;
}

export async function generarActaDocumento(
  id: string
): Promise<GenerarDocumentoResponse> {
  const response = await fetch(`${API_URL}/actas/${id}/documento/generar`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  });

  if (!response.ok) {
    const errorJson = await response.json().catch(() => ({}));
    if (response.status === 401) {
      throw new Error("La sesión no es válida o ha expirado.");
    }
    if (response.status === 403) {
      throw new Error("No tiene permisos para generar documentos de actas.");
    }
    throw new Error(errorJson.message || "No fue posible generar el documento oficial.");
  }

  return response.json();
}