import { useEffect } from "react";
import {
  CheckCircleIcon,
  AlertCircleIcon,
  AlertTriangleIcon,
  InfoIcon,
  CloseIcon,
} from "./Icons";

export interface AutoDismissAlertProps {
  type: "success" | "error" | "info" | "warning";
  message: string | null;
  duration?: number;
  onDismiss?: () => void;
}

const typeStyles: Record<
  AutoDismissAlertProps["type"],
  {
    bg: string;
    border: string;
    text: string;
    iconColor: string;
    Icon: typeof CheckCircleIcon;
  }
> = {
  success: {
    bg: "var(--color-success-50, #ecfdf5)",
    border: "var(--color-success-100, #d1fae5)",
    text: "var(--color-success-700, #047857)",
    iconColor: "var(--color-success-600, #059669)",
    Icon: CheckCircleIcon,
  },
  error: {
    bg: "var(--color-danger-50, #fff1f2)",
    border: "var(--color-danger-100, #ffe4e6)",
    text: "var(--color-danger-700, #be123c)",
    iconColor: "var(--color-danger-600, #e11d48)",
    Icon: AlertCircleIcon,
  },
  warning: {
    bg: "var(--color-warning-50, #fffbeb)",
    border: "var(--color-warning-100, #fef3c7)",
    text: "var(--color-warning-700, #b45309)",
    iconColor: "var(--color-warning-600, #d97706)",
    Icon: AlertTriangleIcon,
  },
  info: {
    bg: "var(--color-info-50, #f0f9ff)",
    border: "var(--color-info-100, #e0f2fe)",
    text: "var(--color-info-700, #0369a1)",
    iconColor: "var(--color-info-600, #0284c7)",
    Icon: InfoIcon,
  },
};

export function AutoDismissAlert({
  type,
  message,
  duration = 5000,
  onDismiss,
}: AutoDismissAlertProps) {
  useEffect(() => {
    if (!message || !onDismiss || duration <= 0) return;

    const timer = setTimeout(() => {
      onDismiss();
    }, duration);

    return () => clearTimeout(timer);
  }, [message, duration, onDismiss]);

  if (!message) return null;

  const style = typeStyles[type] || typeStyles.info;
  const { Icon } = style;

  return (
    <div
      role="alert"
      aria-live={type === "error" ? "assertive" : "polite"}
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "12px",
        padding: "12px 16px",
        marginBottom: "16px",
        borderRadius: "var(--radius-md, 8px)",
        background: style.bg,
        border: `1px solid ${style.border}`,
        color: style.text,
        fontSize: "13.5px",
        fontWeight: 500,
        lineHeight: 1.4,
        boxShadow: "0 1px 3px rgba(0, 0, 0, 0.04)",
        animation: "fadeInAlert 0.25s ease-out both",
      }}
    >
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "10px",
          flex: 1,
          minWidth: 0,
        }}
      >
        <span
          style={{
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
            color: style.iconColor,
            flexShrink: 0,
          }}
        >
          <Icon size={18} />
        </span>
        <span
          style={{
            wordBreak: "break-word",
            color: style.text,
          }}
        >
          {message}
        </span>
      </div>

      {onDismiss && (
        <button
          type="button"
          onClick={onDismiss}
          aria-label="Cerrar notificación"
          style={{
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
            width: "24px",
            height: "24px",
            padding: 0,
            background: "transparent",
            border: "none",
            borderRadius: "var(--radius-sm, 6px)",
            color: style.text,
            opacity: 0.75,
            cursor: "pointer",
            flexShrink: 0,
            transition: "opacity 0.15s ease, background 0.15s ease",
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.opacity = "1";
            e.currentTarget.style.background = "rgba(0, 0, 0, 0.05)";
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.opacity = "0.75";
            e.currentTarget.style.background = "transparent";
          }}
        >
          <CloseIcon size={14} />
        </button>
      )}
    </div>
  );
}
