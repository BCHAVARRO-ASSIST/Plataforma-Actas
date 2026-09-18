import { apiGet, apiPost, apiGetBlob } from "../../shared/api/httpClient";
import type { Equipo } from "../equipos/equipoService";

export interface Acta {
  id: string;
  numeroActa: string;
  equipmentId: string;
  inventoryKey: string | null;
  nombreEquipo: string | null;
  usuarioId: number;
  destinatarioUsuarioId: number | null;
  mensajeroUsuarioId: number | null;
  actaEntregada: boolean;
  estado: string;
  tipoActa: "Entrega" | "Devolución";
  actaOrigenId: string | null;
  actaOrigenNumero?: string | null;
  rutaDocumento: string | null;
  fechaFirma: string | null;
  fechaCreacion: string;
  fechaAnulacion: string | null;
  fechaFinalizacion: string | null;
  deviceId: string | null;
  formDataJson: string;
  signatureDataJson?: string | null;
  hashSha256: string | null;
  motivoAnulacion: string | null;
}

export interface ActaDetail extends Omit<Acta, "equipmentId"> {
  equipo: Equipo;
  destinatarioNombre?: string | null;
  destinatarioCorreo?: string | null;
  destinatarioCargo?: string | null;
  mensajeroNombre?: string | null;
  mensajeroCorreo?: string | null;
}

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

export interface FirmarActaRequest {
  signatureDataJson: string;
  observacion?: string;
  correosNotificacionAdicionales?: string[];
}

export interface GenerarDocumentoResponse {
  actaId: string;
  numeroActa: string;
  rutaDocumento: string;
  hashSha256: string;
  fechaGeneracion: string;
  tamanoBytes: number;
}

// =====================================================
// OPERACIONES DE SERVICIO ESTANDARIZADAS
// =====================================================

export async function getActas(): Promise<Acta[]> {
  return apiGet<Acta[]>("/actas", {
    errorMessage: "No fue posible obtener las actas.",
  });
}

export async function getMisActas(): Promise<Acta[]> {
  return apiGet<Acta[]>("/actas/mis-actas", {
    errorMessage: "No fue posible obtener las actas del mensajero.",
  });
}

export async function getActaById(id: string): Promise<ActaDetail> {
  return apiGet<ActaDetail>(`/actas/${id}`, {
    errorMessage: "No fue posible obtener el acta.",
  });
}

export async function createActa(request: CreateActaRequest): Promise<Acta> {
  return apiPost<Acta>("/actas", request, {
    errorMessage: "No fue posible crear el acta.",
  });
}

export async function signActa(id: string, request: FirmarActaRequest): Promise<Acta> {
  return apiPost<Acta>(`/actas/${id}/firmar`, request, {
    errorMessage: "No fue posible firmar el acta.",
  });
}

export async function finalizeActa(id: string): Promise<Acta> {
  return apiPost<Acta>(`/actas/${id}/finalizar`, undefined, {
    errorMessage: "No fue posible finalizar el acta.",
  });
}

export async function cancelActa(id: string, motivo: string): Promise<Acta> {
  return apiPost<Acta>(`/actas/${id}/anular`, { motivo }, {
    errorMessage: "No fue posible anular el acta.",
  });
}

export async function previewActaDocumento(id: string): Promise<Blob> {
  return apiGetBlob(`/actas/${id}/pdf`, {
    errorMessage: "No fue posible generar la vista previa del documento.",
  });
}

export async function downloadActaDocumento(id: string, numeroActa?: string): Promise<void> {
  const blob = await apiGetBlob(`/actas/${id}/descargar-pdf`, {
    errorMessage: "No fue posible descargar el documento oficial.",
  });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `Acta_${numeroActa || id}.pdf`;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}

export async function generateActaDocumento(id: string): Promise<GenerarDocumentoResponse> {
  return apiPost<GenerarDocumentoResponse>(`/actas/${id}/generar-pdf`, undefined, {
    errorMessage: "No fue posible generar el documento oficial.",
  });
}

// =====================================================
// ALIASES DE COMPATIBILIDAD RETROACTIVA
// =====================================================
export const firmarActa = signActa;
export const finalizarActa = finalizeActa;
export const anularActa = cancelActa;
export const descargarActaDocumento = downloadActaDocumento;
export const generarActaDocumento = generateActaDocumento;
