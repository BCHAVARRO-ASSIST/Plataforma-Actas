import type { ChangeEvent, FormEvent } from "react";

interface LoginPageProps {
  username: string;
  password: string;
  loading: boolean;
  error: string;
  onUsernameChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onPasswordChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onForgotPassword: () => void;
}

export default function LoginPage({
  username,
  password,
  loading,
  error,
  onUsernameChange,
  onPasswordChange,
  onSubmit,
  onForgotPassword,
}: LoginPageProps) {
  return (
    <main className="login-page">
      <section className="login-visual-panel" aria-label="Plataforma de Actas">
        <div className="login-brand">
          <div className="login-brand-mark">PA</div>

          <div className="login-brand-copy">
            <strong>Plataforma</strong>
            <span>de Actas</span>
          </div>
        </div>

        <div className="login-visual-content">
          <span className="login-kicker">CONTROL OPERATIVO</span>

          <h1>
            La operación,
            <br />
            <span>en orden.</span>
          </h1>

          <p>
            Gestiona equipos, actas y trazabilidad desde un espacio central,
            claro y seguro.
          </p>

          <div className="login-signal-list" aria-label="Funciones principales">
            <div className="login-signal">
              <span className="login-signal-icon">01</span>
              <div>
                <strong>Actas trazables</strong>
                <span>Consulta cada entrega y devolución.</span>
              </div>
            </div>

            <div className="login-signal">
              <span className="login-signal-icon">02</span>
              <div>
                <strong>Inventario visible</strong>
                <span>Encuentra el estado de cada equipo.</span>
              </div>
            </div>
          </div>
        </div>

        <span className="login-visual-footer">SISTEMA ADMINISTRATIVO · 2026</span>
      </section>

      <section className="login-form-panel">
        <div className="login-form-wrap">
          <div className="login-form-heading">
            <span className="login-mobile-mark">PA</span>
            <span className="login-form-kicker">BIENVENIDO DE NUEVO</span>
            <h2>Inicia sesión</h2>
            <p>Ingresa tus credenciales para continuar.</p>
          </div>

          <form className="login-form" onSubmit={onSubmit}>
            <div className="login-field">
              <label htmlFor="loginUser">Usuario</label>
              <input
                id="loginUser"
                type="text"
                value={username}
                onChange={onUsernameChange}
                placeholder="Ej. jperez"
                autoComplete="username"
                disabled={loading}
                autoFocus
              />
            </div>

            <div className="login-field">
              <div className="login-label-row">
                <label htmlFor="loginPassword">Contraseña</label>
                <button
                  type="button"
                  className="login-inline-link"
                  onClick={onForgotPassword}
                  disabled={loading}
                >
                  ¿La olvidaste?
                </button>
              </div>

              <input
                id="loginPassword"
                type="password"
                value={password}
                onChange={onPasswordChange}
                placeholder="Ingresa tu contraseña"
                autoComplete="current-password"
                disabled={loading}
              />
            </div>

            {error && (
              <div className="login-alert" role="alert">
                <span aria-hidden="true">!</span>
                {error}
              </div>
            )}

            <button
              type="submit"
              className="login-submit-button"
              disabled={loading}
            >
              <span>{loading ? "Ingresando..." : "Iniciar sesión"}</span>
              {!loading && <span aria-hidden="true">→</span>}
            </button>
          </form>

          <p className="login-help-text">
            ¿Problemas para acceder? Contacta al administrador del sistema.
          </p>
        </div>
      </section>
    </main>
  );
}
