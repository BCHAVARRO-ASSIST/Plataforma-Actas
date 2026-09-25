import { useState, useEffect, useRef, type ChangeEvent, type FormEvent } from "react";
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  EyeIcon,
  EyeOffIcon,
  LockIcon,
  UserIcon,
} from "@/components/ui/Icons";
import { ThemeToggle, useTheme } from "@/app/theme";

declare global {
  interface Window {
    __gis_initialized?: boolean;
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential?: string }) => void;
            hosted_domain?: string;
            auto_select?: boolean;
          }) => void;
          renderButton: (
            parent: HTMLElement,
            options: {
              type?: string;
              theme?: string;
              size?: string;
              text?: string;
              shape?: string;
              logo_alignment?: string;
              width?: number;
            }
          ) => void;
          prompt: () => void;
          cancel: () => void;
        };
      };
    };
  }
}

interface LoginPageProps {
  username: string;
  password: string;
  loading: boolean;
  error: string;
  onUsernameChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onPasswordChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onGoogleLogin?: (idToken: string) => void;
}

let activeGoogleLoginHandler: ((credential: string) => void) | null = null;

export default function LoginPage({
  username,
  password,
  loading,
  error,
  onUsernameChange,
  onPasswordChange,
  onSubmit,
  onGoogleLogin,
}: LoginPageProps) {
  const [showPassword, setShowPassword] = useState(false);
  const [showLocalAdmin, setShowLocalAdmin] = useState(false);

  const googleBtnContainerRef = useRef<HTMLDivElement>(null);
  const { theme: currentTheme } = useTheme();

  useEffect(() => {
    activeGoogleLoginHandler = onGoogleLogin || null;
  }, [onGoogleLogin]);

  const googleClientId =
    import.meta.env.VITE_GOOGLE_CLIENT_ID ||
    "725167561557-69ivn9ir2avp9vdv725jhjoebpqrpe6m.apps.googleusercontent.com";

  useEffect(() => {
    if (showLocalAdmin) return;

    function renderGisButton() {
      if (!window.google?.accounts?.id || !googleClientId) return;

      try {
        if (!window.__gis_initialized) {
          window.google.accounts.id.initialize({
            client_id: googleClientId,
            callback: (response: { credential?: string }) => {
              if (response.credential && activeGoogleLoginHandler) {
                activeGoogleLoginHandler(response.credential);
              }
            },
            hosted_domain: "assist.com.co",
            auto_select: false,
          });
          window.__gis_initialized = true;
        }

        if (googleBtnContainerRef.current) {
          googleBtnContainerRef.current.innerHTML = "";
          const containerWidth = googleBtnContainerRef.current.parentElement?.clientWidth || window.innerWidth || 360;
          const btnWidth = Math.min(380, Math.max(240, containerWidth - 16));
          window.google.accounts.id.renderButton(googleBtnContainerRef.current, {
            type: "standard",
            theme: currentTheme === "dark" ? "filled_black" : "outline",
            size: "large",
            text: "continue_with",
            shape: "rectangular",
            logo_alignment: "left",
            width: btnWidth,
          });
        }
      } catch (err) {
        console.error("Error al renderizar botón de Google Identity Services:", err);
      }
    }

    if (window.google?.accounts?.id) {
      renderGisButton();
    } else {
      const interval = setInterval(() => {
        if (window.google?.accounts?.id) {
          clearInterval(interval);
          renderGisButton();
        }
      }, 100);
      return () => clearInterval(interval);
    }
  }, [googleClientId, showLocalAdmin, currentTheme]);

  return (
    <main className="login-page-container auth-container">
      {/* Selector de tema (Dark/Light mode switch) */}
      <div className="login-theme-toggle auth-theme-toggle-slot">
        <ThemeToggle />
      </div>

      {/* Luces ambientales de profundidad */}
      <div className="auth-ambient-light auth-ambient-1" aria-hidden="true" />
      <div className="auth-ambient-light auth-ambient-2" aria-hidden="true" />

      {/* Tarjeta principal con comportamiento fluido */}
      <div className="login-card auth-card">
        {/* Encabezado con Identidad Corporativa Oficial ASSIST */}
        <div className="login-header">
          <img
            src="/brand/assist-logo.png"
            alt="ASSIST Consultores"
            className="login-brand-logo"
            width={180}
            height={48}
          />
          <h1 className="login-title">Plataforma de Actas</h1>
          <p className="login-subtitle">Gestión Documental y Custodia de Activos</p>
        </div>

        {/* Notificación de error si existe */}
        {error && (
          <div className="auth-alert error" role="alert">
            <span>{error}</span>
          </div>
        )}

        {!showLocalAdmin ? (
          /* ================= ACCESO CORPORATIVO (GOOGLE OAUTH 2.0) ================= */
          <div className="login-btn-group auth-corporate-flow">
            {/* Contenedor oficial exclusivo para Google Identity Services */}
            <div
              ref={googleBtnContainerRef}
              className="auth-gis-btn-slot"
              style={{
                minHeight: "48px",
                display: "flex",
                justifyContent: "center",
                width: "100%",
              }}
            />

            <button
              type="button"
              className="btn-local-auth auth-link-btn"
              onClick={() => setShowLocalAdmin(true)}
              disabled={loading}
            >
              <LockIcon size={16} />
              <span>Acceso con credenciales locales</span>
            </button>
          </div>
        ) : (
          /* ================= ACCESO ADMINISTRADOR LOCAL ================= */
          <form onSubmit={onSubmit} className="auth-form" style={{ width: "100%" }}>
            <div className="form-group">
              <label htmlFor="loginUser" className="auth-label">
                Usuario
              </label>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <UserIcon size={18} />
                </span>
                <input
                  id="loginUser"
                  name="username"
                  className="auth-input"
                  type="text"
                  value={username}
                  onChange={onUsernameChange}
                  placeholder="admin"
                  autoComplete="username"
                  disabled={loading}
                  autoFocus
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="loginPassword" className="auth-label">
                Contraseña
              </label>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <LockIcon size={18} />
                </span>
                <input
                  id="loginPassword"
                  name="password"
                  className="auth-input"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={onPasswordChange}
                  placeholder="••••••••••••"
                  autoComplete="current-password"
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  className="auth-password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                  title={showPassword ? "Ocultar contraseña" : "Ver contraseña"}
                  aria-label={showPassword ? "Ocultar contraseña" : "Ver contraseña"}
                >
                  {showPassword ? <EyeOffIcon size={16} /> : <EyeIcon size={16} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="auth-btn-primary"
              disabled={loading}
              style={{ minHeight: "48px" }}
            >
              {loading ? (
                <span>Verificando...</span>
              ) : (
                <>
                  <span>Iniciar sesión</span>
                  <ArrowRightIcon size={16} className="auth-btn-arrow" />
                </>
              )}
            </button>

            <button
              type="button"
              className="btn-local-auth auth-link-btn"
              onClick={() => setShowLocalAdmin(false)}
              disabled={loading}
              style={{ marginTop: "0.5rem" }}
            >
              <ArrowLeftIcon size={16} />
              <span>Volver al acceso corporativo</span>
            </button>
          </form>
        )}
      </div>
    </main>
  );
}
