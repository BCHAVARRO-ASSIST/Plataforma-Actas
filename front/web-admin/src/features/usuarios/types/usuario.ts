export interface Usuario {
  id: number;

  nombre: string;

  apellido: string;

  usuarioRed: string;

  correo: string;

  proceso: string;

  activo: boolean;

  rolId: number | null;

  rolNombre: string | null;
}