import React from "react";

export type StatusBadgeVariant =
  | "success"
  | "warning"
  | "danger"
  | "info"
  | "neutral";

export interface StatusBadgeProps {
  status: string;
  label?: string;
  variant?: StatusBadgeVariant;
  showDot?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export function resolveStatusVariant(status: string): StatusBadgeVariant {
  const s = status.trim().toLowerCase();
  switch (s) {
    case "activo":
    case "firmada":
    case "finalizada":
    case "entregada":
    case "completado":
      return "success";

    case "asignado":
    case "en uso":
      return "info";

    case "borrador":
    case "mantenimiento":
    case "en mantenimiento":
    case "pendiente":
      return "warning";

    case "inactivo":
    case "baja":
    case "anulada":
    case "error":
      return "danger";

    default:
      return "neutral";
  }
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({
  status,
  label,
  variant,
  showDot = true,
  className = "",
  style,
}) => {
  const resolvedVariant = variant ?? resolveStatusVariant(status);
  const text = label ?? status;

  return (
    <span
      className={`badge badge-${resolvedVariant} ${className}`.trim()}
      style={style}
    >
      {showDot && <span className="badge-dot" />}
      {text}
    </span>
  );
};
