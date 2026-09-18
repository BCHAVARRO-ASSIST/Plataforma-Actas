import { useState, useCallback } from "react";
import type { FormEvent, ChangeEvent } from "react";

import { EquiposPage } from "@/features/equipos/EquiposPage";
import { ActaPage } from "@/features/actas/ActaPage";
import { DashboardPage } from "@/features/dashboard/DashboardPage";
import { ReportesPage } from "@/features/reportes/ReportesPage";
import { MantenimientosPage } from "@/features/mantenimientos/MantenimientosPage";
import { UsuariosPage } from "@/features/usuarios/UsuariosPage";
import { RolesPage } from "@/features/roles/RolesPage";
import { MensajeroPage } from "@/features/mensajero/MensajeroPage";
import { MensajeroActaDetail } from "@/features/mensajero/components/MensajeroActaDetail";

import LoginPage from "@/features/auth/LoginPage";

import { hasPermission, PERMISSIONS } from "@/features/auth/permissions";
import { AuthProvider } from "@/features/auth/AuthContext";
import { useAuth } from "@/features/auth/useAuth";
import { ProtectedRoute } from "@/components/ProtectedRoute";

import { AppShell } from "@/app/layout/AppShell";
import { Sidebar } from "@/app/layout/Sidebar";
import { Header } from "@/app/layout/Header";
import { NAVIGATION_ITEMS } from "@/app/navigation/navigation";
import type { View } from "@/app/navigation/navigationTypes";

function AppContent() {
  const { user, login, loginGoogle, logout, isAuthenticated, isRestoringSession } = useAuth();

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
  // FILTRAR ÍTEMS DE NAVEGACIÓN POR PERMISOS (EN MEMORIA)
  // =======================================================

  const navigationItems = NAVIGATION_ITEMS.filter((item) => {
    if (!item.permission) return true;
    return hasPermission(user, item.permission);
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

  const handleGoogleLogin = useCallback(async (idToken: string) => {
    setLoginError(null);
    setLoginLoading(true);

    try {
      await loginGoogle(idToken);
    } catch (err) {
      setLoginError(
        err instanceof Error ? err.message : "Error al iniciar sesión con Google Workspace."
      );
    } finally {
      setLoginLoading(false);
    }
  }, [loginGoogle]);

  async function handleLogout() {
    await logout();
    setView("dashboard");
    setSelectedMensajeroActaId(null);
  }

  // =======================================================
  // ESTADO DE CARGA DE SESIÓN SEGURA
  // =======================================================

  if (isRestoringSession) {
    return <ProtectedRoute>{null}</ProtectedRoute>;
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
        onGoogleLogin={handleGoogleLogin}
      />
    );
  }

  // =======================================================
  // RENDERIZADO DE VISTA ACTIVA PROTEGIDA
  // =======================================================

  function renderCurrentView() {
    switch (effectiveView) {
      case "equipment":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.EQUIPOS_CONSULTAR}>
            <EquiposPage />
          </ProtectedRoute>
        );

      case "actas":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.ACTAS_CONSULTAR}>
            <ActaPage />
          </ProtectedRoute>
        );

      case "mantenimientos":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.EQUIPOS_CONSULTAR}>
            <MantenimientosPage />
          </ProtectedRoute>
        );

      case "reportes":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.REPORTES_CONSULTAR}>
            <ReportesPage />
          </ProtectedRoute>
        );

      case "mensajero":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR}>
            {selectedMensajeroActaId !== null ? (
              <MensajeroActaDetail
                actaId={selectedMensajeroActaId}
                onBack={() => setSelectedMensajeroActaId(null)}
              />
            ) : (
              <MensajeroPage
                onSelectActa={(id) => setSelectedMensajeroActaId(id)}
              />
            )}
          </ProtectedRoute>
        );

      case "usuarios":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.USUARIOS_CONSULTAR}>
            <UsuariosPage />
          </ProtectedRoute>
        );

      case "roles":
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.ROLES_CONSULTAR}>
            <RolesPage />
          </ProtectedRoute>
        );

      case "dashboard":
      default:
        return (
          <ProtectedRoute requiredPermission={PERMISSIONS.DASHBOARD_CONSULTAR}>
            <DashboardPage onNavigate={(nextView) => setView(nextView)} />
          </ProtectedRoute>
        );
    }
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