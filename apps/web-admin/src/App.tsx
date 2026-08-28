import { useState, useEffect } from "react";
import type { FormEvent, ChangeEvent } from "react";

import { EquipmentPage } from "./features/equipment/EquipmentPage";
import { ActaPage } from "./features/actas/ActaPage";
import { DashboardPage } from "./features/dashboard/DashboardPage";
import { ReportesPage } from "./features/reportes/ReportesPage";
import { MantenimientosPage } from "./features/mantenimientos/MantenimientosPage";
import { UsuariosPage } from "./features/usuarios/UsuariosPage";
import { RolesPage } from "./features/roles/RolesPage";
import { MensajeroPage } from "./features/mensajero/MensajeroPage";
import { MensajeroActaDetail } from "./features/mensajero/components/MensajeroActaDetail";

import ForgotPasswordPage from "./features/auth/ForgotPasswordPage";
import ResetPasswordPage from "./features/auth/ResetPasswordPage";
import LoginPage from "./features/auth/LoginPage";

import { hasPermission, PERMISSIONS } from "./features/auth/permissions";
import { AuthProvider, useAuth } from "./features/auth/AuthContext";

import { AppShell } from "./app/layout/AppShell";
import { Sidebar } from "./app/layout/Sidebar";
import { Header } from "./app/layout/Header";
import { NAVIGATION_ITEMS } from "./app/navigation/navigation";
import type { View } from "./app/navigation/navigationTypes";

type AuthView = "login" | "forgot-password" | "reset-password";

function AppContent() {
  const { user, token, login, logout, isAuthenticated } = useAuth();

  const [authView, setAuthView] = useState<AuthView>("login");
  const [view, setView] = useState<View>("dashboard");
  const [selectedMensajeroActaId, setSelectedMensajeroActaId] = useState<string | null>(null);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  // Login Form States
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loginLoading, setLoginLoading] = useState(false);
  const [loginError, setLoginError] = useState<string | null>(null);

  // =======================================================
  // ADAPTAR VISTA INICIAL SEGÚN ROL OPERATIVO
  // =======================================================

  useEffect(() => {
    if (user) {
      const isMensajeroOnly =
        user.rol === "Mensajero" ||
        (!hasPermission(token, PERMISSIONS.DASHBOARD_CONSULTAR) &&
          hasPermission(token, PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR));

      if (isMensajeroOnly) {
        setView("mensajero");
      }
    }
  }, [user, token]);

  // =======================================================
  // FILTRAR ÍTEMS DE NAVEGACIÓN POR PERMISOS
  // =======================================================

  const navigationItems = NAVIGATION_ITEMS.filter((item) => {
    if (!item.permission) return true;
    return hasPermission(token, item.permission);
  });

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
    setAuthView("login");
    setView("dashboard");
    setSelectedMensajeroActaId(null);
  }

  // =======================================================
  // RENDERIZADO DE LOGIN / RECUPERACIÓN
  // =======================================================

  if (!isAuthenticated) {
    if (authView === "forgot-password") {
      return (
        <ForgotPasswordPage
          onBackToLogin={() => setAuthView("login")}
        />
      );
    }

    if (authView === "reset-password") {
      return (
        <ResetPasswordPage
          onBackToLogin={() => setAuthView("login")}
        />
      );
    }

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
        onForgotPassword={() => setAuthView("forgot-password")}
      />
    );
  }

  // =======================================================
  // RENDERIZADO DE VISTA ACTIVA
  // =======================================================

  function renderCurrentView() {
    if (view === "equipment" && hasPermission(token, PERMISSIONS.EQUIPOS_CONSULTAR)) {
      return <EquipmentPage />;
    }

    if (view === "actas" && hasPermission(token, PERMISSIONS.ACTAS_CONSULTAR)) {
      return <ActaPage />;
    }

    if (view === "mantenimientos" && hasPermission(token, PERMISSIONS.EQUIPOS_CONSULTAR)) {
      return <MantenimientosPage />;
    }

    if (view === "reportes" && hasPermission(token, PERMISSIONS.REPORTES_CONSULTAR)) {
      return <ReportesPage />;
    }

    if (view === "mensajero" && hasPermission(token, PERMISSIONS.MENSAJERO_ACTAS_CONSULTAR)) {
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

    if (view === "usuarios" && hasPermission(token, PERMISSIONS.USUARIOS_CONSULTAR)) {
      return <UsuariosPage />;
    }

    if (view === "roles" && hasPermission(token, PERMISSIONS.ROLES_CONSULTAR)) {
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
          view={view}
          navigationItems={navigationItems}
          isOpen={mobileMenuOpen}
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
          onToggleMenu={() => setMobileMenuOpen(true)}
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