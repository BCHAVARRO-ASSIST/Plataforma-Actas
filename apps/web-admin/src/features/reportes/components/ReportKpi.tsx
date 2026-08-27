import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../types/reportes";

interface ReportKpiProps {
  tab: ReportTab;
  data: EquiposReportItem[] | ActasReportItem[] | EntregasReportItem[] | DevolucionesReportItem[];
}

export function ReportKpi({ tab, data }: ReportKpiProps) {
  const count = data.length;

  if (tab === "equipos") {
    const list = data as EquiposReportItem[];
    const activos = list.filter((e) => e.estatusEquipo?.toLowerCase() === "activo").length;
    const asignados = list.filter((e) => e.estatusEquipo?.toLowerCase() === "asignado").length;
    const inactivos = list.filter((e) => e.estatusEquipo?.toLowerCase() === "inactivo").length;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <article className="inventory-metric inventory-metric-dark">
          <span>RESULTADOS FILTRADOS</span>
          <strong>{count}</strong>
          <p>Total equipos encontrados</p>
        </article>
        <article className="inventory-metric">
          <span>DISPONIBLES (ACTIVOS)</span>
          <strong style={{ color: "#059669" }}>{activos}</strong>
          <p>Listos para entrega</p>
        </article>
        <article className="inventory-metric">
          <span>EN CUSTODIA (ASIGNADOS)</span>
          <strong style={{ color: "#2563eb" }}>{asignados}</strong>
          <p>En uso por usuarios</p>
        </article>
        <article className="inventory-metric">
          <span>INACTIVOS / BODEGA</span>
          <strong style={{ color: "#e11d48" }}>{inactivos}</strong>
          <p>En baja o desuso</p>
        </article>
      </div>
    );
  }

  if (tab === "actas") {
    const list = data as ActasReportItem[];
    const firmadas = list.filter((a) => a.estado === "Firmada" || a.estado === "Finalizada").length;
    const borradores = list.filter((a) => a.estado === "Borrador").length;
    const anuladas = list.filter((a) => a.estado === "Anulada").length;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <article className="inventory-metric inventory-metric-dark">
          <span>RESULTADOS FILTRADOS</span>
          <strong>{count}</strong>
          <p>Total actas encontradas</p>
        </article>
        <article className="inventory-metric">
          <span>PROCESADAS (FIRMADAS)</span>
          <strong style={{ color: "#059669" }}>{firmadas}</strong>
          <p>Con firma certificada</p>
        </article>
        <article className="inventory-metric">
          <span>PENDIENTES (BORRADOR)</span>
          <strong style={{ color: "#d97706" }}>{borradores}</strong>
          <p>Por firmar en campo</p>
        </article>
        <article className="inventory-metric">
          <span>ANULADAS</span>
          <strong style={{ color: "#e11d48" }}>{anuladas}</strong>
          <p>Canceladas</p>
        </article>
      </div>
    );
  }

  if (tab === "entregas") {
    const list = data as EntregasReportItem[];
    const entregadas = list.filter((e) => e.firmado).length;
    const pendientes = list.filter((e) => !e.firmado).length;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <article className="inventory-metric inventory-metric-dark">
          <span>TOTAL ENTREGAS</span>
          <strong>{count}</strong>
          <p>En el rango seleccionado</p>
        </article>
        <article className="inventory-metric">
          <span>ENTREGAS CONCLUIDAS</span>
          <strong style={{ color: "#059669" }}>{entregadas}</strong>
          <p>Firmadas por el receptor</p>
        </article>
        <article className="inventory-metric">
          <span>ENTREGAS EN CURSO</span>
          <strong style={{ color: "#d97706" }}>{pendientes}</strong>
          <p>En ruta o borrador</p>
        </article>
      </div>
    );
  }

  const list = data as DevolucionesReportItem[];
  const firmadas = list.filter((d) => d.estado === "Firmada" || d.estado === "Finalizada").length;

  return (
    <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))", gap: "12px", marginBottom: "20px" }}>
      <article className="inventory-metric inventory-metric-dark">
        <span>TOTAL DEVOLUCIONES</span>
        <strong>{count}</strong>
        <p>En el rango seleccionado</p>
      </article>
      <article className="inventory-metric">
        <span>DEVOLUCIONES CONCLUIDAS</span>
        <strong style={{ color: "#059669" }}>{firmadas}</strong>
        <p>Equipo reingresado a bodega</p>
      </article>
    </div>
  );
}
