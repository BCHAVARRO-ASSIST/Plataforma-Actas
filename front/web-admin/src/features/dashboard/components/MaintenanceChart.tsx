import type { MaintenanceStats } from "../types/dashboard";

interface MaintenanceChartProps {
  stats: MaintenanceStats | null;
}

export function MaintenanceChart({ stats }: MaintenanceChartProps) {
  if (!stats || stats.totalMantenimientos === 0) {
    return (
      <div className="dashboard-block" style={{ padding: "20px" }}>
        <div className="dashboard-block-top">
          <div>
            <span className="block-kicker">SOPORTE Y SERVICIOS</span>
            <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
              Mantenimiento de Equipos
            </h3>
          </div>
        </div>
        <div style={{ padding: "30px 20px", textAlign: "center", color: "#64748b" }}>
          <p style={{ margin: "0 0 6px", fontWeight: 600 }}>Sin mantenimientos programados</p>
          <small style={{ color: "#94a3b8" }}>
            Los datos se actualizarán automáticamente cuando se registren mantenimientos en los equipos.
          </small>
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-block" style={{ padding: "20px" }}>
      <div className="dashboard-block-top" style={{ marginBottom: "16px" }}>
        <div>
          <span className="block-kicker">SOPORTE Y SERVICIOS</span>
          <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            Mantenimiento de Equipos
          </h3>
        </div>
        <span style={{ fontSize: "12px", color: "#64748b", fontWeight: 600 }}>
          {stats.totalMantenimientos} registros
        </span>
      </div>

      {/* MINI METRICS */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr", gap: "10px", marginBottom: "16px" }}>
        <div style={{ padding: "10px", background: "#f8fafc", borderRadius: "8px", textAlign: "center" }}>
          <span style={{ fontSize: "11px", color: "#64748b", fontWeight: 600 }}>PREVENTIVOS</span>
          <strong style={{ display: "block", fontSize: "16px", color: "#059669", marginTop: "2px" }}>
            {stats.preventivos}
          </strong>
        </div>

        <div style={{ padding: "10px", background: "#f8fafc", borderRadius: "8px", textAlign: "center" }}>
          <span style={{ fontSize: "11px", color: "#64748b", fontWeight: 600 }}>CORRECTIVOS</span>
          <strong style={{ display: "block", fontSize: "16px", color: "#d97706", marginTop: "2px" }}>
            {stats.correctivos}
          </strong>
        </div>

        <div style={{ padding: "10px", background: "#f8fafc", borderRadius: "8px", textAlign: "center" }}>
          <span style={{ fontSize: "11px", color: "#64748b", fontWeight: 600 }}>CONCLUIDOS</span>
          <strong style={{ display: "block", fontSize: "16px", color: "#2563eb", marginTop: "2px" }}>
            {stats.concluidos}
          </strong>
        </div>

        <div style={{ padding: "10px", background: "#f8fafc", borderRadius: "8px", textAlign: "center" }}>
          <span style={{ fontSize: "11px", color: "#64748b", fontWeight: 600 }}>PENDIENTES</span>
          <strong style={{ display: "block", fontSize: "16px", color: "#e11d48", marginTop: "2px" }}>
            {stats.pendientes}
          </strong>
        </div>
      </div>

      {/* DESGLOSE POR TIPO */}
      <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
        {stats.mantenimientosPorTipo.map((item, idx) => (
          <div key={idx} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", fontSize: "12px" }}>
            <span style={{ color: "#334155", fontWeight: 600 }}>• {item.estado}</span>
            <span style={{ color: "#64748b" }}>
              <strong>{item.cantidad}</strong> ({item.porcentaje}%)
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
