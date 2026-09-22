import { API_URL } from "../config/api";

export class ApiError extends Error {
  status: number;
  response?: Response;

  constructor(status: number, message: string, response?: Response) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.response = response;
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}

const TOKEN_KEYS = ["auth_token", "token", "actas_session_token"];

export function getStoredToken(): string | null {
  if (typeof window === "undefined" || !window.localStorage) return null;
  for (const key of TOKEN_KEYS) {
    const val = localStorage.getItem(key);
    if (val && val.trim()) return val.trim();
  }
  return null;
}

export function setStoredToken(token: string): void {
  if (typeof window === "undefined" || !window.localStorage) return;
  localStorage.setItem("auth_token", token);
  localStorage.setItem("token", token);
}

export function clearStoredToken(): void {
  if (typeof window === "undefined" || !window.localStorage) return;
  for (const key of TOKEN_KEYS) {
    localStorage.removeItem(key);
  }
}

type UnauthorizedHandler = () => void;
type ForbiddenHandler = (message: string) => void;

let onUnauthorizedHandler: UnauthorizedHandler | null = null;
let onForbiddenHandler: ForbiddenHandler | null = null;

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  onUnauthorizedHandler = handler;
}

export function setForbiddenHandler(handler: ForbiddenHandler | null) {
  onForbiddenHandler = handler;
}

export function getAuthHeaders(includeJson: boolean = false, customHeaders?: HeadersInit): HeadersInit {
  const headers: Record<string, string> = {
    "X-Requested-With": "XMLHttpRequest", // Mitigación CSRF en llamadas cross-origin / browser
  };

  const token = getStoredToken();
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  if (includeJson) {
    headers["Content-Type"] = "application/json";
  }

  if (customHeaders) {
    return { ...headers, ...customHeaders };
  }

  return headers;
}

export async function getApiErrorMessage(
  response: Response,
  defaultMessage: string = "Ha ocurrido un error en la solicitud."
): Promise<string> {
  if (response.status === 401) {
    return "La sesión no es válida o ha expirado.";
  }
  if (response.status === 403) {
    return "No tienes permisos suficientes para realizar esta operación.";
  }
  if (response.status === 404) {
    return "El recurso solicitado no existe.";
  }

  try {
    const text = await response.text();
    if (text && text.trim()) {
      try {
        const data = JSON.parse(text);
        if (typeof data === "string") return data;
        if (data.message) return data.message;
        if (data.title) return data.title;
        if (data.error) return data.error;
      } catch {
        return text;
      }
    }
  } catch {
    // Fallback al mensaje por defecto
  }

  return defaultMessage;
}

function resolveUrl(endpoint: string): string {
  if (endpoint.startsWith("http://") || endpoint.startsWith("https://")) {
    return endpoint;
  }
  const cleanEndpoint = endpoint.startsWith("/") ? endpoint : `/${endpoint}`;
  return `${API_URL}${cleanEndpoint}`;
}

export interface RequestOptions extends RequestInit {
  errorMessage?: string;
}

export async function apiRequest<T = unknown>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { errorMessage, headers, ...restOptions } = options;
  const url = resolveUrl(endpoint);

  const hasBody = restOptions.body !== undefined && restOptions.body !== null;
  const isFormData = typeof FormData !== "undefined" && restOptions.body instanceof FormData;

  const authHeaders = getAuthHeaders(hasBody && !isFormData, headers);

  const response = await fetch(url, {
    credentials: "include", // Envío automático de cookies HttpOnly seguras
    ...restOptions,
    headers: authHeaders,
  });

  if (!response.ok) {
    const message = await getApiErrorMessage(response, errorMessage);

    if (response.status === 401) {
      clearStoredToken();
      // Notificar reactivamente a AuthContext para limpiar la sesión si expiró
      // Se excluye /auth/me para permitir chequeo inicial de sesión defensivo y silencioso
      if (!endpoint.includes("/auth/login") && !endpoint.includes("/auth/google") && !endpoint.includes("/auth/me")) {
        onUnauthorizedHandler?.();
      }
    } else if (response.status === 403) {
      onForbiddenHandler?.(message);
    }

    throw new ApiError(response.status, message, response);
  }

  if (response.status === 204) {
    return undefined as unknown as T;
  }

  const contentType = response.headers.get("content-type");
  if (contentType && contentType.includes("application/json")) {
    return response.json() as Promise<T>;
  }

  return response.text() as unknown as Promise<T>;
}

export async function apiGet<T>(endpoint: string, options?: RequestOptions): Promise<T> {
  return apiRequest<T>(endpoint, { ...options, method: "GET" });
}

export async function apiPost<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
  return apiRequest<T>(endpoint, {
    ...options,
    method: "POST",
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
}

export async function apiPut<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
  return apiRequest<T>(endpoint, {
    ...options,
    method: "PUT",
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
}

export async function apiPatch<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
  return apiRequest<T>(endpoint, {
    ...options,
    method: "PATCH",
    body: body !== undefined ? (typeof body === "string" ? body : JSON.stringify(body)) : undefined,
  });
}

export async function apiDelete<T = void>(endpoint: string, options?: RequestOptions): Promise<T> {
  return apiRequest<T>(endpoint, { ...options, method: "DELETE" });
}

export async function apiGetBlob(endpoint: string, options?: RequestOptions): Promise<Blob> {
  const { errorMessage, headers, ...restOptions } = options || {};
  const url = resolveUrl(endpoint);
  const authHeaders = getAuthHeaders(false, headers);

  const response = await fetch(url, {
    credentials: "include",
    ...restOptions,
    method: "GET",
    headers: authHeaders,
  });

  if (!response.ok) {
    const message = await getApiErrorMessage(response, errorMessage || "No fue posible descargar el archivo.");
    if (response.status === 401) {
      onUnauthorizedHandler?.();
    } else if (response.status === 403) {
      onForbiddenHandler?.(message);
    }
    throw new ApiError(response.status, message, response);
  }

  return response.blob();
}
