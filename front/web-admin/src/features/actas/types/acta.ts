export interface Acta {
  id: string;

  /**
   * Número visible del acta.
   * Ejemplo: ACT-2026-000001
   */
  numeroActa: string;

  equipmentId: string;

  /**
   * Información amigable del equipo devuelta por la API.
   */
  inventoryKey: string | null;
  nombreEquipo: string | null;

  usuarioId: number;
  destinatarioUsuarioId: number | null;
  mensajeroUsuarioId: number | null;

  actaEntregada: boolean;

  estado: string;

  /**
   * Tipo de operación del acta.
   */
  tipoActa: "Entrega" | "Devolución";

  /**
   * ID interno del acta de entrega
   * relacionada con una devolución.
   */
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

export interface ActaEquipment {
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

export interface ActaDetail
  extends Omit<Acta, "equipmentId">
{
  equipo: ActaEquipment;
  destinatarioNombre?: string | null;
  destinatarioCorreo?: string | null;
  destinatarioCargo?: string | null;
  mensajeroNombre?: string | null;
  mensajeroCorreo?: string | null;
}