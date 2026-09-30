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
import { PasswordRecoveryModal } from "./PasswordRecoveryModal";

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
  const [showRecoveryModal, setShowRecoveryModal] = useState(false);
  const [recoverySuccessMessage, setRecoverySuccessMessage] = useState<string | null>(null);
  const [recoveryInitialToken, setRecoveryInitialToken] = useState("");

  const googleBtnContainerRef = useRef<HTMLDivElement>(null);
  const { theme: currentTheme } = useTheme();

  useEffect(() => {
    if (typeof window !== "undefined") {
      const params = new URLSearchParams(window.location.search);
      const urlToken = params.get("token") || params.get("resetToken") || params.get("code");
      if (urlToken) {
        setRecoveryInitialToken(urlToken);
        setShowRecoveryModal(true);
      }
    }
  }, []);

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
        {/* Encabezado con Insignia Oficial de la Plataforma */}
        <div className="login-header">
          <div className="login-product-badge" aria-hidden="true">
            <svg
              className="login-product-icon"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="1.8"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              {/* Documento base */}
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
              <polyline points="14 2 14 8 20 8" />
              {/* Líneas de acta y firma */}
              <line x1="16" y1="13" x2="8" y2="13" />
              <line x1="16" y1="17" x2="8" y2="17" />
              <polyline points="10 9 9 9 8 9" />
              {/* Sello de validación */}
              <path d="m15 15 3-3 3 3-3 3z" fill="currentColor" fillOpacity="0.25" />
            </svg>
          </div>
          <h1 className="login-title">Plataforma de Actas</h1>
          <p className="login-subtitle">Gestión Documental y Custodia de Activos</p>
        </div>

        {/* Notificación de error si existe */}
        {error && (
          <div className="auth-alert error" role="alert">
            <span>{error}</span>
          </div>
        )}

        {/* Notificación de éxito si fue recuperada la contraseña */}
        {recoverySuccessMessage && (
          <div
            className="auth-alert success"
            role="status"
            style={{
              backgroundColor: "rgba(16, 185, 129, 0.12)",
              color: "#10b981",
              border: "1px solid rgba(16, 185, 129, 0.25)",
              padding: "0.75rem 1rem",
              borderRadius: "8px",
              marginBottom: "1rem",
            }}
          >
            <span>{recoverySuccessMessage}</span>
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

            <button
              type="button"
              className="auth-link-btn"
              onClick={() => {
                setShowRecoveryModal(true);
                setRecoverySuccessMessage(null);
              }}
              style={{
                background: "none",
                border: "none",
                color: "var(--color-text-secondary, #6b7280)",
                fontSize: "0.825rem",
                marginTop: "0.5rem",
                cursor: "pointer",
              }}
            >
              ¿Problemas para acceder o validar credenciales?
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
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <label htmlFor="loginPassword" className="auth-label">
                  Contraseña
                </label>
                <button
                  type="button"
                  onClick={() => {
                    setShowRecoveryModal(true);
                    setRecoverySuccessMessage(null);
                  }}
                  style={{
                    background: "none",
                    border: "none",
                    color: "var(--color-primary, #3b82f6)",
                    fontSize: "0.8rem",
                    cursor: "pointer",
                    padding: 0,
                  }}
                >
                  ¿Olvidaste tu contraseña?
                </button>
              </div>
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

      <PasswordRecoveryModal
        isOpen={showRecoveryModal}
        initialToken={recoveryInitialToken}
        onClose={() => setShowRecoveryModal(false)}
        onSuccess={(msg) => {
          setRecoverySuccessMessage(msg);
          setShowRecoveryModal(false);
        }}
      />
    </main>
  );
}
