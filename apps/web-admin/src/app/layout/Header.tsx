import { useState, useRef, useEffect } from "react";
import type { AuthenticatedUser } from "@/features/auth/authService";
import { LogoutIcon, MenuIcon, ChevronDownIcon } from "@/components/ui/Icons";

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
  const [menuAbierto, setMenuAbierto] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const initials = user
    ? `${user.nombre.charAt(0)}${user.apellido.charAt(0)}`.toUpperCase()
    : "PA";

  // Cerrar menú al hacer clic fuera
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setMenuAbierto(false);
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setMenuAbierto(false);
      }
    }

    if (menuAbierto) {
      document.addEventListener("mousedown", handleClickOutside);
      document.addEventListener("keydown", handleKeyDown);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [menuAbierto]);

  function handleLogoutClick() {
    setMenuAbierto(false);
    onLogout();
  }

  return (
    <header className="app-header">
      <div className="header-left">
        {onToggleMenu && (
          <button
            type="button"
            className="header-menu-button"
            onClick={onToggleMenu}
            aria-label="Alternar menú lateral"
            title="Mostrar u ocultar barra lateral"
          >
            <MenuIcon size={18} />
          </button>
        )}

        <div className="header-title-block">
          <span className="header-subtitle">
            Plataforma Corporativa
          </span>
          <div className="header-title">Control de Actas e Inventario</div>
        </div>
      </div>

      <div className="header-right">
        {/* Menú Desplegable de Perfil de Usuario */}
        <div className="user-menu-wrapper" ref={menuRef}>
          <button
            type="button"
            className={`user-badge-button ${menuAbierto ? "active" : ""}`}
            onClick={() => setMenuAbierto((prev) => !prev)}
            aria-haspopup="true"
            aria-expanded={menuAbierto}
            title="Opciones de cuenta y usuario"
          >
            <div className="user-avatar">{initials}</div>
            <div className="user-info">
              <span className="user-name">{user ? `${user.nombre} ${user.apellido}` : "Usuario"}</span>
              <span className="user-role">{user?.rol ?? "Colaborador"}</span>
            </div>
            <div className="user-badge-chevron">
              <ChevronDownIcon size={14} />
            </div>
          </button>

          {menuAbierto && (
            <div className="user-menu-dropdown">
              {/* Cabecera del Menú con Detalle del Usuario */}
              <div className="user-menu-header">
                <span className="user-menu-header-name">
                  {user ? `${user.nombre} ${user.apellido}` : "Usuario"}
                </span>
                <span className="user-menu-header-email">
                  {user?.correo || user?.usuarioRed || "usuario@empresa.com"}
                </span>
                <span className="user-menu-header-role">
                  {user?.rol ?? "Colaborador"}
                </span>
              </div>

              {/* Lista de Acciones */}
              <div className="user-menu-items">
                <button
                  type="button"
                  className="user-menu-item danger"
                  onClick={handleLogoutClick}
                >
                  <span className="user-menu-item-icon">
                    <LogoutIcon size={15} />
                  </span>
                  <span>Cerrar sesión</span>
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
