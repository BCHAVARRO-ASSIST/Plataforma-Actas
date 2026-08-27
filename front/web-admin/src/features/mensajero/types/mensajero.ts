// =====================================================
// TIPOS DEL MÓDULO MENSAJERO
// Plataforma de Actas
// =====================================================

export interface MensajeroActa {
  id: string;

  /**
   * Número visible del acta.
   * Ejemplo:
   * ACT-2026-000006
   */
  numeroActa: string;

  /**
   * Tipo de operación.
   */
  tipoActa:
    | "Entrega"
    | "Devolución";

  /**
   * Estado actual del acta.
   */
  estado: string;

  /**
   * Indica si el acta ya fue entregada.
   */
  actaEntregada: boolean;

  /**
   * Equipo asociado al acta.
   */
  equipmentId: string;

  /**
   * Identificador visible del inventario.
   * Ejemplo:
   * 5CD7483MNS
   */
  inventoryKey: string | null;

  /**
   * Nombre del equipo.
   */
  nombreEquipo: string | null;


  /**
   * Fecha de creación.
   */
  fechaCreacion: string;

  /**
   * Fecha de firma.
   */
  fechaFirma: string | null;

  /**
   * Identificador del acta de origen.
   *
   * Se utiliza principalmente
   * para devoluciones.
   */
  actaOrigenId: string | null;
}


// =====================================================
// DETALLE DEL EQUIPO
// =====================================================

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


// =====================================================
// DETALLE DEL ACTA PARA EL MENSAJERO
// =====================================================

export interface MensajeroActaDetail
  extends MensajeroActa {

  equipo: MensajeroEquipo;

  usuarioId: number;

  destinatarioUsuarioId:
    number | null;

  mensajeroUsuarioId:
    number | null;

  deviceId: string | null;

  formDataJson: string;

  rutaDocumento: string | null;

  hashSha256: string | null;

  motivoAnulacion: string | null;

  fechaFinalizacion:
    string | null;

  fechaAnulacion:
    string | null;
}