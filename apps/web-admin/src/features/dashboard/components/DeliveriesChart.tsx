import type { MessengerDeliveryItem } from "../dashboardService";

interface DeliveriesChartProps {
  data: MessengerDeliveryItem[];
}

export function DeliveriesChart({ data }: DeliveriesChartProps) {
  if (data.length === 0) {
    return (
      <div className="dashboard-block" style={{ padding: "20px" }}>
        <div className="dashboard-block-top">
          <div>
            <span className="block-kicker">CAMPO Y LOGÃSTICA</span>
            <h3>Entregas por Mensajero</h3>
          </div>
        </div>
        <div style={{ padding: "40px 20px", textAlign: "center", color: "#64748b" }}>
          No existen actas asignadas a mensajeros para mostrar.
        </div>
      </div>
    );
  }

  const maxDeliveries = Math.max(...data.map((d) => d.totalAsignadas), 1);

  return (
    <div className="dashboard-block" style={{ padding: "20px" }}>
      <div className="dashboard-block-top" style={{ marginBottom: "16px" }}>
        <div>
          <span className="block-kicker">CAMPO Y LOGÃSTICA</span>
          <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            Rendimiento y Entregas por Mensajero
          </h3>
        </div>
        <div style={{ display: "flex", gap: "12px", fontSize: "12px", color: "#64748b" }}>
          <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
            <span style={{ width: "8px", height: "8px", borderRadius: "2px", background: "#10b981" }} /> Entregadas
          </span>
          <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
            <span style={{ width: "8px", height: "8px", borderRadius: "2px", background: "#f59e0b" }} /> Pendientes
          </span>
        </div>
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
        {data.map((item, idx) => {
          const entregadasPct = (item.totalEntregadas / maxDeliveries) * 100;
          const pendientesPct = (item.totalPendientes / maxDeliveries) * 100;

          return (
            <div key={idx} style={{ display: "flex", flexDirection: "column", gap: "6px" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", fontSize: "13px" }}>
                <span style={{ fontWeight: 600, color: "#1e293b" }}>
                  ðŸ‘¤ {item.mensajeroNombre}
                </span>
                <span style={{ fontSize: "12px", color: "#475569" }}>
                  <strong>{item.totalEntregadas}</strong> firmadas de <strong>{item.totalAsignadas}</strong> asignadas
                </span>
              </div>

              {/* STACKED BAR */}
              <div
                style={{
                  height: "12px",
                  width: "100%",
                  backgroundColor: "#f1f5f9",
                  borderRadius: "6px",
                  overflow: "hidden",
                  display: "flex",
                }}
              >
                <div
                  style={{
                    height: "100%",
                    width: `${entregadasPct}%`,
                    backgroundColor: "#10b981",
                    transition: "width 0.4s ease",
                  }}
                  title={`${item.totalEntregadas} entregadas`}
                />
                <div
                  style={{
                    height: "100%",
                    width: `${pendientesPct}%`,
                    backgroundColor: "#f59e0b",
                    transition: "width 0.4s ease",
                  }}
                  title={`${item.totalPendientes} pendientes`}
                />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

