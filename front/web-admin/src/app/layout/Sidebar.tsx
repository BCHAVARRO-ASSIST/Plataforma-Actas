import type {
  View,
  NavigationItem,
} from "../navigation/navigationTypes";

interface SidebarProps {
  view: View;
  navigationItems: NavigationItem[];
  isOpen?: boolean;
  onClose?: () => void;
  onViewChange: (view: View) => void;
}

export function Sidebar({
  view,
  navigationItems,
  isOpen = false,
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
      {/* OVERLAY PARA MÓVIL */}
      {isOpen && (
        <div
          className="sidebar-overlay active"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      <aside className={`app-sidebar ${isOpen ? "open" : ""}`}>
        {/* BRAND */}
        <div className="app-brand">
          <div className="app-brand-mark">
            PA
          </div>

          <div className="app-brand-text">
            <strong>
              Plataforma
            </strong>
            <span>
              de Actas
            </span>
          </div>

          {onClose && (
            <button
              type="button"
              className="sidebar-close-button"
              onClick={onClose}
              aria-label="Cerrar menú"
            >
              ✕
            </button>
          )}
        </div>

        {/* NAVEGACIÓN UNIFICADA EN UNA SOLA VISTA */}
        <nav className="app-nav">
          <span className="app-nav-section">
            Menú Principal
          </span>

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
              <span className="app-nav-icon">
                {item.icon}
              </span>
              {item.label}
            </button>
          ))}
        </nav>

        {/* FOOTER */}
        <div className="sidebar-version">
          <span>PLATAFORMA DE ACTAS</span>
          <strong>v0.1 · Responsive</strong>
        </div>
      </aside>
    </>
  );
}