import type { DashboardKpis } from "../dashboardService";

interface ExecutiveSummaryCardsProps {
  kpis: DashboardKpis;
  onNavigate: (view: "equipment" | "actas" | "reportes") => void;
}

export function ExecutiveSummaryCards({ kpis, onNavigate }: ExecutiveSummaryCardsProps) {
  const activosPct =
    kpis.totalEquipos > 0
      ? Math.round((kpis.equiposActivos / kpis.totalEquipos) * 100)
      : 0;

  const firmadasPct =
    kpis.totalActas > 0
      ? Math.round(((kpis.actasFirmadas + kpis.actasFinalizadas) / kpis.totalActas) * 100)
      : 100;

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
        gap: "16px",
        marginBottom: "24px",
      }}
    >
      {/* 1. INVENTARIO */}
      <div
        onClick={() => onNavigate("equipment")}
        style={{
          background: "#ffffff",
          borderRadius: "12px",
          padding: "20px 22px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.03)",
          cursor: "pointer",
          transition: "transform 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = "#94a3b8";
          e.currentTarget.style.transform = "translateY(-2px)";
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = "#e2e8f0";
          e.currentTarget.style.transform = "translateY(0)";
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Equipos en Inventario
          </span>
          <span style={{ fontSize: "12px", color: "#3b82f6", fontWeight: 600 }}>Ver catÃ¡logo â†’</span>
        </div>

        <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "4px 0 10px" }}>
          <strong style={{ fontSize: "32px", fontWeight: 800, color: "#0f172a", lineHeight: 1 }}>
            {kpis.totalEquipos}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b" }}>activos de hardware</span>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "12px", fontSize: "12px", color: "#475569", paddingTop: "8px", borderTop: "1px solid #f8fafc" }}>
          <span style={{ color: "#059669", fontWeight: 600, display: "flex", alignItems: "center", gap: "4px" }}>
            <span style={{ width: "6px", height: "6px", borderRadius: "50%", background: "#059669" }} />
            {kpis.equiposActivos} operativos ({activosPct}%)
          </span>
          <span style={{ color: "#94a3b8" }}>â€¢</span>
          <span>{kpis.equiposInactivos} inactivos</span>
        </div>
      </div>

      {/* 2. ACTAS */}
      <div
        onClick={() => onNavigate("actas")}
        style={{
          background: "#ffffff",
          borderRadius: "12px",
          padding: "20px 22px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.03)",
          cursor: "pointer",
          transition: "transform 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = "#94a3b8";
          e.currentTarget.style.transform = "translateY(-2px)";
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = "#e2e8f0";
          e.currentTarget.style.transform = "translateY(0)";
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            GestiÃ³n de Actas
          </span>
          <span style={{ fontSize: "12px", color: "#10b981", fontWeight: 600 }}>Supervisar â†’</span>
        </div>

        <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "4px 0 10px" }}>
          <strong style={{ fontSize: "32px", fontWeight: 800, color: "#0f172a", lineHeight: 1 }}>
            {kpis.totalActas}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b" }}>documentos emitidos</span>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "12px", fontSize: "12px", color: "#475569", paddingTop: "8px", borderTop: "1px solid #f8fafc" }}>
          <span style={{ color: "#059669", fontWeight: 600, display: "flex", alignItems: "center", gap: "4px" }}>
            <span style={{ width: "6px", height: "6px", borderRadius: "50%", background: "#059669" }} />
            {kpis.actasFirmadas + kpis.actasFinalizadas} firmadas ({firmadasPct}%)
          </span>
          <span style={{ color: "#94a3b8" }}>â€¢</span>
          <span style={{ color: kpis.actasBorrador > 0 ? "#d97706" : "#64748b", fontWeight: kpis.actasBorrador > 0 ? 700 : 400 }}>
            {kpis.actasBorrador} pendientes
          </span>
        </div>
      </div>

      {/* 3. LOGÃSTICA */}
      <div
        onClick={() => onNavigate("reportes")}
        style={{
          background: "#ffffff",
          borderRadius: "12px",
          padding: "20px 22px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.03)",
          cursor: "pointer",
          transition: "transform 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = "#94a3b8";
          e.currentTarget.style.transform = "translateY(-2px)";
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = "#e2e8f0";
          e.currentTarget.style.transform = "translateY(0)";
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            OperaciÃ³n y LogÃ­stica
          </span>
          <span style={{ fontSize: "12px", color: "#6366f1", fontWeight: 600 }}>Ver informes â†’</span>
        </div>

        <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "4px 0 10px" }}>
          <strong style={{ fontSize: "32px", fontWeight: 800, color: "#0f172a", lineHeight: 1 }}>
            {kpis.entregasPendientes + kpis.devolucionesPendientes}
          </strong>
          <span style={{ fontSize: "13px", color: "#64748b" }}>trÃ¡mites pendientes</span>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "12px", fontSize: "12px", color: "#475569", paddingTop: "8px", borderTop: "1px solid #f8fafc" }}>
          <span style={{ color: "#2563eb", fontWeight: 600 }}>
            ðŸ“¦ {kpis.entregasPendientes} entregas
          </span>
          <span style={{ color: "#94a3b8" }}>â€¢</span>
          <span style={{ color: "#6366f1", fontWeight: 600 }}>
            â†© {kpis.devolucionesPendientes} devoluciones
          </span>
        </div>
      </div>
    </div>
  );
}

