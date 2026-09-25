import {
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import {
  getAuthenticatedUser,
  getStoredToken,
  clearStoredToken,
  AUTH_TOKEN_KEY,
  login as loginApi,
  loginWithGoogle,
  logoutApi,
  type AuthenticatedUser,
  type AuthContextValue,
  type LoginCredentials,
} from "./authService";
import { AuthContext } from "./AuthContextInstance";
import { setUnauthorizedHandler } from "../../shared/api/httpClient";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [isRestoringSession, setIsRestoringSession] = useState(true);

  async function restoreSession(): Promise<void> {
    try {
      const authenticatedUser = await getAuthenticatedUser();
      setUser(authenticatedUser);
      if (!authenticatedUser) {
        clearStoredToken();
      }
    } catch {
      clearStoredToken();
      setUser(null);
    } finally {
      setIsRestoringSession(false);
    }
  }

  useEffect(() => {
    // Interceptor reactivo ante 401: limpia la sesión en memoria y storage
    setUnauthorizedHandler(() => {
      clearStoredToken();
      if (typeof window !== "undefined" && window.localStorage) {
        localStorage.removeItem(AUTH_TOKEN_KEY);
        localStorage.removeItem("token");
        localStorage.removeItem("plataforma_actas_token");
        localStorage.removeItem("plataforma_actas_permissions");
        localStorage.removeItem("actas_session_token");
      }
      setUser(null);
      if (typeof window !== "undefined") {
        window.location.replace("/login");
      }
    });

    void restoreSession();

    return () => {
      setUnauthorizedHandler(null);
    };
  }, []);

  async function login(credentials: LoginCredentials): Promise<void> {
    await loginApi(credentials);
    const authenticatedUser = await getAuthenticatedUser();
    setUser(authenticatedUser);
  }

  async function loginGoogle(idToken: string): Promise<void> {
    await loginWithGoogle(idToken);
    const authenticatedUser = await getAuthenticatedUser();
    setUser(authenticatedUser);
  }

  async function logout(): Promise<void> {
    // 1. Limpieza inmediata y atómica de todas las claves en localStorage
    clearStoredToken();
    if (typeof window !== "undefined" && window.localStorage) {
      localStorage.removeItem(AUTH_TOKEN_KEY);
      localStorage.removeItem("token");
      localStorage.removeItem("plataforma_actas_token");
      localStorage.removeItem("plataforma_actas_permissions");
      localStorage.removeItem("actas_session_token");
    }

    // 2. Limpiar estados en memoria
    setUser(null);

    // 3. Notificar al backend de forma no bloqueante
    try {
      await logoutApi();
    } catch {
      // Ignorar errores de red en logout
    } finally {
      // Asegurar limpieza final
      clearStoredToken();
      if (typeof window !== "undefined" && window.localStorage) {
        localStorage.removeItem(AUTH_TOKEN_KEY);
        localStorage.removeItem("token");
        localStorage.removeItem("plataforma_actas_token");
        localStorage.removeItem("plataforma_actas_permissions");
        localStorage.removeItem("actas_session_token");
      }
      // 4. Redirigir a login
      if (typeof window !== "undefined") {
        window.location.replace("/login");
      }
    }
  }

  const value = useMemo<AuthContextValue>(() => {
    return {
      token: getStoredToken(),
      user,
      isAuthenticated: Boolean(user),
      isRestoringSession,
      login,
      loginGoogle,
      logout,
      restoreSession,
    };
  }, [user, isRestoringSession]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
