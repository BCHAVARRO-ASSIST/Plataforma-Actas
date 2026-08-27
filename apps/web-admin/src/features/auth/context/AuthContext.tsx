import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";

import type { ReactNode } from "react";

import { AuthContext } from "./authContextInstance";

import {
  AUTH_TOKEN_KEY,
  AUTH_PERMISSIONS_KEY,
  getAuthenticatedUser,
  login as requestLogin,
} from "../services/authService";

import type {
  AuthenticatedUser,
  AuthContextValue,
  LoginCredentials,
} from "../types/auth";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({
  children,
}: AuthProviderProps) {
  const [token, setToken] =
    useState<string | null>(() =>
      localStorage.getItem(AUTH_TOKEN_KEY)
    );

  const [user, setUser] =
    useState<AuthenticatedUser | null>(null);

  const [isRestoringSession, setIsRestoringSession] =
    useState<boolean>(() =>
      Boolean(localStorage.getItem(AUTH_TOKEN_KEY))
    );

  const logout =
    useCallback(() => {
      localStorage.removeItem(AUTH_TOKEN_KEY);
      localStorage.removeItem(AUTH_PERMISSIONS_KEY);
      setToken(null);
      setUser(null);
      setIsRestoringSession(false);
    }, []);

  const validateSession =
    useCallback(async (sessionToken: string) => {
      const authenticatedUser =
        await getAuthenticatedUser(sessionToken);

      setToken(sessionToken);
      setUser(authenticatedUser);

      return authenticatedUser;
    }, []);

  const restoreSession =
    useCallback(async () => {
      const storedToken =
        localStorage.getItem(AUTH_TOKEN_KEY);

      if (!storedToken) {
        logout();
        return;
      }

      setIsRestoringSession(true);

      try {
        await validateSession(storedToken);
      } catch {
        logout();
      } finally {
        setIsRestoringSession(false);
      }
    }, [logout, validateSession]);

  // Carga inicial de sesión
  useEffect(() => {
    let isMounted = true;
    const storedToken =
      localStorage.getItem(AUTH_TOKEN_KEY);

    if (storedToken) {
      getAuthenticatedUser(storedToken)
        .then((authenticatedUser) => {
          if (isMounted) {
            setToken(storedToken);
            setUser(authenticatedUser);
          }
        })
        .catch(() => {
          if (isMounted) {
            logout();
          }
        })
        .finally(() => {
          if (isMounted) {
            setIsRestoringSession(false);
          }
        });
    }

    return () => {
      isMounted = false;
    };
  }, [logout]);

  // =========================================================================
  // WATCHDOG DE SEGURIDAD EN TIEMPO REAL
  // Detecta inmediatamente si el token fue eliminado en Chrome DevTools / Storage
  // =========================================================================
  useEffect(() => {
    const checkTokenIntegrity = () => {
      const currentStoredToken = localStorage.getItem(AUTH_TOKEN_KEY);

      // Si el estado en memoria tiene token pero en LocalStorage fue eliminado:
      if (!currentStoredToken && token !== null) {
        logout();
        return;
      }

      // Si el token cambió en otra pestaña
      if (currentStoredToken && token && currentStoredToken !== token) {
        setToken(currentStoredToken);
        validateSession(currentStoredToken).catch(() => logout());
      }
    };

    // 1. Escuchar evento nativo de storage (cambios entre pestañas)
    window.addEventListener("storage", checkTokenIntegrity);

    // 2. Escuchar cuando la ventana/pestaña vuelve a estar activa
    window.addEventListener("focus", checkTokenIntegrity);
    document.addEventListener("visibilitychange", checkTokenIntegrity);

    // 3. Intervalo activo rápido (cada 400ms) para detectar eliminación directa en DevTools
    const intervalId = setInterval(checkTokenIntegrity, 400);

    return () => {
      window.removeEventListener("storage", checkTokenIntegrity);
      window.removeEventListener("focus", checkTokenIntegrity);
      document.removeEventListener("visibilitychange", checkTokenIntegrity);
      clearInterval(intervalId);
    };
  }, [token, logout, validateSession]);

  const login =
    useCallback(async (credentials: LoginCredentials) => {
      const response =
        await requestLogin(credentials);

      localStorage.setItem(
        AUTH_TOKEN_KEY,
        response.token
      );

      setIsRestoringSession(true);

      try {
        await validateSession(response.token);
      } catch (error) {
        logout();
        throw error;
      } finally {
        setIsRestoringSession(false);
      }
    }, [logout, validateSession]);

  const value =
    useMemo<AuthContextValue>(() => ({
      token,
      user,
      isAuthenticated: token !== null,
      isRestoringSession,
      login,
      logout,
      restoreSession,
    }), [
      token,
      user,
      isRestoringSession,
      login,
      logout,
      restoreSession,
    ]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}
