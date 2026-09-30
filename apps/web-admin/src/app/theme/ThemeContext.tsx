import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

export type Theme = "light" | "dark";

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const STORAGE_KEY = "plataforma-theme";

function getInitialTheme(): Theme {
  // 1. Verificar preferencia guardada en localStorage
  const savedTheme = localStorage.getItem(STORAGE_KEY) as Theme | null;
  if (savedTheme === "light" || savedTheme === "dark") {
    return savedTheme;
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
    document.documentElement.setAttribute("data-theme", theme);
    document.documentElement.classList.toggle("dark", theme === "dark");
    if (typeof document !== "undefined" && document.body) {
      document.body.setAttribute("data-theme", theme);
      document.body.classList.toggle("dark", theme === "dark");
    }
    localStorage.setItem(STORAGE_KEY, theme);
  }, [theme]);

  // Escuchar cambios en la preferencia del sistema si el usuario no guardó preferencia manual
  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = (e: MediaQueryListEvent) => {
      const savedTheme = localStorage.getItem(STORAGE_KEY);
      if (!savedTheme) {
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
