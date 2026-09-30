import { useState, useEffect } from "react";
import {
  LockIcon,
  UserIcon,
  EyeIcon,
  EyeOffIcon,
  CheckCircleIcon,
  AlertCircleIcon,
} from "@/components/ui/Icons";
import {
  requestPasswordReset,
  resetPasswordWithToken,
} from "./authService";

interface PasswordRecoveryModalProps {
  isOpen: boolean;
  initialToken?: string;
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export function PasswordRecoveryModal({
  isOpen,
  initialToken = "",
  onClose,
  onSuccess,
}: PasswordRecoveryModalProps) {
  const [step, setStep] = useState<"request" | "verify">(
    initialToken ? "verify" : "request"
  );
  const [identifier, setIdentifier] = useState("");
  const [tokenOrCode, setTokenOrCode] = useState(initialToken);
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);
  const [resendCooldown, setResendCooldown] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [infoMessage, setInfoMessage] = useState<string | null>(null);

  useEffect(() => {
    if (initialToken) {
      setTokenOrCode(initialToken);
      setStep("verify");
    }
  }, [initialToken]);

  useEffect(() => {
    if (resendCooldown > 0) {
      const timer = setTimeout(() => setResendCooldown(resendCooldown - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [resendCooldown]);

  if (!isOpen) return null;

  async function handleSendCode(e: React.FormEvent) {
    e.preventDefault();
    if (!identifier.trim()) {
      setError("Por favor ingresa tu correo institucional o usuario de red.");
      return;
    }

    setError(null);
    setInfoMessage(null);
    setLoading(true);

    try {
      const res = await requestPasswordReset(identifier);
      setInfoMessage(
        res.message ||
          "Se ha enviado un código de validación a tu correo corporativo."
      );
      setStep("verify");
      setResendCooldown(60);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Error al solicitar el código de recuperación."
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleResend() {
    if (resendCooldown > 0 || !identifier.trim()) return;

    setError(null);
    setResending(true);

    try {
      const res = await requestPasswordReset(identifier);
      setInfoMessage(
        res.message || "Nuevo código enviado a tu correo corporativo."
      );
      setResendCooldown(60);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Error al reenviar el código."
      );
    } finally {
      setResending(false);
    }
  }

  async function handleResetPassword(e: React.FormEvent) {
    e.preventDefault();
    if (!tokenOrCode.trim()) {
      setError("El código de validación o token es obligatorio.");
      return;
    }

    if (newPassword.length < 10) {
      setError("La contraseña debe tener al menos 10 caracteres.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    setError(null);
    setLoading(true);

    try {
      const res = await resetPasswordWithToken({
        token: tokenOrCode.trim(),
        newPassword,
        confirmPassword,
      });

      onSuccess(
        res.message || "Tu contraseña ha sido restablecida exitosamente."
      );
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Error al restablecer la contraseña."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="modal-overlay"
      role="dialog"
      aria-modal="true"
      aria-labelledby="recovery-modal-title"
      style={{ zIndex: 1000 }}
    >
      <div
        className="modal-content"
        style={{ maxWidth: "460px", width: "92%", padding: "1.75rem" }}
      >
        <div className="modal-header" style={{ marginBottom: "1rem" }}>
          <div>
            <h2 id="recovery-modal-title" className="modal-title" style={{ fontSize: "1.25rem" }}>
              {step === "request"
                ? "Recuperación de Contraseña"
                : "Validación y Nueva Contraseña"}
            </h2>
            <p className="text-secondary" style={{ fontSize: "0.875rem", marginTop: "0.25rem" }}>
              {step === "request"
                ? "Ingresa tu usuario o correo institucional para recibir las instrucciones vía Gmail / SMTP."
                : "Introduce el código recibido por correo y define tu nueva contraseña segura."}
            </p>
          </div>
          <button
            type="button"
            className="modal-close"
            onClick={onClose}
            aria-label="Cerrar ventana"
          >
            &times;
          </button>
        </div>

        {error && (
          <div
            className="auth-alert error"
            role="alert"
            style={{ marginBottom: "1rem", display: "flex", gap: "0.5rem", alignItems: "center" }}
          >
            <AlertCircleIcon size={16} />
            <span>{error}</span>
          </div>
        )}

        {infoMessage && (
          <div
            className="auth-alert info"
            role="status"
            style={{
              marginBottom: "1rem",
              display: "flex",
              gap: "0.5rem",
              alignItems: "center",
              backgroundColor: "rgba(16, 185, 129, 0.12)",
              color: "#10b981",
              border: "1px solid rgba(16, 185, 129, 0.25)",
              padding: "0.75rem 1rem",
              borderRadius: "8px",
            }}
          >
            <CheckCircleIcon size={16} />
            <span>{infoMessage}</span>
          </div>
        )}

        {step === "request" ? (
          <form onSubmit={handleSendCode} className="auth-form">
            <div className="form-group" style={{ marginBottom: "1.25rem" }}>
              <label htmlFor="recoveryIdentifier" className="auth-label">
                Usuario o Correo Institucional
              </label>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <UserIcon size={18} />
                </span>
                <input
                  id="recoveryIdentifier"
                  type="text"
                  className="auth-input"
                  placeholder="ej. cvalencia@assist.com.co o cvalencia"
                  value={identifier}
                  onChange={(e) => setIdentifier(e.target.value)}
                  disabled={loading}
                  autoFocus
                  required
                />
              </div>
            </div>

            <div style={{ display: "flex", gap: "0.75rem", marginTop: "1.5rem" }}>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
                disabled={loading}
                style={{ flex: 1 }}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="auth-btn-primary"
                disabled={loading}
                style={{ flex: 1.5, minHeight: "44px" }}
              >
                {loading ? "Enviando..." : "Enviar Código"}
              </button>
            </div>
          </form>
        ) : (
          <form onSubmit={handleResetPassword} className="auth-form">
            <div className="form-group" style={{ marginBottom: "1rem" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <label htmlFor="recoveryToken" className="auth-label">
                  Código de Validación / Token
                </label>
                {identifier && (
                  <button
                    type="button"
                    onClick={handleResend}
                    disabled={resendCooldown > 0 || resending}
                    style={{
                      background: "none",
                      border: "none",
                      color: resendCooldown > 0 ? "var(--color-text-muted)" : "var(--color-primary)",
                      fontSize: "0.8rem",
                      cursor: resendCooldown > 0 ? "default" : "pointer",
                      padding: 0,
                    }}
                  >
                    {resendCooldown > 0
                      ? `Reenviar en ${resendCooldown}s`
                      : resending
                      ? "Reenviando..."
                      : "Reenviar código"}
                  </button>
                )}
              </div>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <LockIcon size={18} />
                </span>
                <input
                  id="recoveryToken"
                  type="text"
                  className="auth-input"
                  placeholder="Introduce el código o token recibido"
                  value={tokenOrCode}
                  onChange={(e) => setTokenOrCode(e.target.value)}
                  disabled={loading}
                  autoFocus
                  required
                />
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: "1rem" }}>
              <label htmlFor="recoveryNewPassword" className="auth-label">
                Nueva Contraseña
              </label>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <LockIcon size={18} />
                </span>
                <input
                  id="recoveryNewPassword"
                  type={showPassword ? "text" : "password"}
                  className="auth-input"
                  placeholder="Mínimo 10 caracteres (A-Z, a-z, 0-9, @#$)"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  className="auth-password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOffIcon size={16} /> : <EyeIcon size={16} />}
                </button>
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: "1.25rem" }}>
              <label htmlFor="recoveryConfirmPassword" className="auth-label">
                Confirmar Nueva Contraseña
              </label>
              <div className="auth-input-box">
                <span className="auth-input-icon">
                  <LockIcon size={18} />
                </span>
                <input
                  id="recoveryConfirmPassword"
                  type={showPassword ? "text" : "password"}
                  className="auth-input"
                  placeholder="Repite la nueva contraseña"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  disabled={loading}
                  required
                />
              </div>
            </div>

            <div style={{ display: "flex", gap: "0.75rem", marginTop: "1.5rem" }}>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setStep("request")}
                disabled={loading}
                style={{ flex: 1 }}
              >
                Volver
              </button>
              <button
                type="submit"
                className="auth-btn-primary"
                disabled={loading}
                style={{ flex: 1.5, minHeight: "44px" }}
              >
                {loading ? "Restableciendo..." : "Guardar Contraseña"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
