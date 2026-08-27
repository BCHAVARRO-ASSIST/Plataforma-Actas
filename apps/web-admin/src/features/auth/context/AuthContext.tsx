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
            localStorage.removeItem(AUTH_TOKEN_KEY);
            setToken(null);
            setUser(null);
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
  }, []);

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
