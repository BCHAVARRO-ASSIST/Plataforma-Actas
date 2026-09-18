import { apiRequest } from "../../shared/api/httpClient";

export interface AuthenticatedUser {
  usuarioId?: string;
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso?: string;
  rol: string;
  permisos: string[];
}

export interface LoginCredentials {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  token?: string;
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
  token?: string | null;
  user: AuthenticatedUser | null;
  isAuthenticated: boolean;
  isRestoringSession: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  loginGoogle: (idToken: string) => Promise<void>;
  logout: () => Promise<void>;
  restoreSession: () => Promise<void>;
}

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
  const data = await apiRequest<LoginResponse>("/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      usuario: credentials.usuario.trim(),
      password: credentials.password,
    }),
    errorMessage: "Usuario o contraseña incorrectos.",
  });

  return data;
}

export async function loginWithGoogle(idToken: string): Promise<LoginResponse> {
  const data = await apiRequest<LoginResponse>("/auth/google", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      IdToken: idToken.trim(),
    }),
    errorMessage: "No fue posible validar la cuenta corporativa con Google.",
  });

  return data;
}

export async function getAuthenticatedUser(): Promise<AuthenticatedUser> {
  const data = await apiRequest<AuthenticatedUserResponse>("/auth/me", {
    method: "GET",
    errorMessage: "Sesión inválida o expirada.",
  });

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

export async function logoutApi(): Promise<void> {
  try {
    await apiRequest("/auth/logout", {
      method: "POST",
    });
  } catch {
    // Si la sesión ya expiró en el servidor, ignorar el error
  }
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
  return apiRequest<ChangePasswordResponse>("/auth/change-password", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      CurrentPassword: request.currentPassword,
      NewPassword: request.newPassword,
      ConfirmPassword: request.confirmPassword,
    }),
    errorMessage: "No fue posible actualizar la contraseña.",
  });
}
