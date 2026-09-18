import React from "react";

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalItems?: number;
  currentCount?: number;
  itemName?: string;
  onPageChange: (page: number) => void;
  className?: string;
  style?: React.CSSProperties;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  totalItems,
  currentCount,
  itemName = "elementos",
  onPageChange,
  className = "",
  style,
}) => {
  if (totalPages <= 1) return null;

  return (
    <div
      className={className}
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        marginTop: "16px",
        padding: "8px 0",
        ...style,
      }}
    >
      <span style={{ fontSize: "12.5px", color: "var(--text-muted)" }}>
        {totalItems !== undefined
          ? currentCount !== undefined
            ? `Mostrando ${currentCount} de ${totalItems} ${itemName}`
            : `Total: ${totalItems} ${itemName}`
          : `Página ${currentPage} de ${totalPages}`}
      </span>

      <div style={{ display: "flex", gap: "6px" }}>
        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={() => onPageChange(Math.max(1, currentPage - 1))}
          disabled={currentPage === 1}
        >
          ← Anterior
        </button>
        <span
          style={{
            padding: "6px 12px",
            fontSize: "12.5px",
            fontWeight: 600,
            color: "var(--text-secondary)",
          }}
        >
          Página {currentPage} de {totalPages}
        </span>
        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
          disabled={currentPage === totalPages}
        >
          Siguiente →
        </button>
      </div>
    </div>
  );
};
