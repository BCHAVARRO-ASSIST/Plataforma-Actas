import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../features/reportes/reportesService";
import {
  EquipmentIcon,
  CheckIcon,
  ClockIcon,
  ActaIcon,
  PackageIcon,
} from "./Icons";

interface ReportKpiProps {
  tab: ReportTab;
  data: EquiposReportItem[] | ActasReportItem[] | EntregasReportItem[] | DevolucionesReportItem[];
}

export function ReportKpi({ tab, data }: ReportKpiProps) {
  const count = data.length;

  if (tab === "equipos") {
    const list = data as EquiposReportItem[];
    const { activos, asignados, inactivos } = list.reduce(
      (acc, e) => {
        const estatus = e.estatusEquipo?.toLowerCase();
        if (estatus === "activo") acc.activos++;
        if (estatus === "asignado") acc.asignados++;
        if (estatus === "inactivo") acc.inactivos++;
        return acc;
      },
      { activos: 0, asignados: 0, inactivos: 0 }
    );

    const activosPct = count > 0 ? Math.round((activos / count) * 100) : 0;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "14px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Total Filtrados
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <EquipmentIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--text-primary)" }}>{count}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Equipos bajo este criterio</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Disponibles
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <CheckIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-success-700)" }}>{activos}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>{activosPct}% listos para asignación</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-primary-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Asignados
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <PackageIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-primary-700)" }}>{asignados}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>En custodia de colaboradores</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Inactivos / Baja
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-slate-100)", color: "var(--color-slate-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <ClockIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--text-muted)" }}>{inactivos}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>En bodega o desuso</p>
        </div>
      </div>
    );
  }

  if (tab === "actas") {
    const list = data as ActasReportItem[];
    const { firmadas, borradores, anuladas } = list.reduce(
      (acc, a) => {
        if (a.estado === "Firmada" || a.estado === "Finalizada") acc.firmadas++;
        if (a.estado === "Borrador") acc.borradores++;
        if (a.estado === "Anulada") acc.anuladas++;
        return acc;
      },
      { firmadas: 0, borradores: 0, anuladas: 0 }
    );

    const firmadasPct = count > 0 ? Math.round((firmadas / count) * 100) : 0;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "14px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Total Actas
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <ActaIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--text-primary)" }}>{count}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Documentos registrados</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Firmadas / Validadas
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <CheckIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-success-700)" }}>{firmadas}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>{firmadasPct}% con firma digital</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Borradores
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <ClockIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-warning-700)" }}>{borradores}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Pendientes por procesar</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-danger-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Anuladas
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-danger-50)", color: "var(--color-danger-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <ClockIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-danger-700)" }}>{anuladas}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Actas canceladas</p>
        </div>
      </div>
    );
  }

  if (tab === "entregas") {
    const list = data as EntregasReportItem[];
    const { entregadas, pendientes } = list.reduce(
      (acc, e) => {
        if (e.firmado) acc.entregadas++;
        else acc.pendientes++;
        return acc;
      },
      { entregadas: 0, pendientes: 0 }
    );
    const entregadasPct = count > 0 ? Math.round((entregadas / count) * 100) : 0;

    return (
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "14px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Total Entregas
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <PackageIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--text-primary)" }}>{count}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>En el rango seleccionado</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              Concluidas
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <CheckIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-success-700)" }}>{entregadas}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>{entregadasPct}% firmadas por receptor</p>
        </div>

        <div className="card" style={{ padding: "16px 20px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
            <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
              En Ruta / Pendientes
            </span>
            <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <ClockIcon size={18} />
            </div>
          </div>
          <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-warning-700)" }}>{pendientes}</div>
          <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Pendientes de entrega o firma</p>
        </div>
      </div>
    );
  }

  const list = data as DevolucionesReportItem[];
  const firmadas = list.filter((d) => d.estado === "Firmada" || d.estado === "Finalizada").length;
  const pendientes = count - firmadas;

  return (
    <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "14px", marginBottom: "20px" }}>
      <div className="card" style={{ padding: "16px 20px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
          <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Total Devoluciones
          </span>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <PackageIcon size={18} />
          </div>
        </div>
        <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--text-primary)" }}>{count}</div>
        <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Trámites de reintegro</p>
      </div>

      <div className="card" style={{ padding: "16px 20px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
          <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
            Reingresadas a Bodega
          </span>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <CheckIcon size={18} />
          </div>
        </div>
        <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-success-700)" }}>{firmadas}</div>
        <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Custodia confirmada en bodega</p>
      </div>

      <div className="card" style={{ padding: "16px 20px" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "6px" }}>
          <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.5px" }}>
            En Trámite
          </span>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <ClockIcon size={18} />
          </div>
        </div>
        <div style={{ fontSize: "24px", fontWeight: 800, color: "var(--color-warning-700)" }}>{pendientes}</div>
        <p style={{ margin: "4px 0 0", fontSize: "12px", color: "var(--text-muted)" }}>Pendiente de firma o inspección</p>
      </div>
    </div>
  );
}
