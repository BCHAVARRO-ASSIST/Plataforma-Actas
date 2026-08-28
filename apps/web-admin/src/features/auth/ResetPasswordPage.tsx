import {
  useMemo,
  useState,
} from "react";

import { resetPassword } from "./authService";
import "../../styles/08-reset-password.css";

interface ResetPasswordPageProps {
  onBackToLogin: () => void;
}

export default function ResetPasswordPage({
  onBackToLogin,
}: ResetPasswordPageProps) {

  // =====================================================
  // TOKEN
  // =====================================================

  const token = useMemo(() => {

    const params =
      new URLSearchParams(
        window.location.search
      );

    return params.get("token") ?? "";

  }, []);


  // =====================================================
  // ESTADOS
  // =====================================================

  const [password, setPassword] =
    useState("");

  const [confirmPassword, setConfirmPassword] =
    useState("");

  const [loading, setLoading] =
    useState(false);

  const [message, setMessage] =
    useState("");

  const [error, setError] =
    useState("");

  const [showPassword, setShowPassword] =
    useState(false);

  const [showConfirmPassword, setShowConfirmPassword] =
    useState(false);


  // =====================================================
  // VALIDACIÓN
  // =====================================================

  const passwordLength =
    password.length;

  const hasMinimumLength =
    passwordLength >= 8;

  const passwordsMatch =
    password.length > 0 &&
    password === confirmPassword;


  // =====================================================
  // RESTABLECER CONTRASEÑA
  // =====================================================

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {

    event.preventDefault();

    setMessage("");
    setError("");


    if (!token) {

      setError(
        "El enlace de recuperación no contiene un token válido."
      );

      return;
    }


    if (!hasMinimumLength) {

      setError(
        "La contraseña debe tener al menos 8 caracteres."
      );

      return;
    }


    if (!passwordsMatch) {

      setError(
        "Las contraseñas no coinciden."
      );

      return;
    }


    try {

      setLoading(true);


      const response =
        await resetPassword({
          token,
          newPassword:
            password,
          confirmPassword:
            confirmPassword,
        });


      setMessage(
        response.message
      );


      setPassword("");
      setConfirmPassword("");


    } catch (error) {

      setError(
        error instanceof Error
          ? error.message
          : "No fue posible restablecer la contraseña."
      );

    } finally {

      setLoading(false);

    }

  }


  // =====================================================
  // VISTA
  // =====================================================

  return (
    <div className="reset-page">

      {/* =================================================
          PANEL IZQUIERDO
      ================================================= */}

      <section className="reset-visual-panel">

        <div className="reset-brand">

          <div className="reset-brand-mark">
            PA
          </div>

          <div>
            <strong>
              Plataforma
            </strong>

            <span>
              de Actas
            </span>
          </div>

        </div>


        <div className="reset-visual-content">

          <span className="reset-kicker">
            SEGURIDAD DE ACCESO
          </span>


          <h1>
            Recupera el
            <br />

            <span>
              control.
            </span>
          </h1>


          <p>
            Configura una nueva contraseña y
            vuelve a acceder a toda la operación
            de la Plataforma de Actas.
          </p>


          <div className="reset-security-card">

            <div className="reset-security-icon">
              ✓
            </div>

            <div>

              <strong>
                Recuperación segura
              </strong>

              <span>
                Tu enlace de recuperación es
                temporal y de uso único.
              </span>

            </div>

          </div>

        </div>


        <div className="reset-visual-footer">

          <span>
            PLATAFORMA DE ACTAS
          </span>

          <span>
            SISTEMA ADMINISTRATIVO
          </span>

        </div>

      </section>


      {/* =================================================
          PANEL FORMULARIO
      ================================================= */}

      <section className="reset-form-panel">

        <div className="reset-form-wrapper">

          <button
            type="button"
            className="reset-back-button"
            onClick={onBackToLogin}
            disabled={loading}
          >
            â† Volver al inicio
          </button>


          <div className="reset-form-header">

            <div className="reset-form-icon">
              ðŸ”
            </div>


            <span className="reset-form-kicker">
              RESTABLECER ACCESO
            </span>


            <h2>
              Nueva contraseña
            </h2>


            <p>
              Crea una contraseña segura para
              proteger tu cuenta.
            </p>

          </div>


          <form
            className="reset-form"
            onSubmit={handleSubmit}
          >

            {/* ===========================================
                NUEVA CONTRASEÑA
            =========================================== */}

            <div className="reset-field">

              <label htmlFor="password">
                Nueva contraseña
              </label>


              <div className="reset-input-wrapper">

                <input
                  id="password"
                  type={
                    showPassword
                      ? "text"
                      : "password"
                  }
                  value={password}
                  onChange={(event) =>
                    setPassword(
                      event.target.value
                    )
                  }
                  placeholder="Escribe tu nueva contraseña"
                  disabled={loading}
                  autoComplete="new-password"
                />


                <button
                  type="button"
                  className="reset-password-toggle"
                  onClick={() =>
                    setShowPassword(
                      !showPassword
                    )
                  }
                  disabled={loading}
                  aria-label={
                    showPassword
                      ? "Ocultar contraseña"
                      : "Mostrar contraseña"
                  }
                >
                  {showPassword
                    ? "Ocultar"
                    : "Ver"}
                </button>

              </div>


              <div className="password-strength">

                <div className="password-strength-track">

                  <span
                    className={
                      passwordLength >= 1
                        ? "active"
                        : ""
                    }
                  />

                  <span
                    className={
                      passwordLength >= 4
                        ? "active"
                        : ""
                    }
                  />

                  <span
                    className={
                      passwordLength >= 8
                        ? "active"
                        : ""
                    }
                  />

                  <span
                    className={
                      passwordLength >= 12
                        ? "active"
                        : ""
                    }
                  />

                </div>


                <span>
                  {passwordLength === 0
                    ? "Ingresa una contraseña"
                    : passwordLength < 8
                    ? "Contraseña corta"
                    : passwordLength < 12
                    ? "Contraseña aceptable"
                    : "Contraseña sólida"}
                </span>

              </div>

            </div>


            {/* ===========================================
                CONFIRMAR CONTRASEÑA
            =========================================== */}

            <div className="reset-field">

              <label htmlFor="confirmPassword">
                Confirmar contraseña
              </label>


              <div className="reset-input-wrapper">

                <input
                  id="confirmPassword"
                  type={
                    showConfirmPassword
                      ? "text"
                      : "password"
                  }
                  value={confirmPassword}
                  onChange={(event) =>
                    setConfirmPassword(
                      event.target.value
                    )
                  }
                  placeholder="Repite la contraseña"
                  disabled={loading}
                  autoComplete="new-password"
                />


                <button
                  type="button"
                  className="reset-password-toggle"
                  onClick={() =>
                    setShowConfirmPassword(
                      !showConfirmPassword
                    )
                  }
                  disabled={loading}
                  aria-label={
                    showConfirmPassword
                      ? "Ocultar contraseña"
                      : "Mostrar contraseña"
                  }
                >
                  {showConfirmPassword
                    ? "Ocultar"
                    : "Ver"}
                </button>

              </div>


              {confirmPassword.length > 0 && (

                <div
                  className={
                    passwordsMatch
                      ? "password-match success"
                      : "password-match error"
                  }
                >

                  <span>
                    {passwordsMatch
                      ? "✓"
                      : "!"}
                  </span>


                  {passwordsMatch
                    ? "Las contraseñas coinciden"
                    : "Las contraseñas no coinciden"}

                </div>

              )}

            </div>


            {/* ===========================================
                REQUISITOS
            =========================================== */}

            <div className="reset-requirements">

              <div className="requirements-title">
                La contraseña debe cumplir
              </div>


              <div className="requirement-item">

                <span
                  className={
                    hasMinimumLength
                      ? "requirement-check active"
                      : "requirement-check"
                  }
                >
                  {hasMinimumLength
                    ? "✓"
                    : "•"}
                </span>

                Mínimo 8 caracteres

              </div>


              <div className="requirement-item">

                <span
                  className={
                    passwordsMatch
                      ? "requirement-check active"
                      : "requirement-check"
                  }
                >
                  {passwordsMatch
                    ? "✓"
                    : "•"}
                </span>

                Las contraseñas deben coincidir

              </div>

            </div>


            {/* ===========================================
                MENSAJE ERROR
            =========================================== */}

            {error && (

              <div className="reset-alert reset-alert-error">

                <span>
                  !
                </span>

                <div>
                  {error}
                </div>

              </div>

            )}


            {/* ===========================================
                MENSAJE ÉXITO
            =========================================== */}

            {message && (

              <div className="reset-alert reset-alert-success">

                <span>
                  ✓
                </span>

                <div>
                  {message}
                </div>

              </div>

            )}


            {/* ===========================================
                BOTÓN
            =========================================== */}

            <button
              type="submit"
              className="reset-submit-button"
              disabled={
                loading ||
                !token
              }
            >

              <span>

                {loading
                  ? "Actualizando contraseña..."
                  : "Actualizar contraseña"}

              </span>


              {!loading && (
                <span className="reset-submit-arrow">
                  â†’
                </span>
              )}

            </button>

          </form>


          <div className="reset-footer-note">

            <span className="reset-lock">
              â—ˆ
            </span>

            Tu contraseña se almacena de forma
            segura y protegida.

          </div>

        </div>

      </section>

    </div>
  );
}
