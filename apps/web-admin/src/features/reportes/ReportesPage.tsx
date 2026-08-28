import { useState, useEffect } from "react";
import { ReportFilters } from "./components/ReportFilters";
import { ReportKpi } from "./components/ReportKpi";
import { ReportCharts } from "./components/ReportCharts";
import { ReportTable } from "./components/ReportTable";
import {
  getEquiposReport,
  getActasReport,
  getEntregasReport,
  getDevolucionesReport,
  exportToCsv,
  exportToPdf,
} from "./reportesService";
import type {
  ReportTab,
  ReportFilter,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "./reportesService";

export function ReportesPage() {
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

  // =========================================================
  // EXPORTAR A EXCEL
  // =========================================================

  function handleExportExcel() {
    if (activeTab === "equipos") {
      const headers = ["Clave Inventario", "Nombre Equipo", "Marca", "Asignado A", "Departamento", "Empresa", "Proveedor", "Estado", "Tipo Mantenimiento", "Fecha Mantenimiento"];
      const rows = equiposData.map((e) => [
        e.inventoryKey,
        e.nombreEquipo,
        e.marca,
        e.asignadoA,
        e.departamento,
        e.empresa,
        e.proveedor,
        e.estatusEquipo,
        e.tipoMantenimiento,
        e.fechaMantenimiento,
      ]);
      exportToCsv("Reporte_Equipos", headers, rows);
    } else if (activeTab === "actas") {
      const headers = ["Número Acta", "Tipo", "Estado", "Clave Equipo", "Nombre Equipo", "Creador", "Destinatario", "Mensajero", "Fecha Creación", "Fecha Firma", "Fecha Finalización"];
      const rows = actasData.map((a) => [
        a.numeroActa,
        a.tipoActa,
        a.estado,
        a.equipmentInventoryKey,
        a.equipmentNombre,
        a.creadorNombre,
        a.destinatarioNombre,
        a.mensajeroNombre,
        a.fechaCreacion,
        a.fechaFirma,
        a.fechaFinalizacion,
      ]);
      exportToCsv("Reporte_Actas", headers, rows);
    } else if (activeTab === "entregas") {
      const headers = ["Número Acta", "Clave Equipo", "Nombre Equipo", "Mensajero", "Destinatario", "Fecha Creación", "Fecha Firma", "Firmada", "Estado"];
      const rows = entregasData.map((e) => [
        e.numeroActa,
        e.equipmentInventoryKey,
        e.equipmentNombre,
        e.mensajeroNombre,
        e.destinatarioNombre,
        e.fechaCreacion,
        e.fechaFirma,
        e.firmado ? "Sí" : "No",
        e.estado,
      ]);
      exportToCsv("Reporte_Entregas", headers, rows);
    } else if (activeTab === "devoluciones") {
      const headers = ["Número Acta Devolución", "Acta Origen Vinculada", "Clave Equipo", "Nombre Equipo", "Responsable Entrega", "Fecha Creación", "Fecha Firma", "Estado"];
      const rows = devolucionesData.map((d) => [
        d.numeroActa,
        d.actaOrigenNumero || d.actaOrigenId,
        d.equipmentInventoryKey,
        d.equipmentNombre,
        d.responsableEntrega,
        d.fechaCreacion,
        d.fechaFirma,
        d.estado,
      ]);
      exportToCsv("Reporte_Devoluciones", headers, rows);
    }
  }

  // =========================================================
  // EXPORTAR A PDF
  // =========================================================

  function handleExportPdf() {
    if (activeTab === "equipos") {
      const headers = ["Inventario", "Nombre Equipo", "Marca", "Asignado A", "Área", "Empresa", "Estado"];
      const rows = equiposData.map((e) => [
        e.inventoryKey,
        e.nombreEquipo,
        e.marca,
        e.asignadoA,
        e.departamento,
        e.empresa,
        e.estatusEquipo,
      ]);
      exportToPdf("Reporte Consolidado de Equipos e Inventario", headers, rows);
    } else if (activeTab === "actas") {
      const headers = ["N° Acta", "Tipo", "Estado", "Equipo", "Destinatario", "Mensajero", "Fecha Creación"];
      const rows = actasData.map((a) => [
        a.numeroActa,
        a.tipoActa,
        a.estado,
        a.equipmentNombre || a.equipmentInventoryKey,
        a.destinatarioNombre,
        a.mensajeroNombre,
        new Date(a.fechaCreacion).toLocaleDateString("es-CO"),
      ]);
      exportToPdf("Reporte Maestro de Actas y Trazabilidad", headers, rows);
    } else if (activeTab === "entregas") {
      const headers = ["N° Acta", "Equipo", "Mensajero", "Destinatario", "Fecha Creación", "Firma", "Estado"];
      const rows = entregasData.map((e) => [
        e.numeroActa,
        e.equipmentNombre,
        e.mensajeroNombre,
        e.destinatarioNombre,
        new Date(e.fechaCreacion).toLocaleDateString("es-CO"),
        e.firmado ? "Firmada" : "Pendiente",
        e.estado,
      ]);
      exportToPdf("Reporte de Entregas y Custodia en Campo", headers, rows);
    } else if (activeTab === "devoluciones") {
      const headers = ["N° Devolución", "Acta Origen", "Equipo", "Responsable", "Fecha Creación", "Estado"];
      const rows = devolucionesData.map((d) => [
        d.numeroActa,
        d.actaOrigenNumero || "—",
        d.equipmentNombre,
        d.responsableEntrega,
        new Date(d.fechaCreacion).toLocaleDateString("es-CO"),
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

  return (
    <main className="inventory-page" style={{ padding: "24px", maxWidth: "1600px", margin: "0 auto" }}>
      {/* HEADER */}
      <header className="inventory-header" style={{ marginBottom: "20px" }}>
        <div>
          <span className="inventory-kicker">INFORMES Y EXPORTACIÓN AVANZADA</span>
          <h1 style={{ margin: "4px 0 8px", fontSize: "24px", fontWeight: 800, color: "#0f172a" }}>
            Módulo de Reportes Oficiales
          </h1>
          <p style={{ margin: 0, color: "#475569", fontSize: "14px" }}>
            Consolidación y exportación de información operativa, inventario de hardware y trazabilidad documental.
          </p>
        </div>

        <div style={{ display: "flex", gap: "10px" }}>
          <button
            type="button"
            className="actas-reset"
            onClick={handleExportExcel}
            disabled={loading || activeData.length === 0}
            style={{ display: "flex", alignItems: "center", gap: "6px", fontSize: "13px" }}
          >
            <span>ðŸ“¥</span> Exportar a Excel (.CSV)
          </button>

          <button
            type="button"
            className="actas-primary"
            onClick={handleExportPdf}
            disabled={loading || activeData.length === 0}
            style={{ minHeight: "36px", padding: "8px 16px", fontSize: "13px", display: "flex", alignItems: "center", gap: "6px" }}
          >
            <span>ðŸ“„</span> Exportar a PDF
          </button>
        </div>
      </header>

      {/* ERROR */}
      {error && (
        <div className="actas-error" style={{ marginBottom: "20px" }}>
          {error}
        </div>
      )}

      {/* TABS DE SELECCIÓN */}
      <div style={{ display: "flex", gap: "8px", borderBottom: "2px solid #e2e8f0", marginBottom: "20px" }}>
        {[
          { id: "equipos", label: "ðŸ’» Catálogo de Equipos", count: equiposData.length },
          { id: "actas", label: "ðŸ“‹ Todas las Actas", count: actasData.length },
          { id: "entregas", label: "ðŸ“¦ Reporte de Entregas", count: entregasData.length },
          { id: "devoluciones", label: "â†© Reporte de Devoluciones", count: devolucionesData.length },
        ].map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => handleTabChange(tab.id as ReportTab)}
            style={{
              padding: "10px 18px",
              border: "none",
              background: "transparent",
              borderBottom: activeTab === tab.id ? "3px solid #0f172a" : "3px solid transparent",
              color: activeTab === tab.id ? "#0f172a" : "#64748b",
              fontWeight: activeTab === tab.id ? 700 : 500,
              fontSize: "14px",
              cursor: "pointer",
              marginBottom: "-2px",
              display: "flex",
              alignItems: "center",
              gap: "8px",
            }}
          >
            <span>{tab.label}</span>
            <span
              style={{
                fontSize: "11px",
                padding: "2px 6px",
                borderRadius: "10px",
                backgroundColor: activeTab === tab.id ? "#0f172a" : "#e2e8f0",
                color: activeTab === tab.id ? "#ffffff" : "#475569",
              }}
            >
              {tab.count}
            </span>
          </button>
        ))}
      </div>

      {/* FILTROS */}
      <ReportFilters
        tab={activeTab}
        filters={filters}
        onFilterChange={setFilters}
        onApply={handleApplyFilters}
        onClear={handleClearFilters}
        loading={loading}
      />

      {/* KPIS DINÁMICOS */}
      <ReportKpi tab={activeTab} data={activeData} />

      {/* GRÁFICA RESUMEN */}
      <ReportCharts tab={activeTab} data={activeData} />

      {/* TABLA DE RESULTADOS */}
      <section className="inventory-panel">
        <ReportTable tab={activeTab} data={activeData} />
      </section>
    </main>
  );
}

