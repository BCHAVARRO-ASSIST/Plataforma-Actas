import { useState, useEffect } from "react";
import { ReportFilters } from "./components/ReportFilters";
import { ReportKpi } from "./components/ReportKpi";
import { ReportCharts } from "./components/ReportCharts";
import { ReportTable } from "./components/ReportTable";
import { formatDate } from "@/shared/utils/date";
import {
  getEquiposReport,
  getActasReport,
  getEntregasReport,
  getDevolucionesReport,
  exportToCsv,
  exportToExcel,
  exportToPdf,
} from "./reportsService";
import type {
  ReportTab,
  ReportFilter,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "./reportsService";
import {
  EquipmentIcon,
  ActaIcon,
  MessengerIcon,
  PackageIcon,
  DocumentPdfIcon,
  FileSpreadsheetIcon,
  FileTextIcon,
  RefreshIcon,
} from "@/components/ui/Icons";

export function ReportsPage() {
  const [activeTab, setActiveTab] = useState<ReportTab>("equipos");
  const [filters, setFilters] = useState<ReportFilter>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Datasets
  const [equiposData, setEquiposData] = useState<EquiposReportItem[]>([]);
  const [actasData, setActasData] = useState<ActasReportItem[]>([]);
  const [entregasData, setEntregasData] = useState<EntregasReportItem[]>([]);
  const [devolucionesData, setDevolucionesData] = useState<DevolucionesReportItem[]>([]);

  const runFetch = async (tab: ReportTab, currentFilters: ReportFilter) => {
    try {
      setLoading(true);
      setError(null);

      if (tab === "equipos") {
        const data = await getEquiposReport(currentFilters);
        setEquiposData(data);
      } else if (tab === "actas") {
        const data = await getActasReport(currentFilters);
        setActasData(data);
      } else if (tab === "entregas") {
        const data = await getEntregasReport(currentFilters);
        setEntregasData(data);
      } else if (tab === "devoluciones") {
        const data = await getDevolucionesReport(currentFilters);
        setDevolucionesData(data);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al consultar los reportes.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let mounted = true;
    if (activeTab === "equipos") {
      getEquiposReport(filters).then((d) => { if (mounted) setEquiposData(d); }).catch((e) => { if (mounted) setError(e.message); });
    } else if (activeTab === "actas") {
      getActasReport(filters).then((d) => { if (mounted) setActasData(d); }).catch((e) => { if (mounted) setError(e.message); });
    } else if (activeTab === "entregas") {
      getEntregasReport(filters).then((d) => { if (mounted) setEntregasData(d); }).catch((e) => { if (mounted) setError(e.message); });
    } else if (activeTab === "devoluciones") {
      getDevolucionesReport(filters).then((d) => { if (mounted) setDevolucionesData(d); }).catch((e) => { if (mounted) setError(e.message); });
    }

    return () => {
      mounted = false;
    };
  }, [activeTab, filters]);

  function handleTabChange(tab: ReportTab) {
    setActiveTab(tab);
    setFilters({});
  }

  function handleApplyFilters() {
    runFetch(activeTab, filters);
  }

  function handleClearFilters() {
    setFilters({});
    runFetch(activeTab, {});
  }

  function handleExportExcel() {
    if (activeTab === "equipos") {
      const headers = ["Clave Inventario", "Nombre Equipo", "Marca", "Asignado A", "Departamento", "Proveedor", "Estado", "Tipo Mantenimiento", "Fecha Mantenimiento"];
      const rows = equiposData.map((e) => [
        e.inventoryKey,
        e.nombreEquipo,
        e.marca,
        e.asignadoA,
        e.departamento,
        e.proveedor,
        e.estatusEquipo,
        e.tipoMantenimiento,
        formatDate(e.fechaMantenimiento),
      ]);
      exportToExcel("Reporte_Equipos_ASSIST", "Reporte Consolidado de Equipos e Inventario", headers, rows);
    } else if (activeTab === "actas") {
      const headers = ["Número Acta", "Tipo Acta", "Estado", "Clave Equipo", "Nombre Equipo", "Destinatario", "Mensajero", "Fecha Creación", "Fecha Firma", "Fecha Finalización"];
      const rows = actasData.map((a) => [
        a.numeroActa,
        a.tipoActa,
        a.estado,
        a.inventoryKey,
        a.nombreEquipo,
        a.destinatarioNombre,
        a.mensajeroNombre,
        formatDate(a.fechaCreacion),
        formatDate(a.fechaFirma),
        formatDate(a.fechaFinalizacion),
      ]);
      exportToExcel("Reporte_Actas_ASSIST", "Reporte Maestro de Actas y Trazabilidad", headers, rows);
    } else if (activeTab === "entregas") {
      const headers = ["Número Acta", "Clave Equipo", "Nombre Equipo", "Mensajero", "Destinatario", "Fecha Creación", "Fecha Firma", "Firmada", "Estado"];
      const rows = entregasData.map((e) => [
        e.numeroActa,
        e.inventoryKey,
        e.nombreEquipo,
        e.mensajeroNombre,
        e.destinatarioNombre,
        formatDate(e.fechaCreacion),
        formatDate(e.fechaFirma),
        e.firmado ? "Sí" : "No",
        e.estado,
      ]);
      exportToExcel("Reporte_Entregas_ASSIST", "Reporte de Entregas y Custodia en Campo", headers, rows);
    } else if (activeTab === "devoluciones") {
      const headers = ["Número Acta Devolución", "Acta Origen Vinculada", "Clave Equipo", "Nombre Equipo", "Responsable Entrega", "Fecha Creación", "Fecha Firma", "Estado"];
      const rows = devolucionesData.map((d) => [
        d.numeroActa,
        d.actaOrigenNumero || d.actaOrigenId || "—",
        d.inventoryKey,
        d.nombreEquipo,
        d.responsableEntrega,
        formatDate(d.fechaCreacion),
        formatDate(d.fechaFirma),
        d.estado,
      ]);
      exportToExcel("Reporte_Devoluciones_ASSIST", "Reporte de Devoluciones y Reingresos a Bodega", headers, rows);
    }
  }

  function handleExportCsv() {
    if (activeTab === "equipos") {
      const headers = ["Clave Inventario", "Nombre Equipo", "Marca", "Procesador", "RAM", "Disco Duro", "Asignado A", "Departamento", "Proveedor", "Estatus"];
      const rows = equiposData.map((e) => [
        e.inventoryKey,
        e.nombreEquipo,
        e.marca,
        e.procesador,
        e.memoriaRam,
        e.discoDuro,
        e.asignadoA,
        e.departamento,
        e.proveedor,
        e.estatusEquipo,
      ]);
      exportToCsv("Reporte_Equipos_ASSIST", "Reporte Consolidado de Equipos e Inventario", headers, rows);
    } else if (activeTab === "actas") {
      const headers = ["Número Acta", "Tipo Acta", "Estado", "Clave Equipo", "Nombre Equipo", "Destinatario", "Mensajero", "Fecha Creación", "Fecha Firma", "Fecha Finalización"];
      const rows = actasData.map((a) => [
        a.numeroActa,
        a.tipoActa,
        a.estado,
        a.inventoryKey,
        a.nombreEquipo,
        a.destinatarioNombre,
        a.mensajeroNombre,
        formatDate(a.fechaCreacion),
        formatDate(a.fechaFirma),
        formatDate(a.fechaFinalizacion),
      ]);
      exportToCsv("Reporte_Actas_ASSIST", "Reporte Maestro de Actas y Trazabilidad", headers, rows);
    } else if (activeTab === "entregas") {
      const headers = ["Número Acta", "Clave Equipo", "Nombre Equipo", "Mensajero", "Destinatario", "Fecha Creación", "Fecha Firma", "Firmada", "Estado"];
      const rows = entregasData.map((e) => [
        e.numeroActa,
        e.inventoryKey,
        e.nombreEquipo,
        e.mensajeroNombre,
        e.destinatarioNombre,
        formatDate(e.fechaCreacion),
        formatDate(e.fechaFirma),
        e.firmado ? "Sí" : "No",
        e.estado,
      ]);
      exportToCsv("Reporte_Entregas_ASSIST", "Reporte de Entregas y Custodia en Campo", headers, rows);
    } else if (activeTab === "devoluciones") {
      const headers = ["Número Acta Devolución", "Acta Origen Vinculada", "Clave Equipo", "Nombre Equipo", "Responsable Entrega", "Fecha Creación", "Fecha Firma", "Estado"];
      const rows = devolucionesData.map((d) => [
        d.numeroActa,
        d.actaOrigenNumero || d.actaOrigenId || "—",
        d.inventoryKey,
        d.nombreEquipo,
        d.responsableEntrega,
        formatDate(d.fechaCreacion),
        formatDate(d.fechaFirma),
        d.estado,
      ]);
      exportToCsv("Reporte_Devoluciones_ASSIST", "Reporte de Devoluciones y Reingresos a Bodega", headers, rows);
    }
  }

  function handleExportPdf() {
    if (activeTab === "equipos") {
      const headers = ["Inventario", "Nombre Equipo", "Marca", "Asignado A", "Área / Dpto", "Proveedor", "Estado"];
      const rows = equiposData.map((e) => [
        e.inventoryKey,
        e.nombreEquipo,
        e.marca,
        e.asignadoA,
        e.departamento,
        e.proveedor,
        e.estatusEquipo,
      ]);
      exportToPdf("Reporte Consolidado de Equipos e Inventario", headers, rows);
    } else if (activeTab === "actas") {
      const headers = ["N° Acta", "Tipo", "Estado", "Equipo Asociado", "Destinatario", "Mensajero / Custodia", "Fecha Emisión"];
      const rows = actasData.map((a) => [
        a.numeroActa,
        a.tipoActa,
        a.estado,
        a.nombreEquipo || a.inventoryKey,
        a.destinatarioNombre,
        a.mensajeroNombre,
        formatDate(a.fechaCreacion),
      ]);
      exportToPdf("Reporte Maestro de Actas y Trazabilidad", headers, rows);
    } else if (activeTab === "entregas") {
      const headers = ["N° Acta", "Equipo Asociado", "Mensajero Responsable", "Destinatario", "Fecha Emisión", "Firma", "Estado"];
      const rows = entregasData.map((e) => [
        e.numeroActa,
        e.nombreEquipo,
        e.mensajeroNombre,
        e.destinatarioNombre,
        formatDate(e.fechaCreacion),
        e.firmado ? "Firmada" : "Pendiente",
        e.estado,
      ]);
      exportToPdf("Reporte de Entregas y Custodia en Campo", headers, rows);
    } else if (activeTab === "devoluciones") {
      const headers = ["N° Devolución", "Acta Origen", "Equipo Asociado", "Responsable Entrega", "Fecha Creación", "Estado"];
      const rows = devolucionesData.map((d) => [
        d.numeroActa,
        d.actaOrigenNumero || "—",
        d.nombreEquipo,
        d.responsableEntrega,
        formatDate(d.fechaCreacion),
        d.estado,
      ]);
      exportToPdf("Reporte de Devoluciones y Reingresos a Bodega", headers, rows);
    }
  }

  const activeData =
    activeTab === "equipos"
      ? equiposData
      : activeTab === "actas"
      ? actasData
      : activeTab === "entregas"
      ? entregasData
      : devolucionesData;

  const tabsConfig: { id: ReportTab; label: string; icon: React.ReactNode; count: number }[] = [
    { id: "equipos", label: "Catálogo de Equipos", icon: <EquipmentIcon size={16} />, count: equiposData.length },
    { id: "actas", label: "Todas las Actas", icon: <ActaIcon size={16} />, count: actasData.length },
    { id: "entregas", label: "Reporte de Entregas", icon: <MessengerIcon size={16} />, count: entregasData.length },
    { id: "devoluciones", label: "Reporte de Devoluciones", icon: <PackageIcon size={16} />, count: devolucionesData.length },
  ];

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            INFORMES Y EXPORTACIÓN AVANZADA
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Módulo de Reportes Oficiales
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Consolidación y exportación de información operativa, inventario y trazabilidad documental.
          </p>
        </div>

        <div style={{ display: "flex", gap: "10px", alignItems: "center", flexWrap: "wrap" }}>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={() => runFetch(activeTab, filters)}
            disabled={loading}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            title="Refrescar datos del reporte"
          >
            <RefreshIcon size={14} className={loading ? "spin-animation" : ""} />
            <span>Actualizar</span>
          </button>

          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={handleExportExcel}
            disabled={loading || activeData.length === 0}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            title="Descargar libro Excel estructurado con membrete corporativo y formato oficial"
          >
            <FileSpreadsheetIcon size={15} style={{ color: "#16a34a" }} />
            <span>Exportar Excel (.xls)</span>
          </button>

          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={handleExportCsv}
            disabled={loading || activeData.length === 0}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            title="Descargar archivo CSV estándar con cabecera de metadatos ASSIST"
          >
            <FileTextIcon size={15} style={{ color: "var(--color-primary-600)" }} />
            <span>Exportar CSV</span>
          </button>

          <button
            type="button"
            className="btn btn-primary btn-sm"
            onClick={handleExportPdf}
            disabled={loading || activeData.length === 0}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            title="Generar reporte oficial en PDF con logo institucional ASSIST para guardar o imprimir"
          >
            <DocumentPdfIcon size={15} />
            <span>Exportar PDF</span>
          </button>
        </div>
      </div>

      {error && (
        <div className="auth-alert error" style={{ marginBottom: "16px" }}>
          <span>!</span>
          <span>{error}</span>
        </div>
      )}

      {/* Modern Segmented Navigation Tabs Grid */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(210px, 1fr))",
          gap: "8px",
          padding: "6px",
          background: "var(--color-slate-100)",
          borderRadius: "var(--radius-lg)",
          marginBottom: "20px",
          border: "1px solid var(--border-default)",
        }}
      >
        {tabsConfig.map((tab) => {
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => handleTabChange(tab.id)}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "8px",
                padding: "9px 16px",
                border: isActive ? "1px solid var(--color-primary-200)" : "1px solid transparent",
                borderRadius: "var(--radius-md)",
                background: isActive ? "#ffffff" : "transparent",
                color: isActive ? "var(--color-primary-700)" : "var(--text-secondary)",
                fontWeight: isActive ? 700 : 500,
                fontSize: "13.5px",
                cursor: "pointer",
                boxShadow: isActive ? "0 2px 8px rgba(99, 102, 241, 0.1)" : "none",
                transition: "all var(--transition-fast)",
                whiteSpace: "nowrap",
                width: "100%",
              }}
            >
              <span style={{ color: isActive ? "var(--color-primary-600)" : "var(--text-muted)", display: "flex", alignItems: "center" }}>
                {tab.icon}
              </span>
              <span>{tab.label}</span>
              <span
                style={{
                  fontSize: "11px",
                  padding: "2px 8px",
                  borderRadius: "var(--radius-pill)",
                  backgroundColor: isActive ? "var(--color-primary-100)" : "var(--color-slate-200)",
                  color: isActive ? "var(--color-primary-800)" : "var(--text-muted)",
                  fontWeight: 700,
                }}
              >
                {tab.count}
              </span>
            </button>
          );
        })}
      </div>

      {/* Filters Toolbar */}
      <ReportFilters
        tab={activeTab}
        filters={filters}
        onFilterChange={setFilters}
        onApply={handleApplyFilters}
        onClear={handleClearFilters}
        loading={loading}
      />

      {/* KPI Cards */}
      <ReportKpi tab={activeTab} data={activeData} />

      {/* Distribution Chart */}
      <ReportCharts tab={activeTab} data={activeData} />

      {/* Data Table Card */}
      <div className="card" style={{ padding: 0, overflow: "hidden", marginBottom: "30px" }}>
        <div className="card-header">
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
              REGISTROS OFICIALES
            </span>
            <h3 style={{ margin: "2px 0 0", fontSize: "15px", fontWeight: 700, color: "var(--text-primary)" }}>
              Detalle Consolidado de {tabsConfig.find((t) => t.id === activeTab)?.label}
            </h3>
          </div>
          <span style={{ fontSize: "12px", color: "var(--text-muted)", fontWeight: 500 }}>
            {activeData.length} {activeData.length === 1 ? "registro" : "registros"}
          </span>
        </div>

        <ReportTable tab={activeTab} data={activeData} />
      </div>
    </div>
  );
}

// Retrocompatibilidad
export { ReportsPage as ReportesPage };
