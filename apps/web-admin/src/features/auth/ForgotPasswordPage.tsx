import { useState } from "react";

import type { FormEvent } from "react";

import { forgotPassword } from "./authService";
import "../../styles/12-forgot-password.css";

interface ForgotPasswordPageProps {
  onBackToLogin: () => void;
}

export default function ForgotPasswordPage({
  onBackToLogin,
}: ForgotPasswordPageProps) {
  
  const [identifier, setIdentifier] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage("");
    setError("");

    if (!identifier.trim()) {
      setError("Ingresa tu usuario o correo electrÃ³nico.");
      return;
    }

    try {
      setLoading(true);
      const response = await forgotPassword(identifier);
      setMessage(response.message);
      setIdentifier("");
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "No fue posible solicitar la recuperaciÃ³n."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="recovery-page">
      <section className="recovery-visual-panel" aria-label="RecuperaciÃ³n de acceso">
        <div className="recovery-brand">
          <div className="recovery-brand-mark">PA</div>
          <div>
            <strong>Plataforma</strong>
            <span>de Actas</span>
          </div>
        </div>

        <div className="recovery-visual-content">
          <span className="recovery-kicker">SEGURIDAD DE ACCESO</span>
          <h1>
            Vuelve al
            <br />
            <span>control.</span>
          </h1>
          <p>
            Te enviaremos un enlace temporal para recuperar el acceso a la
            Plataforma de Actas.
          </p>

          <div className="recovery-security-card">
            <span className="recovery-security-icon" aria-hidden="true">âœ“</span>
            <div>
              <strong>RecuperaciÃ³n segura</strong>
              <span>El enlace serÃ¡ temporal y de uso Ãºnico.</span>
            </div>
          </div>
        </div>

        <span className="recovery-footer">SISTEMA ADMINISTRATIVO Â· 2026</span>
      </section>

      <section className="recovery-form-panel">
        <div className="recovery-form-wrapper">
          <button
            type="button"
            className="recovery-back-button"
            onClick={onBackToLogin}
            disabled={loading}
          >
            <span aria-hidden="true">â†</span> Volver al inicio de sesiÃ³n
          </button>

          <div className="recovery-form-header">
            <span className="recovery-form-icon" aria-hidden="true">?</span>
            <span className="recovery-form-kicker">RECUPERAR ACCESO</span>
            <h2>Â¿Olvidaste tu contraseÃ±a?</h2>
            <p>Indica tu usuario o correo y te enviaremos las instrucciones.</p>
          </div>

          <form className="recovery-form" onSubmit={handleSubmit}>
            <div className="recovery-field">
              <label htmlFor="recoveryIdentifier">Usuario o correo electrÃ³nico</label>
              <input
                id="recoveryIdentifier"
                type="text"
                value={identifier}
                onChange={(event) => setIdentifier(event.target.value)}
                placeholder="Ej. jperez o jperez@empresa.com"
                autoComplete="username"
                disabled={loading}
                autoFocus
              />
            </div>

            {error && (
              <div className="recovery-alert recovery-alert-error" role="alert">
                <span aria-hidden="true">!</span>
                {error}
              </div>
            )}

            {message && (
              <div className="recovery-alert recovery-alert-success" role="status">
                <span aria-hidden="true">âœ“</span>
                {message}
              </div>
            )}

            <button
              type="submit"
              className="recovery-submit-button"
              disabled={loading}
            >
              <span>{loading ? "Enviando..." : "Enviar enlace de recuperaciÃ³n"}</span>
              {!loading && <span aria-hidden="true">â†’</span>}
            </button>
          </form>

          <p className="recovery-footer-note">
            Si no recibes el correo, revisa tu carpeta de spam o contacta al administrador.
          </p>
        </div>
      </section>
    </main>
  );
}

