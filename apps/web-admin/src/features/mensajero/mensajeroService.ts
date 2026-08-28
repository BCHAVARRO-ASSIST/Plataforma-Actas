import { API_URL } from "../../shared/config/api";

export interface MensajeroActa {
  id: string;
  numeroActa: string;
  tipoActa: "Entrega" | "Devolución";
  estado: string;
  actaEntregada: boolean;
  equipmentId: string;
  inventoryKey: string | null;
  nombreEquipo: string | null;
  fechaCreacion: string;
  fechaFirma: string | null;
  actaOrigenId: string | null;
}

export interface MensajeroEquipo {
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

export interface MensajeroActaDetail extends MensajeroActa {
  equipo: MensajeroEquipo;
  usuarioId: number;
  destinatarioUsuarioId: number | null;
  mensajeroUsuarioId: number | null;
  deviceId: string | null;
  formDataJson: string;
  rutaDocumento: string | null;
  hashSha256: string | null;
  motivoAnulacion: string | null;
  fechaFinalizacion: string | null;
  fechaAnulacion: string | null;
}

const TOKEN_KEY = "plataforma_actas_token";

function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function getAuthHeaders(): HeadersInit {
  const token = getToken();
  if (!token) return {};
  return {
    Authorization: `Bearer ${token}`,
  };
}

export async function getMisActas(): Promise<MensajeroActa[]> {
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
      throw new Error("No tienes permisos para consultar tus actas.");
    }
    throw new Error("No fue posible obtener las actas asignadas.");
  }

  return response.json();
}

export async function getMiActaById(id: string): Promise<MensajeroActaDetail> {
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
      throw new Error("El usuario no tiene permiso para consultar el detalle de esta acta.");
    }
    if (response.status === 404) {
      throw new Error("El acta solicitada no existe.");
    }
    throw new Error(`No fue posible obtener el detalle del acta. Código HTTP: ${response.status}`);
  }

  return response.json();
}
