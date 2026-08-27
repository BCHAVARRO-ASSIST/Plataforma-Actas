export type ReportTab = "equipos" | "actas" | "entregas" | "devoluciones";

export interface ReportFilter {
  fechaInicio?: string | null;
  fechaFin?: string | null;
  estado?: string | null;
  tipoActa?: string | null;
  equipmentId?: string | null;
  mensajeroId?: number | null;
  empresa?: string | null;
  marca?: string | null;
  search?: string | null;
}

export interface EquiposReportItem {
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
  actaEntregada: boolean;
  tipoMantenimiento: string | null;
  fechaMantenimiento: string | null;
}

export interface ActasReportItem {
  id: string;
  numeroActa: string;
  tipoActa: string;
  estado: string;
  equipmentId: string;
  equipmentInventoryKey: string | null;
  equipmentNombre: string | null;
  usuarioId: number;
  creadorNombre: string | null;
  destinatarioUsuarioId: number | null;
  destinatarioNombre: string | null;
  mensajeroUsuarioId: number | null;
  mensajeroNombre: string | null;
  actaEntregada: boolean;
  fechaCreacion: string;
  fechaFirma: string | null;
  fechaFinalizacion: string | null;
  fechaAnulacion: string | null;
  actaOrigenId: string | null;
}

export interface EntregasReportItem {
  id: string;
  numeroActa: string;
  equipmentInventoryKey: string;
  equipmentNombre: string;
  mensajeroNombre: string | null;
  destinatarioNombre: string | null;
  estado: string;
  firmado: boolean;
  fechaCreacion: string;
  fechaFirma: string | null;
}

export interface DevolucionesReportItem {
  id: string;
  numeroActa: string;
  actaOrigenId: string | null;
  actaOrigenNumero: string | null;
  equipmentInventoryKey: string;
  equipmentNombre: string;
  responsableEntrega: string | null;
  estado: string;
  fechaCreacion: string;
  fechaFirma: string | null;
}
