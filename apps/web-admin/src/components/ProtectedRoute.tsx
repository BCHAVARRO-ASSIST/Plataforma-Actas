import type { ReactNode } from "react";
import { useAuth } from "@/features/auth/useAuth";
import { hasPermission, type Permission } from "@/features/auth/permissions";
import { AlertCircleIcon, ShieldIcon } from "@/components/ui/Icons";

export interface ProtectedRouteProps {
  children: ReactNode;
  requiredPermission?: Permission;
  fallback?: ReactNode;
}

export function ProtectedRoute({
  children,
  requiredPermission,
  fallback,
}: ProtectedRouteProps) {
  const { user, isAuthenticated, isRestoringSession } = useAuth();

  // 1. Estado de verificación de sesión segura al iniciar
  if (isRestoringSession) {
    return (
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "50vh",
          padding: "3rem 1.5rem",
          color: "var(--color-text-secondary, #64748b)",
        }}
      >
        <div
          style={{
            width: "48px",
            height: "48px",
            border: "4px solid var(--color-border, #e2e8f0)",
            borderTopColor: "var(--color-primary, #2563eb)",
            borderRadius: "50%",
            animation: "spin 0.8s linear infinite",
            marginBottom: "1.25rem",
          }}
        />
        <p
          style={{
            fontSize: "1rem",
            fontWeight: 500,
            color: "var(--color-text-primary, #1e293b)",
            margin: 0,
          }}
        >
          Verificando sesión segura...
        </p>
        <span style={{ fontSize: "0.85rem", marginTop: "0.25rem" }}>
          Validando credenciales y entorno protegido
        </span>
        <style>{`
          @keyframes spin {
            to { transform: rotate(360deg); }
          }
        `}</style>
      </div>
    );
  }

  // 2. Si no está autenticado, no renderiza contenido protegido
  if (!isAuthenticated) {
    return fallback ? <>{fallback}</> : null;
  }

  // 3. Si se requiere un permiso y el usuario no lo posee, mostrar acceso denegado sin cerrar la sesión
  if (requiredPermission && !hasPermission(user, requiredPermission)) {
    if (fallback) {
      return <>{fallback}</>;
    }

    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "60vh",
          padding: "2rem",
        }}
      >
        <div
          style={{
            maxWidth: "520px",
            width: "100%",
            backgroundColor: "var(--color-surface, #ffffff)",
            border: "1px solid var(--color-danger-100, #fee2e2)",
            borderRadius: "16px",
            padding: "2.5rem 2rem",
            textAlign: "center",
            boxShadow: "0 10px 25px -5px rgba(0, 0, 0, 0.05)",
          }}
        >
          <div
            style={{
              width: "64px",
              height: "64px",
              borderRadius: "50%",
              backgroundColor: "var(--color-danger-50, #fff1f2)",
              color: "var(--color-danger-600, #e11d48)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              margin: "0 auto 1.5rem auto",
            }}
          >
            <ShieldIcon style={{ width: "32px", height: "32px" }} />
          </div>

          <h2
            style={{
              fontSize: "1.35rem",
              fontWeight: 700,
              color: "var(--color-text-primary, #0f172a)",
              marginBottom: "0.5rem",
            }}
          >
            Acceso Restringido
          </h2>

          <p
            style={{
              fontSize: "0.95rem",
              color: "var(--color-text-secondary, #64748b)",
              lineHeight: 1.5,
              marginBottom: "1.25rem",
            }}
          >
            Tu rol actual (<strong>{user?.rol || "Usuario"}</strong>) no dispone de los permisos necesarios para acceder a esta sección.
          </p>

          <div
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: "0.5rem",
              padding: "0.5rem 0.85rem",
              borderRadius: "8px",
              backgroundColor: "var(--color-slate-50, #f8fafc)",
              border: "1px solid var(--color-slate-200, #e2e8f0)",
              fontSize: "0.8rem",
              color: "var(--color-slate-600, #475569)",
              fontFamily: "monospace",
              marginBottom: "1.5rem",
            }}
          >
            <AlertCircleIcon style={{ width: "16px", height: "16px", color: "var(--color-amber-500, #f59e0b)" }} />
            Permiso requerido: {requiredPermission}
          </div>

          <p
            style={{
              fontSize: "0.85rem",
              color: "var(--color-text-muted, #94a3b8)",
              margin: 0,
            }}
          >
            Tu sesión permanece activa de forma segura. Si necesitas acceder a este módulo, comunícate con un Administrador del sistema.
          </p>
        </div>
      </div>
    );
  }

  // 4. Usuario autenticado y con permisos válidos
  return <>{children}</>;
}
