import { useTheme } from "./ThemeContext";
import { SunIcon, MoonIcon } from "@/components/ui/Icons";

interface ThemeToggleProps {
  className?: string;
  showLabel?: boolean;
}

export function ThemeToggle({ className = "", showLabel = false }: ThemeToggleProps) {
  const { theme, toggleTheme } = useTheme();
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      className={`theme-toggle-button ${isDark ? "is-dark" : "is-light"} ${className}`}
      onClick={toggleTheme}
      aria-label={isDark ? "Cambiar a modo claro" : "Cambiar a modo oscuro"}
      title={isDark ? "Cambiar a modo claro" : "Cambiar a modo oscuro"}
    >
      <span className="theme-toggle-icon">
        {isDark ? <SunIcon size={17} /> : <MoonIcon size={17} />}
      </span>
      {showLabel && (
        <span className="theme-toggle-label">
          {isDark ? "Modo Claro" : "Modo Oscuro"}
        </span>
      )}
    </button>
  );
}
