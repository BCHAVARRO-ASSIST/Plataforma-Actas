export interface Permiso {
  id: number;
  nombre: string;
  modulo: string;
  descripcion: string | null;
  activo: boolean;
}

export interface Rol {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
  fechaCreacion: string;
}

export interface RolDetalle extends Rol {
  permisos: Permiso[];
}

export interface RolCreateRequest {
  nombre: string;
  descripcion: string;
}

export interface RolUpdateRequest {
  nombre: string;
  descripcion: string;
}

export interface RolPermisosRequest {
  permisoIds: number[];
}