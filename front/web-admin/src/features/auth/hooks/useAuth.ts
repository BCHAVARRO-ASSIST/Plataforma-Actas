import { useContext } from "react";
import { AuthContext } from "../context/authContextInstance";
import type { AuthContextValue } from "../types/auth";


export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth debe utilizarse dentro de AuthProvider."
    );
  }

  return context;
}
