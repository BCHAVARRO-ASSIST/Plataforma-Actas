import { useState, type KeyboardEvent } from "react";

/**
 * Propiedades del componente de gestión de destinatarios de notificación de actas.
 */
export interface RecipientNotificationModalProps {
  /**
   * Correo corporativo del consultor asignado (destinatario principal inmutable).
   * Proviene del directorio institucional y no puede ser removido por el mensajero en campo
   * para garantizar el deber legal de notificación al custodio del hardware.
   */
  consultorCorreo?: string | null;

  /**
   * Nombre completo del colaborador que recibe o devuelve el activo tecnológico.
   */
  consultorNombre?: string | null;

  /**
   * Callback invocado ante cualquier adición o remoción de direcciones de copia en campo.
   * Notifica a la vista contenedora la lista actualizada de correos secundarios validados.
   */
  onAdditionalEmailsChange: (emails: string[]) => void;

  /**
   * Bloquea la interacción durante el procesamiento de la firma o cuando el formulario está deshabilitado.
   */
  disabled?: boolean;
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Componente interactivo para la pre-carga fija del custodio y la incorporación dinámica de copias en campo.
 *
 * @param props - Propiedades de configuración del componente {@link RecipientNotificationModalProps}.
 */
export function RecipientNotificationModal({
  consultorCorreo,
  consultorNombre,
  onAdditionalEmailsChange,
  disabled = false,
}: RecipientNotificationModalProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [inputValue, setInputValue] = useState("");
  const [emails, setEmails] = useState<string[]>([]);
  const [validationError, setValidationError] = useState<string | null>(null);

  const displayPrimaryEmail = consultorCorreo?.trim() || "correo-no-registrado@assist.com.co";

  function handleAddEmail() {
    const trimmed = inputValue.trim().toLowerCase();
    if (!trimmed) return;

    if (!EMAIL_REGEX.test(trimmed)) {
      setValidationError("El formato de correo ingresado no es válido.");
      return;
    }

    if (trimmed === displayPrimaryEmail.toLowerCase() || emails.includes(trimmed)) {
      setValidationError("Este correo ya se encuentra en la lista de notificación.");
      return;
    }

    const updated = [...emails, trimmed];
    setEmails(updated);
    onAdditionalEmailsChange(updated);
    setInputValue("");
    setValidationError(null);
  }

  function handleKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter" || e.key === ",") {
      e.preventDefault();
      handleAddEmail();
    }
  }

  function handleRemoveEmail(emailToRemove: string) {
    const updated = emails.filter((e) => e !== emailToRemove);
    setEmails(updated);
    onAdditionalEmailsChange(updated);
  }

  return (
    <div
      className="card"
      style={{
        padding: "16px",
        marginBottom: "18px",
        borderRadius: "8px",
        border: "1px solid var(--border-default)",
        backgroundColor: "var(--bg-card)",
      }}
    >
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: "8px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <span style={{ fontSize: "16px" }}>✉️</span>
          <span style={{ fontSize: "13px", fontWeight: 600, color: "var(--text-primary)" }}>
            Notificación Automática del Acta Firmada
          </span>
        </div>
        {!isOpen && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setIsOpen(true)}
            disabled={disabled}
            style={{ fontSize: "12px", color: "var(--color-primary-500)", padding: "4px 8px" }}
          >
            + Agregar copia (opcional)
          </button>
        )}
      </div>

      {/* Correo Principal Informativo */}
      <div
        className="notification-recipient-box recipient-field"
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          padding: "8px 12px",
          backgroundColor: "var(--bg-muted)",
          borderRadius: "6px",
          border: "1px solid var(--border-subtle)",
          fontSize: "12px",
        }}
      >
        <span style={{ color: "var(--text-secondary)" }}>
          Para: <strong style={{ color: "var(--text-primary)" }}>{consultorNombre || "Consultor"}</strong> ({displayPrimaryEmail})
        </span>
        <span
          className="badge"
          style={{
            fontSize: "10.5px",
            backgroundColor: "var(--color-primary-50)",
            color: "var(--color-primary-600)",
            border: "1px solid var(--color-primary-200)",
            padding: "2px 8px",
            fontWeight: 700,
          }}
        >
          Principal
        </span>
      </div>

      {/* Sección Expandible de Copias Adicionales */}
      {isOpen && (
        <div style={{ marginTop: "12px" }}>
          <div style={{ display: "flex", gap: "8px", alignItems: "flex-start" }}>
            <div style={{ flex: 1 }}>
              <input
                type="email"
                className="form-input"
                placeholder="correo-adicional@cliente.com (presione Enter)"
                value={inputValue}
                onChange={(e) => {
                  setInputValue(e.target.value);
                  if (validationError) setValidationError(null);
                }}
                onKeyDown={handleKeyDown}
                disabled={disabled}
                style={{ fontSize: "13px", padding: "7px 10px" }}
              />
              {validationError && (
                <div style={{ color: "var(--color-danger, #dc2626)", fontSize: "11px", marginTop: "4px" }}>
                  {validationError}
                </div>
              )}
            </div>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={handleAddEmail}
              disabled={disabled || !inputValue.trim()}
              style={{ minHeight: "36px", padding: "0 12px", fontSize: "12px" }}
            >
              Añadir
            </button>
          </div>

          {/* Chips de Correos Adicionales */}
          {emails.length > 0 && (
            <div style={{ display: "flex", flexWrap: "wrap", gap: "6px", marginTop: "10px" }}>
              {emails.map((email) => (
                <span
                  key={email}
                  className="email-chip email-tag recipient-badge"
                >
                  <span>{email}</span>
                  {!disabled && (
                    <button
                      type="button"
                      className="email-chip-remove email-tag-remove"
                      onClick={() => handleRemoveEmail(email)}
                      title="Quitar destinatario"
                    >
                      &times;
                    </button>
                  )}
                </span>
              ))}
            </div>
          )}

          <p style={{ fontSize: "11px", color: "var(--text-muted)", marginTop: "6px", marginBottom: 0 }}>
            Una copia certificada con el PDF firmado será remitida a estos correos una vez completada la firma.
          </p>
        </div>
      )}
    </div>
  );
}

// Retrocompatibilidad
export { RecipientNotificationModal as NotificacionDestinatarios };
export type { RecipientNotificationModalProps as NotificacionDestinatariosProps };
