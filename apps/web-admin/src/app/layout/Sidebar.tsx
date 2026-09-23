import type { View, NavigationItem } from "../navigation/navigationTypes";
import { CloseIcon } from "@/components/ui/Icons";

interface SidebarProps {
  view: View;
  navigationItems: NavigationItem[];
  isOpen?: boolean;
  isCollapsed?: boolean;
  onClose?: () => void;
  onViewChange: (view: View) => void;
}

export function Sidebar({
  view,
  navigationItems,
  isOpen = false,
  isCollapsed = false,
  onClose,
  onViewChange,
}: SidebarProps) {
  function handleSelectView(selectedView: View) {
    onViewChange(selectedView);
    if (onClose) {
      onClose();
    }
  }

  return (
    <>
      {/* Overlay para móvil */}
      {isOpen && (
        <div
          className="sidebar-overlay active"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      <aside className={`app-sidebar ${isOpen ? "open" : ""} ${isCollapsed ? "collapsed" : ""}`}>
        {/* Brand */}
        <div className="app-brand">
          <div className="app-brand-mark">PA</div>

          <div className="app-brand-text">
            <span className="brand-title">Plataforma de Actas</span>
            <span className="brand-subtitle">Assist Consultores</span>
          </div>

          {onClose && (
            <button
              type="button"
              className="sidebar-close-button"
              onClick={onClose}
              aria-label="Cerrar menú"
            >
              <CloseIcon size={16} />
            </button>
          )}
        </div>

        {/* Navegación unificada */}
        <nav className="app-nav">
          <span className="app-nav-section">Menú Principal</span>

          {navigationItems.map((item) => (
            <button
              key={item.id}
              type="button"
              disabled={item.disabled}
              className={view === item.view ? "active" : ""}
              onClick={() => {
                if (item.disabled) return;
                handleSelectView(item.view);
              }}
            >
              {item.icon && <span className="app-nav-icon">{item.icon}</span>}
              <span className="app-nav-label">{item.label}</span>
            </button>
          ))}
        </nav>

        {/* Footer Institucional */}
        <div className="sidebar-version">
          <span>ASSIST CONSULTORES DE SISTEMAS S.A.</span>
          <strong>v2.0 · Enterprise</strong>
        </div>
      </aside>
    </>
  );
}