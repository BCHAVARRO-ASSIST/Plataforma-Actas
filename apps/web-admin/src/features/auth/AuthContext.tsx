import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

import {
  AUTH_PERMISSIONS_KEY,
  AUTH_TOKEN_KEY,
  getAuthenticatedUser,
  login as loginApi,
  type AuthenticatedUser,
  type AuthContextValue,
  type LoginCredentials,
} from "./authService";

const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth debe utilizarse dentro de un AuthProvider");
  }
  return context;
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem(AUTH_TOKEN_KEY);
  });

  const [user, setUser] = useState<AuthenticatedUser | null>(null);
  const [isRestoringSession, setIsRestoringSession] = useState(true);

  async function restoreSession(): Promise<void> {
    const storedToken = localStorage.getItem(AUTH_TOKEN_KEY);

    if (!storedToken) {
      setUser(null);
      setIsRestoringSession(false);
      return;
    }

    try {
      const authenticatedUser = await getAuthenticatedUser(storedToken);
      setToken(storedToken);
      setUser(authenticatedUser);
    } catch {
      localStorage.removeItem(AUTH_TOKEN_KEY);
      localStorage.removeItem(AUTH_PERMISSIONS_KEY);
      setToken(null);
      setUser(null);
    } finally {
      setIsRestoringSession(false);
    }
  }

  useEffect(() => {
    void Promise.resolve().then(restoreSession);
  }, []);

  async function login(credentials: LoginCredentials): Promise<void> {
    const data = await loginApi(credentials);

    localStorage.setItem(AUTH_TOKEN_KEY, data.token);
    setToken(data.token);

    if (data.permisos && Array.isArray(data.permisos)) {
      localStorage.setItem(AUTH_PERMISSIONS_KEY, JSON.stringify(data.permisos));
    }

    const authenticatedUser = await getAuthenticatedUser(data.token);
    setUser(authenticatedUser);
  }

  function logout(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_PERMISSIONS_KEY);
    setToken(null);
    setUser(null);
  }

  const value = useMemo<AuthContextValue>(() => {
    return {
      token,
      user,
      isAuthenticated: Boolean(token && user),
      isRestoringSession,
      login,
      logout,
      restoreSession,
    };
  }, [token, user, isRestoringSession]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
