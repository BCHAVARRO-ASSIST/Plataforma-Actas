import {
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import {
  getAuthenticatedUser,
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
    } catch {
      setUser(null);
    } finally {
      setIsRestoringSession(false);
    }
  }

  useEffect(() => {
    // Interceptor reactivo ante 401: limpia la sesión en memoria
    setUnauthorizedHandler(() => {
      setUser(null);
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
    try {
      await logoutApi();
    } finally {
      setUser(null);
    }
  }

  const value = useMemo<AuthContextValue>(() => {
    return {
      token: null,
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
