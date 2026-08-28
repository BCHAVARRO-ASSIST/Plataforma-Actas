import type { DashboardKpis } from "../dashboardService";

interface DashboardKpiProps {
  kpis: DashboardKpis;
}

export function DashboardKpi({ kpis }: DashboardKpiProps) {
  return (
    <section className="dashboard-kpi-grid" style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "16px", marginBottom: "24px" }}>
      {/* TOTAL EQUIPOS */}
      <article className="inventory-metric inventory-metric-dark">
        <div className="inventory-metric-top">
          <span>TOTAL EQUIPOS</span>
          <span>â–¦</span>
        </div>
        <strong>{kpis.totalEquipos}</strong>
        <p>Activos de hardware registrados</p>
      </article>

      {/* EQUIPOS ACTIVOS */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>EQUIPOS ACTIVOS</span>
          <span className="live-dot" />
        </div>
        <strong style={{ color: "#059669" }}>{kpis.equiposActivos}</strong>
        <p>
          {kpis.totalEquipos > 0
            ? `${Math.round((kpis.equiposActivos / kpis.totalEquipos) * 100)}% operativos`
            : "0% operativos"}
        </p>
      </article>

      {/* EQUIPOS ASIGNADOS */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>EN CUSTODIA / ASIGNADOS</span>
          <span>ðŸ‘¤</span>
        </div>
        <strong style={{ color: "#2563eb" }}>{kpis.equiposAsignados}</strong>
        <p>Entregados a colaboradores</p>
      </article>

      {/* EQUIPOS EN MANTENIMIENTO */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>MANTENIMIENTO</span>
          <span>âš™</span>
        </div>
        <strong style={{ color: "#d97706" }}>{kpis.equiposMantenimiento}</strong>
        <p>En revisión o soporte</p>
      </article>

      {/* TOTAL ACTAS */}
      <article className="inventory-metric inventory-metric-dark">
        <div className="inventory-metric-top">
          <span>TOTAL ACTAS</span>
          <span>â–¤</span>
        </div>
        <strong>{kpis.totalActas}</strong>
        <p>Procesos documentales</p>
      </article>

      {/* ACTAS PENDIENTES / BORRADOR */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>ACTAS PENDIENTES</span>
          <span>â³</span>
        </div>
        <strong style={{ color: "#e11d48" }}>{kpis.actasBorrador}</strong>
        <p>En espera de firma en campo</p>
      </article>

      {/* ACTAS FIRMADAS */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>ACTAS FIRMADAS</span>
          <span>✓</span>
        </div>
        <strong style={{ color: "#059669" }}>{kpis.actasFirmadas}</strong>
        <p>Certificadas y con firma</p>
      </article>

      {/* ACTAS FINALIZADAS */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>ACTAS FINALIZADAS</span>
          <span>ðŸ”’</span>
        </div>
        <strong style={{ color: "#4f46e5" }}>{kpis.actasFinalizadas}</strong>
        <p>Procesos cerrados formalmente</p>
      </article>

      {/* ENTREGAS PENDIENTES */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>ENTREGAS PENDIENTES</span>
          <span>ðŸ“¦</span>
        </div>
        <strong style={{ color: "#d97706" }}>{kpis.entregasPendientes}</strong>
        <p>Equipos por entregar</p>
      </article>

      {/* DEVOLUCIONES PENDIENTES */}
      <article className="inventory-metric">
        <div className="inventory-metric-top">
          <span>DEVOLUCIONES PENDIENTES</span>
          <span>â†©</span>
        </div>
        <strong style={{ color: "#6366f1" }}>{kpis.devolucionesPendientes}</strong>
        <p>Equipos en retorno</p>
      </article>
    </section>
  );
}

