import type { DashboardKpis } from "@/features/dashboard/dashboardService";
import { EquipmentIcon, ActaIcon, MessengerIcon } from "@/components/ui/Icons";

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
    <div className="dashboard-kpi-grid">
      {/* 1. INVENTARIO */}
      <div
        className="stat-card kpi-card-inventario"
        onClick={() => onNavigate("equipment")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div className="kpi-icon-box">
                <EquipmentIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Equipos en Inventario</span>
            </div>
            <span className="kpi-action-link">Ver catálogo →</span>
          </div>

          <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "8px 0 12px" }}>
            <span className="stat-value">{kpis.totalEquipos}</span>
            <span style={{ fontSize: "13px", color: "var(--text-muted)" }}>activos registrados</span>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px", fontSize: "12px", color: "var(--text-secondary)", paddingTop: "10px", borderTop: "1px solid var(--border-subtle)" }}>
            <span className="badge badge-success">
              <span className="badge-dot" />
              {kpis.equiposActivos} operativos ({activosPct}%)
            </span>
            <span style={{ color: "var(--text-muted)" }}>•</span>
            <span className="badge badge-neutral">
              <span className="badge-dot" />
              {kpis.equiposInactivos} inactivos
            </span>
          </div>
        </div>
      </div>

      {/* 2. ACTAS */}
      <div
        className="stat-card kpi-card-actas"
        onClick={() => onNavigate("actas")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div className="kpi-icon-box">
                <ActaIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Gestión Documental</span>
            </div>
            <span className="kpi-action-link">Supervisar →</span>
          </div>

          <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "8px 0 12px" }}>
            <span className="stat-value">{kpis.totalActas}</span>
            <span style={{ fontSize: "13px", color: "var(--text-muted)" }}>actas emitidas</span>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px", fontSize: "12px", color: "var(--text-secondary)", paddingTop: "10px", borderTop: "1px solid var(--border-subtle)" }}>
            <span className="badge badge-success">
              <span className="badge-dot" />
              {kpis.actasFirmadas + kpis.actasFinalizadas} firmadas ({firmadasPct}%)
            </span>
            <span style={{ color: "var(--text-muted)" }}>•</span>
            <span className={kpis.actasBorrador > 0 ? "badge badge-warning" : "badge badge-neutral"}>
              <span className="badge-dot" />
              {kpis.actasBorrador} borradores
            </span>
          </div>
        </div>
      </div>

      {/* 3. LOGÍSTICA */}
      <div
        className="stat-card kpi-card-operacion"
        onClick={() => onNavigate("reportes")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div className="kpi-icon-box">
                <MessengerIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Operación y Traslados</span>
            </div>
            <span className="kpi-action-link">Ver informes →</span>
          </div>

          <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "8px 0 12px" }}>
            <span className="stat-value">{kpis.entregasPendientes + kpis.devolucionesPendientes}</span>
            <span style={{ fontSize: "13px", color: "var(--text-muted)" }}>trámites en curso</span>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px", fontSize: "12px", color: "var(--text-secondary)", paddingTop: "10px", borderTop: "1px solid var(--border-subtle)" }}>
            <span className="badge badge-info">
              <span className="badge-dot" />
              {kpis.entregasPendientes} entregas activas
            </span>
            <span style={{ color: "var(--text-muted)" }}>•</span>
            <span className="badge badge-neutral">
              <span className="badge-dot" />
              {kpis.devolucionesPendientes} devoluciones
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
