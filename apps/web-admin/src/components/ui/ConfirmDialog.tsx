import React, { useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import { AlertCircleIcon, AlertTriangleIcon, InfoIcon, CloseIcon } from "./Icons";

export type ConfirmVariant = "danger" | "warning" | "info";

export interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void | Promise<void>;
  title: string;
  message: React.ReactNode;
  confirmText?: string;
  cancelText?: string;
  variant?: ConfirmVariant;
  loading?: boolean;
}

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = "Confirmar",
  cancelText = "Cancelar",
  variant = "danger",
  loading = false,
}) => {
  const cancelButtonRef = useRef<HTMLButtonElement>(null);
  const confirmButtonRef = useRef<HTMLButtonElement>(null);

  // Cierre accesible con tecla Escape y gestión de foco
  useEffect(() => {
    if (!isOpen) return;

    // Foco automático al botón más seguro (cancelar en caso destructivo, confirmar en informativo)
    const timer = setTimeout(() => {
      if (variant === "danger") {
        cancelButtonRef.current?.focus();
      } else {
        confirmButtonRef.current?.focus();
      }
    }, 50);

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !loading) {
        e.stopPropagation();
        onClose();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => {
      clearTimeout(timer);
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen, loading, onClose, variant]);

  if (!isOpen) return null;

  const renderIcon = () => {
    switch (variant) {
      case "danger":
        return <AlertCircleIcon size={24} />;
      case "warning":
        return <AlertTriangleIcon size={24} />;
      case "info":
      default:
        return <InfoIcon size={24} />;
    }
  };

  const dialogElement = (
    <div
      className="modal-backdrop modal-overlay confirm-dialog-backdrop"
      onClick={!loading ? onClose : undefined}
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="confirm-dialog-title"
      aria-describedby="confirm-dialog-description"
    >
      <div
        className={`confirm-dialog-card confirm-dialog-${variant}`}
        onClick={(e) => e.stopPropagation()}
      >
        <button
          type="button"
          className="modal-close confirm-dialog-close"
          onClick={!loading ? onClose : undefined}
          aria-label="Cerrar confirmación"
          disabled={loading}
          style={{ position: "absolute", top: "16px", right: "16px" }}
        >
          <CloseIcon size={16} />
        </button>
        <div className="confirm-dialog-body">
          <div className={`confirm-dialog-icon-wrapper confirm-icon-${variant}`}>
            {renderIcon()}
          </div>
          <div className="confirm-dialog-content">
            <h3 id="confirm-dialog-title" className="confirm-dialog-title">
              {title}
            </h3>
            <div id="confirm-dialog-description" className="confirm-dialog-message">
              {typeof message === "string" ? (
                <p style={{ margin: 0, whiteSpace: "pre-line" }}>{message}</p>
              ) : (
                message
              )}
            </div>
          </div>
        </div>

        <div className="confirm-dialog-footer">
          <button
            ref={cancelButtonRef}
            type="button"
            className="btn btn-secondary confirm-btn-cancel"
            onClick={onClose}
            disabled={loading}
          >
            {cancelText}
          </button>
          <button
            ref={confirmButtonRef}
            type="button"
            className={`btn confirm-btn-submit confirm-btn-${variant}`}
            onClick={onConfirm}
            disabled={loading}
          >
            {loading && (
              <span
                style={{
                  display: "inline-block",
                  width: "14px",
                  height: "14px",
                  border: "2px solid currentColor",
                  borderTopColor: "transparent",
                  borderRadius: "50%",
                  animation: "spin 0.7s linear infinite",
                  marginRight: "6px",
                }}
              />
            )}
            {loading ? "Procesando..." : confirmText}
          </button>
        </div>
      </div>
    </div>
  );

  return typeof document !== "undefined"
    ? createPortal(dialogElement, document.body)
    : dialogElement;
};
