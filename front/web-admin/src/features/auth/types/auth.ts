export interface AuthenticatedUser {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  rol: string;
}

export interface LoginCredentials {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  token: string;
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

