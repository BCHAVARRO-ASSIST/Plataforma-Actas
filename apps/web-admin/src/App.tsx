import { useState } from "react";
import type { FormEvent, ChangeEvent } from "react";

import { EquipmentPage } from "./features/equipment/EquipmentPage";
import { ActaPage } from "./features/actas/ActaPage";
import { DashboardPage } from "./features/dashboard/DashboardPage";
import { ReportesPage } from "./features/reportes/ReportesPage";
import { MantenimientosPage } from "./features/mantenimientos/MantenimientosPage";
import { UsuariosPage } from "./features/usuarios/UsuariosPage";
import { RolesPage } from "./features/roles/RolesPage";
import { MensajeroPage } from "./features/mensajero/MensajeroPage";
import { MensajeroActaDetail } from "./components/MensajeroActaDetail";

import LoginPage from "./features/auth/LoginPage";

import { hasPermission, PERMISSIONS } from "./features/auth/permissions";
import { AuthProvider } from "./features/auth/AuthContext";
import { useAuth } from "./features/auth/useAuth";

import { AppShell } from "./app/layout/AppShell";
import { Sidebar } from "./app/layout/Sidebar";
import { Header } from "./app/layout/Header";
import { NAVIGATION_ITEMS } from "./app/navigation/navigation";
import type { View } from "./app/navigation/navigationTypes";

function AppContent() {
  const { user, token, login, logout, isAuthenticated } = useAuth();

  const [view, setView] = useState<View>("dashboard");
  const [selectedMensajeroActaId, setSelectedMensajeroActaId] = useState<string | null>(null);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [desktopSidebarCollapsed, setDesktopSidebarCollapsed] = useState(false);

  function handleToggleSidebar() {
    if (typeof window !== "undefined" && window.innerWidth <= 900) {
      setMobileMenuOpen((prev) => !prev);
    } else {
      setDesktopSidebarCollapsed((prev) => !prev);
    }
  }

  // Login Form States
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loginLoading, setLoginLoading] = useState(false);
  const [loginError, setLoginError] = useState<string | null>(null);

  // =======================================================
  // FILTRAR ÍTEMS DE NAVEGACIÓN POR PERMISOS
  // =======================================================

  const navigationItems = NAVIGATION_ITEMS.filter((item) => {
    if (!item.permission) return true;
    return hasPermission(token, item.permission);
  });

  // =======================================================
  // ADAPTAR VISTA EFECTIVA SEGÚN PERMISOS DISPONIBLES
  // =======================================================

  const isCurrentViewAllowed = navigationItems.some((item) => item.view === view);
  const effectiveView: View = isCurrentViewAllowed
    ? view
    : navigationItems.length > 0
    ? navigationItems[0].view
    : "dashboard";

  // =======================================================
  // MANEJADORES DE AUTENTICACIÓN
  // =======================================================

  async function handleLoginSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoginError(null);
    setLoginLoading(true);

    try {
      await login({ usuario: username, password });
    } catch (err) {
      setLoginError(
        err instanceof Error ? err.message : "Error al iniciar sesión."
      );
    } finally {
      setLoginLoading(false);
    }
  }

  function handleLogout() {
    logout();
    setView("dashboard");
    setSelectedMensajeroActaId(null);
  }

  // =======================================================
  // RENDERIZADO DE LOGIN DIRECTO
  // =======================================================

  if (!isAuthenticated) {
    return (
      <LoginPage
        username={username}
        password={password}
        loading={loginLoading}
        error={loginError || ""}
        onUsernameChange={(e: ChangeEvent<HTMLInputElement>) =>
          setUsername(e.target.value)
        }
        onPasswordChange={(e: ChangeEvent<HTMLInputElement>) =>
          setPassword(e.target.value)
        }
        onSubmit={handleLoginSubmit}
      />
    );
  }

  // =======================================================
  // RENDERIZADO DE VISTA ACTIVA
  // =======================================================

  function renderCurrentView() {
    if (effectiveView === "equipment" && hasPermission(token, PERMISSIONS.EQUIPOS_CONSULTAR)) {
      return <EquipmentPage />;
    }

    if (effectiveView === "actas" && hasPermission(token, PERMISSIONS.ACTAS_CONSULTAR)) {
      return <ActaPage />;
    }

    if (effectiveView === "mantenimientos" && hasPermission(token, PERMISSIONS.EQUIPOS_CONSULTAR)) {
      return <MantenimientosPage />;
    }

    if (effectiveView === "reportes" && hasPermission(token, PERMISSIONS.REPORTES_CONSULTAR)) {
      return <ReportesPage />;
    }

    if (effectiveView === "mensajero" && hasPermission(token, PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR)) {
      if (selectedMensajeroActaId !== null) {
        return (
          <MensajeroActaDetail
            actaId={selectedMensajeroActaId}
            onBack={() => setSelectedMensajeroActaId(null)}
          />
        );
      }

      return (
        <MensajeroPage
          onSelectActa={(id) => setSelectedMensajeroActaId(id)}
        />
      );
    }

    if (effectiveView === "usuarios" && hasPermission(token, PERMISSIONS.USUARIOS_CONSULTAR)) {
      return <UsuariosPage />;
    }

    if (effectiveView === "roles" && hasPermission(token, PERMISSIONS.ROLES_CONSULTAR)) {
      return <RolesPage />;
    }

    // Default: Dashboard si tiene permiso, sino Mensajero o primera permitida
    if (hasPermission(token, PERMISSIONS.DASHBOARD_CONSULTAR)) {
      return <DashboardPage onNavigate={(nextView) => setView(nextView)} />;
    }

    if (hasPermission(token, PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR)) {
      return <MensajeroPage onSelectActa={(id) => setSelectedMensajeroActaId(id)} />;
    }

    return <DashboardPage onNavigate={(nextView) => setView(nextView)} />;
  }

  return (
    <AppShell
      sidebar={
        <Sidebar
          view={effectiveView}
          navigationItems={navigationItems}
          isOpen={mobileMenuOpen}
          isCollapsed={desktopSidebarCollapsed}
          onClose={() => setMobileMenuOpen(false)}
          onViewChange={(nextView) => {
            setView(nextView);
            setSelectedMensajeroActaId(null);
            setMobileMenuOpen(false);
          }}
        />
      }
      header={
        <Header
          user={user}
          onLogout={handleLogout}
          onToggleMenu={handleToggleSidebar}
        />
      }
    >
      {renderCurrentView()}
    </AppShell>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}