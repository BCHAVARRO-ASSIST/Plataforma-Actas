export interface DashboardKpis {
  totalEquipos: number;
  equiposActivos: number;
  equiposInactivos: number;
  equiposAsignados: number;
  equiposMantenimiento: number;
  totalActas: number;
  actasBorrador: number;
  actasFirmadas: number;
  actasFinalizadas: number;
  actasAnuladas: number;
  entregasPendientes: number;
  devolucionesPendientes: number;
}

export interface StatusCountItem {
  estado: string;
  cantidad: number;
  porcentaje: number;
  color?: string | null;
}

export interface MessengerDeliveryItem {
  mensajeroId: number | null;
  mensajeroNombre: string;
  totalAsignadas: number;
  totalEntregadas: number;
  totalPendientes: number;
}

export interface TimelinePointItem {
  fecha: string;
  totalCreadas: number;
  totalFirmadas: number;
  totalEntregas: number;
  totalDevoluciones: number;
}

export interface MaintenanceStats {
  totalMantenimientos: number;
  preventivos: number;
  correctivos: number;
  concluidos: number;
  pendientes: number;
  mantenimientosPorTipo: StatusCountItem[];
}

export interface RecentActivityItem {
  id: string;
  tipoEvento: string;
  titulo: string;
  descripcion: string;
  responsable: string | null;
  fecha: string;
  estado: string;
  badgeTipo: string | null;
}
