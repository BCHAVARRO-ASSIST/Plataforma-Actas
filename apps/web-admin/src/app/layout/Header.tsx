import type { AuthenticatedUser } from "../../features/auth/authService";

interface HeaderProps {
  user: AuthenticatedUser | null;
  onLogout: () => void;
  onToggleMenu?: () => void;
}

export function Header({
  user,
  onLogout,
  onToggleMenu,
}: HeaderProps) {
  return (
    <header className="app-topbar">
      <div className="topbar-left">
        {onToggleMenu && (
          <button
            type="button"
            className="mobile-menu-toggle"
            onClick={onToggleMenu}
            aria-label="Abrir menú de navegación"
            title="Abrir menú"
          >
            ☰
          </button>
        )}

        <div className="topbar-context">
          <span>
            SISTEMA ADMINISTRATIVO
          </span>

          <strong>
            Plataforma de Actas
          </strong>
        </div>
      </div>

      <div className="app-user">
        <div className="app-user-avatar">
          {user
            ? `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`
            : "PA"}
        </div>

        <div className="app-user-info">
          <strong>
            {user
              ? `${user.nombre} ${user.apellido}`
              : "Usuario"}
          </strong>

          <span>
            {user?.rol ?? "Usuario"}
          </span>
        </div>

        <button
          type="button"
          onClick={onLogout}
          className="auth-logout-button"
        >
          Salir
        </button>
      </div>
    </header>
  );
}
