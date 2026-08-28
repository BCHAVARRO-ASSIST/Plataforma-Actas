import type { RecentActivityItem } from "../dashboardService";

interface RecentActasCardProps {
  items: RecentActivityItem[];
  onNavigateToActas: () => void;
}

function formatDate(val: string): string {
  try {
    return new Date(val).toLocaleDateString("es-CO", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return val;
  }
}

export function RecentActasCard({ items, onNavigateToActas }: RecentActasCardProps) {
  return (
    <div
      style={{
        background: "#ffffff",
        borderRadius: "12px",
        border: "1px solid #e2e8f0",
        boxShadow: "0 1px 3px rgba(0,0,0,0.03)",
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
      }}
    >
      {/* HEADER */}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          padding: "16px 20px",
          borderBottom: "1px solid #f1f5f9",
        }}
      >
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            TRAZABILIDAD Y REGISTRO
          </span>
          <h2 style={{ margin: "2px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            Ãšltimas Actas Emitidas
          </h2>
        </div>

        <button
          type="button"
          onClick={onNavigateToActas}
          style={{
            background: "none",
            border: "none",
            color: "#3b82f6",
            fontSize: "13px",
            fontWeight: 600,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: "4px",
          }}
        >
          Ver todas las actas â†’
        </button>
      </div>

      {/* LISTA / TABLA DE ACTAS RECIENTES */}
      {items.length === 0 ? (
        <div style={{ padding: "36px 20px", textAlign: "center", color: "#64748b" }}>
          No hay actas registradas recientemente.
        </div>
      ) : (
        <div style={{ overflowX: "auto" }}>
          <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "13px" }}>
            <thead>
              <tr style={{ background: "#f8fafc", borderBottom: "1px solid #e2e8f0" }}>
                <th style={{ textAlign: "left", padding: "10px 18px", fontSize: "11px", fontWeight: 700, color: "#475569", textTransform: "uppercase" }}>
                  Documento
                </th>
                <th style={{ textAlign: "left", padding: "10px 14px", fontSize: "11px", fontWeight: 700, color: "#475569", textTransform: "uppercase" }}>
                  Detalle Hardware
                </th>
                <th style={{ textAlign: "left", padding: "10px 14px", fontSize: "11px", fontWeight: 700, color: "#475569", textTransform: "uppercase" }}>
                  Responsable
                </th>
                <th style={{ textAlign: "left", padding: "10px 14px", fontSize: "11px", fontWeight: 700, color: "#475569", textTransform: "uppercase" }}>
                  Fecha
                </th>
                <th style={{ textAlign: "right", padding: "10px 18px", fontSize: "11px", fontWeight: 700, color: "#475569", textTransform: "uppercase" }}>
                  Estado
                </th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => {
                const isFirmada = item.estado.toLowerCase() === "firmada";
                const isFinalizada = item.estado.toLowerCase() === "finalizada";
                const isAnulada = item.estado.toLowerCase() === "anulada";

                return (
                  <tr
                    key={item.id}
                    style={{
                      borderBottom: "1px solid #f1f5f9",
                      transition: "background 0.1s ease",
                    }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = "#f8fafc")}
                    onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
                  >
                    <td style={{ padding: "12px 18px" }}>
                      <strong style={{ color: "#0f172a", display: "block" }}>{item.titulo}</strong>
                      <span style={{ fontSize: "11px", color: "#64748b" }}>{item.tipoEvento}</span>
                    </td>

                    <td style={{ padding: "12px 14px", color: "#334155" }}>
                      {item.descripcion}
                    </td>

                    <td style={{ padding: "12px 14px", color: "#334155", fontWeight: 500 }}>
                      ðŸ‘¤ {item.responsable || "Sistema"}
                    </td>

                    <td style={{ padding: "12px 14px", color: "#64748b", fontSize: "12px", whiteSpace: "nowrap" }}>
                      {formatDate(item.fecha)}
                    </td>

                    <td style={{ padding: "12px 18px", textAlign: "right" }}>
                      <span
                        style={{
                          display: "inline-block",
                          padding: "3px 8px",
                          borderRadius: "6px",
                          fontSize: "12px",
                          fontWeight: 700,
                          backgroundColor: isFirmada || isFinalizada ? "#ecfdf5" : isAnulada ? "#fef2f2" : "#fef3c7",
                          color: isFirmada || isFinalizada ? "#059669" : isAnulada ? "#dc2626" : "#d97706",
                        }}
                      >
                        {item.estado}
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

