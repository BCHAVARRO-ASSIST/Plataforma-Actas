import type { DashboardKpis } from "../dashboardService";

interface ExecutiveMetricsProps {
  kpis: DashboardKpis;
}

export function ExecutiveMetrics({ kpis }: ExecutiveMetricsProps) {
  const equiposOperativosPct =
    kpis.totalEquipos > 0
      ? Math.round((kpis.equiposActivos / kpis.totalEquipos) * 100)
      : 0;

  const actasFirmadasPct =
    kpis.totalActas > 0
      ? Math.round(((kpis.actasFirmadas + kpis.actasFinalizadas) / kpis.totalActas) * 100)
      : 0;

  return (
    <section
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
        gap: "16px",
        marginBottom: "24px",
      }}
    >
      {/* 1. INVENTARIO */}
      <article
        style={{
          background: "#ffffff",
          borderRadius: "14px",
          padding: "20px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          transition: "transform 0.2s ease, box-shadow 0.2s ease",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Total Inventario
          </span>
          <span style={{ width: "32px", height: "32px", borderRadius: "8px", background: "#f1f5f9", display: "flex", alignItems: "center", justifyContent: "center", fontSize: "14px" }}>
            ðŸ’»
          </span>
        </div>

        <div style={{ margin: "12px 0 6px" }}>
          <strong style={{ fontSize: "28px", fontWeight: 800, color: "#0f172a" }}>
            {kpis.totalEquipos}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b", marginLeft: "6px" }}>
            equipos registrados
          </span>
        </div>

        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", paddingTop: "10px", borderTop: "1px solid #f1f5f9", fontSize: "12px" }}>
          <span style={{ color: "#059669", fontWeight: 600 }}>
            â— {kpis.equiposActivos} activos ({equiposOperativosPct}%)
          </span>
          <span style={{ color: "#64748b" }}>
            {kpis.equiposInactivos} inactivos
          </span>
        </div>
      </article>

      {/* 2. ACTAS Y TRAZABILIDAD */}
      <article
        style={{
          background: "#ffffff",
          borderRadius: "14px",
          padding: "20px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          transition: "transform 0.2s ease, box-shadow 0.2s ease",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Actas Procesadas
          </span>
          <span style={{ width: "32px", height: "32px", borderRadius: "8px", background: "#ecfdf5", display: "flex", alignItems: "center", justifyContent: "center", fontSize: "14px", color: "#059669" }}>
            ✓
          </span>
        </div>

        <div style={{ margin: "12px 0 6px" }}>
          <strong style={{ fontSize: "28px", fontWeight: 800, color: "#0f172a" }}>
            {kpis.totalActas}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b", marginLeft: "6px" }}>
            documentos emitidos
          </span>
        </div>

        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", paddingTop: "10px", borderTop: "1px solid #f1f5f9", fontSize: "12px" }}>
          <span style={{ color: "#059669", fontWeight: 600 }}>
            {kpis.actasFirmadas + kpis.actasFinalizadas} firmadas ({actasFirmadasPct}%)
          </span>
          <span style={{ color: kpis.actasBorrador > 0 ? "#d97706" : "#64748b", fontWeight: kpis.actasBorrador > 0 ? 700 : 400 }}>
            {kpis.actasBorrador} pendientes
          </span>
        </div>
      </article>

      {/* 3. LOGÍSTICA DE CAMPO */}
      <article
        style={{
          background: "#ffffff",
          borderRadius: "14px",
          padding: "20px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          transition: "transform 0.2s ease, box-shadow 0.2s ease",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Operación de Campo
          </span>
          <span style={{ width: "32px", height: "32px", borderRadius: "8px", background: "#eff6ff", display: "flex", alignItems: "center", justifyContent: "center", fontSize: "14px", color: "#2563eb" }}>
            ðŸ“¦
          </span>
        </div>

        <div style={{ margin: "12px 0 6px" }}>
          <strong style={{ fontSize: "28px", fontWeight: 800, color: "#0f172a" }}>
            {kpis.entregasPendientes + kpis.devolucionesPendientes}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b", marginLeft: "6px" }}>
            trámites pendientes
          </span>
        </div>

        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", paddingTop: "10px", borderTop: "1px solid #f1f5f9", fontSize: "12px" }}>
          <span style={{ color: "#2563eb" }}>
            ðŸ“¦ {kpis.entregasPendientes} entregas
          </span>
          <span style={{ color: "#6366f1" }}>
            â†© {kpis.devolucionesPendientes} devoluciones
          </span>
        </div>
      </article>

      {/* 4. MANTENIMIENTO */}
      <article
        style={{
          background: "#ffffff",
          borderRadius: "14px",
          padding: "20px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          transition: "transform 0.2s ease, box-shadow 0.2s ease",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <span style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Salud de Equipos
          </span>
          <span style={{ width: "32px", height: "32px", borderRadius: "8px", background: "#fef3c7", display: "flex", alignItems: "center", justifyContent: "center", fontSize: "14px", color: "#d97706" }}>
            âš™
          </span>
        </div>

        <div style={{ margin: "12px 0 6px" }}>
          <strong style={{ fontSize: "28px", fontWeight: 800, color: "#0f172a" }}>
            {kpis.equiposActivos}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b", marginLeft: "6px" }}>
            equipos operativos
          </span>
        </div>

        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", paddingTop: "10px", borderTop: "1px solid #f1f5f9", fontSize: "12px" }}>
          <span style={{ color: "#059669", fontWeight: 600 }}>
            ✓ Mantenimientos al día
          </span>
          <span style={{ color: "#64748b" }}>
            {kpis.equiposMantenimiento} en revisión
          </span>
        </div>
      </article>
    </section>
  );
}

