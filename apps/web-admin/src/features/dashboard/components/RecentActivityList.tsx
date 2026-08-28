import type { RecentActivityItem } from "../dashboardService";

interface RecentActivityListProps {
  items: RecentActivityItem[];
}

function formatDate(val: string): string {
  try {
    return new Date(val).toLocaleString("es-CO", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return val;
  }
}

export function RecentActivityList({ items }: RecentActivityListProps) {
  if (items.length === 0) {
    return (
      <div className="dashboard-block" style={{ padding: "20px" }}>
        <div className="dashboard-block-top">
          <div>
            <span className="block-kicker">AUDITORÍA Y TRAZABILIDAD</span>
            <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
              Actividad Reciente del Sistema
            </h3>
          </div>
        </div>
        <div style={{ padding: "40px 20px", textAlign: "center", color: "#64748b" }}>
          No se registra actividad reciente.
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-block" style={{ padding: "20px" }}>
      <div className="dashboard-block-top" style={{ marginBottom: "16px" }}>
        <div>
          <span className="block-kicker">AUDITORÍA Y TRAZABILIDAD</span>
          <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            Actividad Reciente del Sistema
          </h3>
        </div>
        <span style={{ fontSize: "12px", color: "#64748b", fontWeight: 600 }}>
          Últimos eventos
        </span>
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
        {items.map((item) => {
          const isFirmada = item.estado.toLowerCase() === "firmada";
          const isFinalizada = item.estado.toLowerCase() === "finalizada";
          const isAnulada = item.estado.toLowerCase() === "anulada";

          return (
            <div
              key={item.id}
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "flex-start",
                gap: "12px",
                padding: "10px 14px",
                background: "#f8fafc",
                borderRadius: "8px",
                borderLeft: `4px solid ${
                  isFirmada ? "#10b981" : isFinalizada ? "#3b82f6" : isAnulada ? "#ef4444" : "#f59e0b"
                }`,
              }}
            >
              <div>
                <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                  <strong style={{ fontSize: "13px", color: "#0f172a" }}>{item.titulo}</strong>
                  <span
                    style={{
                      fontSize: "11px",
                      padding: "2px 6px",
                      borderRadius: "4px",
                      background: "#e2e8f0",
                      color: "#475569",
                      fontWeight: 600,
                    }}
                  >
                    {item.tipoEvento}
                  </span>
                </div>
                <p style={{ margin: "4px 0 0", fontSize: "12px", color: "#475569" }}>
                  {item.descripcion} · Responsable: <strong>{item.responsable || "Sistema"}</strong>
                </p>
              </div>

              <div style={{ textAlign: "right", whiteSpace: "nowrap" }}>
                <span style={{ fontSize: "11px", color: "#94a3b8" }}>{formatDate(item.fecha)}</span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

