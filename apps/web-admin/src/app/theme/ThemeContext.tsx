import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

export type Theme = "light" | "dark";

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const THEME_STORAGE_KEY = "plataforma-theme";

function cleanupResidualStorage(): void {
  try {
    if (typeof window !== "undefined" && window.localStorage) {
      // Limpieza preventiva de clave residual vacía '' en localStorage si existiera
      if (localStorage.getItem("") !== null) {
        localStorage.removeItem("");
      }
    }
  } catch {
    // Manejo seguro en caso de restricciones de acceso al storage
  }
}

function getInitialTheme(): Theme {
  cleanupResidualStorage();

  // 1. Verificar preferencia guardada en localStorage
  try {
    if (typeof window !== "undefined" && window.localStorage) {
      const savedTheme = localStorage.getItem(THEME_STORAGE_KEY) as Theme | null;
      if (savedTheme === "light" || savedTheme === "dark") {
        return savedTheme;
      }
    }
  } catch {
    // Fallback a preferencia del sistema
  }

  // 2. Verificar preferencia del sistema operativo
  if (typeof window !== "undefined" && window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
    return "dark";
  }

  return "light";
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(getInitialTheme);

  useEffect(() => {
    cleanupResidualStorage();
    document.documentElement.setAttribute("data-theme", theme);
    document.documentElement.classList.toggle("dark", theme === "dark");
    if (typeof document !== "undefined" && document.body) {
      document.body.setAttribute("data-theme", theme);
      document.body.classList.toggle("dark", theme === "dark");
    }
    try {
      if (typeof window !== "undefined" && window.localStorage) {
        localStorage.setItem(THEME_STORAGE_KEY, theme);
      }
    } catch {
      // Storage no disponible
    }
  }, [theme]);

  // Escuchar cambios en la preferencia del sistema si el usuario no guardó preferencia manual
  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = (e: MediaQueryListEvent) => {
      try {
        const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);
        if (!savedTheme) {
          setThemeState(e.matches ? "dark" : "light");
        }
      } catch {
        setThemeState(e.matches ? "dark" : "light");
      }
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, []);

  const toggleTheme = () => {
    setThemeState((prev) => (prev === "dark" ? "light" : "dark"));
  };

  const setTheme = (newTheme: Theme) => {
    setThemeState(newTheme);
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme(): ThemeContextType {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useTheme debe ser utilizado dentro de un ThemeProvider");
  }
  return context;
}
