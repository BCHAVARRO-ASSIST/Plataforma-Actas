import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../reportsService";

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
      <div className="card" style={{ padding: "20px", marginBottom: "20px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
              ANALÍTICA DE DISTRIBUCIÓN
            </span>
            <h3 style={{ margin: "2px 0 0", fontSize: "15px", fontWeight: 700, color: "var(--text-primary)" }}>
              Distribución de Estados en Resultados
            </h3>
          </div>
          <span style={{ fontSize: "12px", color: "var(--text-muted)", fontWeight: 500 }}>
            {list.length} {list.length === 1 ? "activo analizado" : "activos analizados"}
          </span>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
          {Object.entries(counts).map(([estado, cant]) => {
            const pct = Math.round((cant / list.length) * 100);
            const isActivo = estado.toLowerCase() === "activo";
            const isAsignado = estado.toLowerCase() === "asignado";
            const isMantenimiento = estado.toLowerCase() === "mantenimiento";
            
            const colorClass = isActivo 
              ? "linear-gradient(90deg, #10b981, #059669)" 
              : isAsignado 
              ? "linear-gradient(90deg, #6366f1, #4f46e5)" 
              : isMantenimiento 
              ? "linear-gradient(90deg, #f59e0b, #d97706)" 
              : "linear-gradient(90deg, #94a3b8, #64748b)";

            return (
              <div key={estado} style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "12.5px" }}>
                  <span style={{ fontWeight: 600, color: "var(--text-primary)" }}>{estado}</span>
                  <span style={{ color: "var(--text-muted)", fontWeight: 600 }}>
                    <strong style={{ color: "var(--text-primary)" }}>{cant}</strong> ({pct}%)
                  </span>
                </div>
                <div style={{ width: "100%", height: "8px", background: "var(--color-slate-100)", borderRadius: "var(--radius-pill)", overflow: "hidden" }}>
                  <div
                    style={{
                      height: "100%",
                      width: `${(cant / maxVal) * 100}%`,
                      background: colorClass,
                      borderRadius: "var(--radius-pill)",
                      transition: "width 0.4s ease",
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
      const key = `${item.tipoActa} · ${item.estado}`;
      counts[key] = (counts[key] || 0) + 1;
    });

    const maxVal = Math.max(...Object.values(counts), 1);

    return (
      <div className="card" style={{ padding: "20px", marginBottom: "20px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
              ANALÍTICA DOCUMENTAL
            </span>
            <h3 style={{ margin: "2px 0 0", fontSize: "15px", fontWeight: 700, color: "var(--text-primary)" }}>
              Actas por Tipo y Estado
            </h3>
          </div>
          <span style={{ fontSize: "12px", color: "var(--text-muted)", fontWeight: 500 }}>
            {list.length} {list.length === 1 ? "documento evaluado" : "documentos evaluados"}
          </span>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
          {Object.entries(counts).map(([label, cant]) => {
            const pct = Math.round((cant / list.length) * 100);
            const isFirmada = label.includes("Firmada") || label.includes("Finalizada");
            const isAnulada = label.includes("Anulada");
            
            const colorClass = isFirmada 
              ? "linear-gradient(90deg, #10b981, #059669)" 
              : isAnulada 
              ? "linear-gradient(90deg, #ef4444, #dc2626)" 
              : "linear-gradient(90deg, #f59e0b, #d97706)";

            return (
              <div key={label} style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "12.5px" }}>
                  <span style={{ fontWeight: 600, color: "var(--text-primary)" }}>{label}</span>
                  <span style={{ color: "var(--text-muted)", fontWeight: 600 }}>
                    <strong style={{ color: "var(--text-primary)" }}>{cant}</strong> ({pct}%)
                  </span>
                </div>
                <div style={{ width: "100%", height: "8px", background: "var(--color-slate-100)", borderRadius: "var(--radius-pill)", overflow: "hidden" }}>
                  <div
                    style={{
                      height: "100%",
                      width: `${(cant / maxVal) * 100}%`,
                      background: colorClass,
                      borderRadius: "var(--radius-pill)",
                      transition: "width 0.4s ease",
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
