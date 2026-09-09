import { createContext } from "react";
import type { AuthContextValue } from "./authService";

export const AuthContext = createContext<AuthContextValue | null>(null);
