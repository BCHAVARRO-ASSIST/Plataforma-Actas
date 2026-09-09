import { API_URL } from "../../shared/config/api";

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

export const AUTH_TOKEN_KEY = "plataforma_actas_token";
export const AUTH_PERMISSIONS_KEY = "plataforma_actas_permissions";

interface AuthenticatedUserResponse {
  usuarioId?: string;
  nombre?: string;
  apellido?: string;
  usuarioRed?: string;
  correo?: string;
  proceso?: string;
  rol?: string;
  permisos?: string[];
}

export async function login(credentials: LoginCredentials): Promise<LoginResponse> {
  const response = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Usuario: credentials.usuario.trim(),
      Password: credentials.password,
    }),
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("Usuario o contraseña incorrectos.");
    }

    const error = await response.json().catch(() => null);
    throw new Error(error?.message ?? "No fue posible iniciar sesión.");
  }

  const data = (await response.json()) as LoginResponse;

  if (!data.token) {
    throw new Error("El servidor no devolvió un token de autenticación.");
  }

  if (data.permisos && Array.isArray(data.permisos)) {
    localStorage.setItem(AUTH_PERMISSIONS_KEY, JSON.stringify(data.permisos));
  }

  return data;
}

export async function getAuthenticatedUser(token: string): Promise<AuthenticatedUser> {
  const response = await fetch(`${API_URL}/auth/me`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Sesión inválida.");
  }

  const data = (await response.json()) as AuthenticatedUserResponse;

  if (data.permisos && Array.isArray(data.permisos)) {
    localStorage.setItem(AUTH_PERMISSIONS_KEY, JSON.stringify(data.permisos));
  }

  return {
    usuarioId: data.usuarioId ?? "",
    nombre: data.nombre ?? "",
    apellido: data.apellido ?? "",
    usuarioRed: data.usuarioRed ?? "",
    correo: data.correo ?? "",
    proceso: data.proceso ?? "",
    rol: data.rol ?? "",
    permisos: data.permisos ?? [],
  };
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordResponse {
  message: string;
}

export async function changePassword(request: ChangePasswordRequest): Promise<ChangePasswordResponse> {
  const token = localStorage.getItem(AUTH_TOKEN_KEY);
  const response = await fetch(`${API_URL}/auth/change-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({
      CurrentPassword: request.currentPassword,
      NewPassword: request.newPassword,
      ConfirmPassword: request.confirmPassword,
    }),
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.message ?? "No fue posible actualizar la contraseña.");
  }

  return data;
}
