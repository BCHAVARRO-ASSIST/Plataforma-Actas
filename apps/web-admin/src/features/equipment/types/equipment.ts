export interface Equipment {
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

export interface CreateEquipmentRequest {
  inventoryKey: string;
  nombreEquipo: string;
  empresa?: string | null;
  asignadoA?: string | null;
  proveedor?: string | null;
  marca?: string | null;
  procesador?: string | null;
  discoDuro?: string | null;
  memoriaRam?: string | null;
  departamento?: string | null;
  estatusEquipo?: string | null;
}

export interface UpdateEquipmentRequest {
  inventoryKey: string;
  nombreEquipo: string;
  empresa?: string | null;
  asignadoA?: string | null;
  proveedor?: string | null;
  marca?: string | null;
  procesador?: string | null;
  discoDuro?: string | null;
  memoriaRam?: string | null;
  departamento?: string | null;
  estatusEquipo?: string | null;
}
