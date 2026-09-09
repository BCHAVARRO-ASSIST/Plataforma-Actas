import { useState, type ChangeEvent, type FormEvent } from "react";
import {
  ArrowRightIcon,
  EyeIcon,
  EyeOffIcon,
  LockIcon,
  ShieldIcon,
  UserIcon,
} from "../../components/Icons";

interface LoginPageProps {
  username: string;
  password: string;
  loading: boolean;
  error: string;
  onUsernameChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onPasswordChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
}

export default function LoginPage({
  username,
  password,
  loading,
  error,
  onUsernameChange,
  onPasswordChange,
  onSubmit,
}: LoginPageProps) {
  const [showPassword, setShowPassword] = useState(false);

  return (
    <main className="auth-container">
      {/* Esferas de Luz Ambiental Animadas (Background Flow) */}
      <div className="auth-orb auth-orb-1" aria-hidden="true" />
      <div className="auth-orb auth-orb-2" aria-hidden="true" />
      <div className="auth-orb auth-orb-3" aria-hidden="true" />

      {/* Contenedor de la Tarjeta con Halo de Luz Suave */}
      <div className="auth-card-wrapper">
        <div className="auth-card-glow" aria-hidden="true" />

        <section className="auth-card" aria-label="Iniciar Sesión">
          {/* Encabezado con Logotipo Pulsante */}
          <header className="auth-card-header">
            <div className="auth-logo-badge">
              <ShieldIcon size={26} />
            </div>
            <h1>Plataforma de Actas</h1>
            <p>Inicia sesión con tus credenciales corporativas</p>
          </header>

          {/* Formulario de Acceso */}
          <form onSubmit={onSubmit}>
            {/* Campo Usuario */}
            <div className="form-group" style={{ marginBottom: "16px" }}>
              <label
                className="form-label"
                htmlFor="loginUser"
                style={{ fontSize: "12.5px", fontWeight: 600, color: "var(--color-slate-700)" }}
              >
                Usuario o Correo
              </label>
              <div className="auth-input-wrapper">
                <span className="auth-input-icon-left">
                  <UserIcon size={16} />
                </span>
                <input
                  id="loginUser"
                  name="username"
                  className="auth-input-field"
                  type="text"
                  value={username}
                  onChange={onUsernameChange}
                  placeholder="Ej. admin o jdoe@empresa.com"
                  autoComplete="username"
                  disabled={loading}
                  autoFocus
                  required
                />
              </div>
            </div>

            {/* Campo Contraseña */}
            <div className="form-group" style={{ marginBottom: "18px" }}>
              <label
                className="form-label"
                htmlFor="loginPassword"
                style={{ fontSize: "12.5px", fontWeight: 600, color: "var(--color-slate-700)" }}
              >
                Contraseña
              </label>
              <div className="auth-input-wrapper">
                <span className="auth-input-icon-left">
                  <LockIcon size={16} />
                </span>
                <input
                  id="loginPassword"
                  name="password"
                  className="auth-input-field"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={onPasswordChange}
                  placeholder="Ingresa tu contraseña"
                  autoComplete="current-password"
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  className="auth-input-toggle-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                  title={showPassword ? "Ocultar contraseña" : "Ver contraseña"}
                  aria-label={showPassword ? "Ocultar contraseña" : "Ver contraseña"}
                >
                  {showPassword ? <EyeOffIcon size={16} /> : <EyeIcon size={16} />}
                </button>
              </div>
            </div>

            {/* Mensaje de Error con Animación Shake */}
            {error && (
              <div className="auth-alert error" role="alert">
                <span style={{ fontWeight: 800 }}>!</span>
                <span>{error}</span>
              </div>
            )}

            {/* Botón Principal con Efecto Shimmer y Flecha Deslizante */}
            <button
              type="submit"
              className="auth-submit-btn"
              disabled={loading}
            >
              {loading ? (
                <>
                  <div
                    style={{
                      width: "16px",
                      height: "16px",
                      border: "2px solid rgba(255,255,255,0.3)",
                      borderTopColor: "#fff",
                      borderRadius: "50%",
                      animation: "spin 0.7s linear infinite",
                    }}
                  />
                  <span>Accediendo...</span>
                </>
              ) : (
                <>
                  <span>Iniciar sesión</span>
                  <span className="btn-arrow">
                    <ArrowRightIcon size={16} />
                  </span>
                </>
              )}
            </button>
          </form>

          {/* Pie de Página */}
          <footer className="auth-card-footer">
            <p>
              ¿Olvidaste tu contraseña o requieres acceso?
              <br />
              Contacta al administrador del sistema.
            </p>
          </footer>
        </section>
      </div>
    </main>
  );
}
