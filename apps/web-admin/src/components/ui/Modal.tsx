import React, { useEffect } from "react";

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  children: React.ReactNode;
  size?: "small" | "medium" | "large" | "custom";
  closeOnEsc?: boolean;
  closeOnBackdrop?: boolean;
  ariaLabel?: string;
  className?: string;
  contentClassName?: string;
  contentStyle?: React.CSSProperties;
  backdropStyle?: React.CSSProperties;
}

export const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  children,
  size = "medium",
  closeOnEsc = true,
  closeOnBackdrop = true,
  ariaLabel,
  className = "",
  contentClassName = "",
  contentStyle,
  backdropStyle,
}) => {
  useEffect(() => {
    if (!isOpen || !closeOnEsc) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, closeOnEsc, onClose]);

  if (!isOpen) return null;

  const sizeClass = size === "large" ? "large" : size === "small" ? "small" : "";
  const finalContentClass =
    size === "custom"
      ? contentClassName
      : `modal-content ${sizeClass} ${contentClassName}`.trim();

  return (
    <div
      className={`modal-backdrop ${className}`.trim()}
      style={backdropStyle}
      onClick={closeOnBackdrop ? onClose : undefined}
      role="dialog"
      aria-modal="true"
      aria-label={ariaLabel}
    >
      <div
        className={finalContentClass}
        style={contentStyle}
        onClick={(e) => e.stopPropagation()}
      >
        {children}
      </div>
    </div>
  );
};
