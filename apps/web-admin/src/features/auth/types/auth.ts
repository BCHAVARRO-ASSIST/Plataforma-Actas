export interface AuthenticatedUser {
  usuarioId?: string;
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso?: string;
  rol: string;
  permisos?: string[];
}

export interface LoginCredentials {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  usuario?: {
    id: number;
    nombre: string;
    apellido: string;
    usuarioRed: string;
    correo: string;
    proceso: string;
    activo: boolean;
  };
  rol?: {
    id: number;
    nombre: string;
    descripcion: string;
  };
  permisos?: string[];
}

export interface AuthContextValue {
  token: string | null;
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  isRestoringSession: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  restoreSession: () => Promise<void>;
}
