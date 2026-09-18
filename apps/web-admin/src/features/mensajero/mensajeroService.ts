import { apiGet } from "../../shared/api/httpClient";
import type { Equipo } from "../equipos/equipoService";

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

/**
 * Homogeneización con el modelo canónico de dominio Equipo (UDL).
 */
export type MensajeroEquipo = Equipo;

export interface MensajeroActaDetail extends MensajeroActa {
  equipo: Equipo;
  usuarioId: number;
  destinatarioUsuarioId: number | null;
  destinatarioNombre?: string | null;
  destinatarioCorreo?: string | null;
  destinatarioCargo?: string | null;
  mensajeroUsuarioId: number | null;
  deviceId: string | null;
  formDataJson: string;
  rutaDocumento: string | null;
  hashSha256: string | null;
  motivoAnulacion: string | null;
  fechaFinalizacion: string | null;
  fechaAnulacion: string | null;
}

export async function getMisActas(): Promise<MensajeroActa[]> {
  return apiGet<MensajeroActa[]>("/actas/mis-actas", {
    errorMessage: "No fue posible obtener las actas asignadas.",
  });
}

export async function getMiActaById(id: string): Promise<MensajeroActaDetail> {
  return apiGet<MensajeroActaDetail>(`/actas/${id}`, {
    errorMessage: "No fue posible obtener el detalle del acta.",
  });
}
