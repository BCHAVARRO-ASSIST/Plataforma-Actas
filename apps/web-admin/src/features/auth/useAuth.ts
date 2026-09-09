import { useContext } from "react";
import { AuthContext } from "./AuthContextInstance";
import type { AuthContextValue } from "./authService";

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth debe utilizarse dentro de un AuthProvider");
  }
  return context;
}
