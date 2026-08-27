import { API_URL } from "../../../shared/config/api";

import type {
  AuthenticatedUser,
  LoginCredentials,
  LoginResponse,
} from "../types/auth";

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

export interface ForgotPasswordRequest {
  identifier: string;
}

export interface ForgotPasswordResponse {
  message: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ResetPasswordResponse {
  message: string;
}

// =====================================================
// INICIAR SESIÓN
// =====================================================

export async function login(
  credentials: LoginCredentials
): Promise<LoginResponse> {
  const response = await fetch(
    `${API_URL}/auth/login`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        Usuario: credentials.usuario.trim(),
        Password: credentials.password,
      }),
    }
  );

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error(
        "Usuario o contraseña incorrectos."
      );
    }

    const error = await response.json()
      .catch(() => null);

    throw new Error(
      error?.message ??
        "No fue posible iniciar sesión."
    );
  }

  const data = await response.json() as LoginResponse;

  if (!data.token) {
    throw new Error(
      "El servidor no devolvió un token de autenticación."
    );
  }

  if (data.permisos && Array.isArray(data.permisos)) {
    localStorage.setItem(AUTH_PERMISSIONS_KEY, JSON.stringify(data.permisos));
  }

  return data;
}

// =====================================================
// OBTENER USUARIO AUTENTICADO (ME)
// =====================================================

export async function getAuthenticatedUser(
  token: string
): Promise<AuthenticatedUser> {
  const response = await fetch(
    `${API_URL}/auth/me`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    throw new Error("Sesión inválida.");
  }

  const data =
    await response.json() as AuthenticatedUserResponse;

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

// =====================================================
// SOLICITAR RECUPERACIÓN
// =====================================================

export async function forgotPassword(
  identifier: string
): Promise<ForgotPasswordResponse> {
  const response = await fetch(
    `${API_URL}/auth/forgot-password`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        identifier: identifier.trim(),
      }),
    }
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      data?.message ??
        "No fue posible solicitar la recuperación."
    );
  }

  return data;
}

// =====================================================
// RESTABLECER CONTRASEÑA
// =====================================================

export async function resetPassword(
  request: ResetPasswordRequest
): Promise<ResetPasswordResponse> {
  const response = await fetch(
    `${API_URL}/auth/reset-password`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    }
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      data?.message ??
        "No fue posible restablecer la contraseña."
    );
  }

  return data;
}
