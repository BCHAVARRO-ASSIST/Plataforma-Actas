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
        className="stat-card"
        onClick={() => onNavigate("equipment")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div style={{ width: "32px", height: "32px", borderRadius: "var(--radius-sm)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <EquipmentIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Equipos en Inventario</span>
            </div>
            <span style={{ fontSize: "12px", color: "var(--color-primary-600)", fontWeight: 600 }}>Ver catálogo →</span>
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
            <span>{kpis.equiposInactivos} inactivos</span>
          </div>
        </div>
      </div>

      {/* 2. ACTAS */}
      <div
        className="stat-card"
        onClick={() => onNavigate("actas")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div style={{ width: "32px", height: "32px", borderRadius: "var(--radius-sm)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <ActaIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Gestión Documental</span>
            </div>
            <span style={{ fontSize: "12px", color: "var(--color-success-600)", fontWeight: 600 }}>Supervisar →</span>
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
              {kpis.actasBorrador} borradores
            </span>
          </div>
        </div>
      </div>

      {/* 3. LOGÍSTICA */}
      <div
        className="stat-card"
        onClick={() => onNavigate("reportes")}
        style={{ cursor: "pointer" }}
      >
        <div className="stat-info" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "8px" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <div style={{ width: "32px", height: "32px", borderRadius: "var(--radius-sm)", background: "var(--color-info-50)", color: "var(--color-info-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <MessengerIcon size={18} />
              </div>
              <span className="stat-label" style={{ margin: 0 }}>Operación y Traslados</span>
            </div>
            <span style={{ fontSize: "12px", color: "var(--color-info-600)", fontWeight: 600 }}>Ver informes →</span>
          </div>

          <div style={{ display: "flex", alignItems: "baseline", gap: "8px", margin: "8px 0 12px" }}>
            <span className="stat-value">{kpis.entregasPendientes + kpis.devolucionesPendientes}</span>
            <span style={{ fontSize: "13px", color: "var(--text-muted)" }}>trámites en curso</span>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px", fontSize: "12px", color: "var(--text-secondary)", paddingTop: "10px", borderTop: "1px solid var(--border-subtle)" }}>
            <span className="badge badge-info">
              {kpis.entregasPendientes} entregas activas
            </span>
            <span style={{ color: "var(--text-muted)" }}>•</span>
            <span className="badge badge-neutral">
              {kpis.devolucionesPendientes} devoluciones
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
