import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../reportesService";

interface ReportChartsProps {
  tab: ReportTab;
  data: EquiposReportItem[] | ActasReportItem[] | EntregasReportItem[] | DevolucionesReportItem[];
}

export function ReportCharts({ tab, data }: ReportChartsProps) {
  if (data.length === 0) return null;

  if (tab === "equipos") {
    const list = data as EquiposReportItem[];
    const counts: Record<string, number> = {};
    list.forEach((item) => {
      const st = item.estatusEquipo || "Sin estado";
      counts[st] = (counts[st] || 0) + 1;
    });

    const maxVal = Math.max(...Object.values(counts), 1);

    return (
      <div className="dashboard-block" style={{ padding: "16px 20px", marginBottom: "20px" }}>
        <h4 style={{ margin: "0 0 12px", fontSize: "14px", fontWeight: 700, color: "#0f172a" }}>
          DistribuciÃ³n de Estados en Resultados
        </h4>
        <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
          {Object.entries(counts).map(([estado, cant]) => {
            const pct = Math.round((cant / list.length) * 100);
            const isActivo = estado.toLowerCase() === "activo";
            const isAsignado = estado.toLowerCase() === "asignado";
            const color = isActivo ? "#10b981" : isAsignado ? "#3b82f6" : "#ef4444";

            return (
              <div key={estado} style={{ display: "flex", flexDirection: "column", gap: "3px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "12px" }}>
                  <span style={{ fontWeight: 600, color: "#334155" }}>{estado}</span>
                  <span style={{ color: "#64748b" }}>
                    <strong>{cant}</strong> ({pct}%)
                  </span>
                </div>
                <div style={{ width: "100%", height: "8px", background: "#f1f5f9", borderRadius: "4px", overflow: "hidden" }}>
                  <div
                    style={{
                      height: "100%",
                      width: `${(cant / maxVal) * 100}%`,
                      backgroundColor: color,
                      borderRadius: "4px",
                      transition: "width 0.3s ease",
                    }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  }

  if (tab === "actas") {
    const list = data as ActasReportItem[];
    const counts: Record<string, number> = {};
    list.forEach((item) => {
      const key = `${item.tipoActa} - ${item.estado}`;
      counts[key] = (counts[key] || 0) + 1;
    });

    const maxVal = Math.max(...Object.values(counts), 1);

    return (
      <div className="dashboard-block" style={{ padding: "16px 20px", marginBottom: "20px" }}>
        <h4 style={{ margin: "0 0 12px", fontSize: "14px", fontWeight: 700, color: "#0f172a" }}>
          Actas por Tipo y Estado
        </h4>
        <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
          {Object.entries(counts).map(([label, cant]) => {
            const pct = Math.round((cant / list.length) * 100);
            const isFirmada = label.includes("Firmada") || label.includes("Finalizada");
            const color = isFirmada ? "#10b981" : label.includes("Anulada") ? "#ef4444" : "#f59e0b";

            return (
              <div key={label} style={{ display: "flex", flexDirection: "column", gap: "3px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "12px" }}>
                  <span style={{ fontWeight: 600, color: "#334155" }}>{label}</span>
                  <span style={{ color: "#64748b" }}>
                    <strong>{cant}</strong> ({pct}%)
                  </span>
                </div>
                <div style={{ width: "100%", height: "8px", background: "#f1f5f9", borderRadius: "4px", overflow: "hidden" }}>
                  <div
                    style={{
                      height: "100%",
                      width: `${(cant / maxVal) * 100}%`,
                      backgroundColor: color,
                      borderRadius: "4px",
                      transition: "width 0.3s ease",
                    }}
                  />
                </div>
              </div>
            );
          })}
        </div>
      </div>
    );
  }

  return null;
}

