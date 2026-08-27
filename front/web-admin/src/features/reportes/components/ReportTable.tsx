import { useState, useMemo } from "react";
import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../types/reportes";

interface ReportTableProps {
  tab: ReportTab;
  data: EquiposReportItem[] | ActasReportItem[] | EntregasReportItem[] | DevolucionesReportItem[];
}

const ITEMS_PER_PAGE = 20;

function formatDate(val: string | null): string {
  if (!val) return "—";
  try {
    return new Date(val).toLocaleDateString("es-CO", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  } catch {
    return val;
  }
}

export function ReportTable({ tab, data }: ReportTableProps) {
  const [currentPage, setCurrentPage] = useState(1);

  const totalPages = Math.max(1, Math.ceil(data.length / ITEMS_PER_PAGE));

  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    return data.slice(start, start + ITEMS_PER_PAGE);
  }, [data, currentPage]);

  if (data.length === 0) {
    return (
      <div className="table-container" style={{ padding: "40px 20px", textAlign: "center", color: "#64748b" }}>
        <p style={{ margin: "0 0 6px", fontWeight: 600 }}>No se encontraron registros con los filtros seleccionados.</p>
        <small>Modifica las fechas o términos de búsqueda para ver más información.</small>
      </div>
    );
  }

  const firstItem = (currentPage - 1) * ITEMS_PER_PAGE + 1;
  const lastItem = Math.min(currentPage * ITEMS_PER_PAGE, data.length);

  return (
    <div className="table-container">
      <table>
        {/* =====================================================
            HEADER POR PESTAÑA
        ====================================================== */}
        {tab === "equipos" && (
          <thead>
            <tr>
              <th>Inventario</th>
              <th>Nombre Equipo</th>
              <th>Marca</th>
              <th>Asignado a</th>
              <th>Departamento</th>
              <th>Empresa</th>
              <th>Proveedor</th>
              <th>Estado</th>
            </tr>
          </thead>
        )}

        {tab === "actas" && (
          <thead>
            <tr>
              <th>N° Acta</th>
              <th>Tipo</th>
              <th>Equipo</th>
              <th>Destinatario</th>
              <th>Mensajero</th>
              <th>Fecha Creación</th>
              <th>Fecha Firma</th>
              <th>Estado</th>
            </tr>
          </thead>
        )}

        {tab === "entregas" && (
          <thead>
            <tr>
              <th>N° Acta</th>
              <th>Equipo</th>
              <th>Mensajero</th>
              <th>Destinatario</th>
              <th>Fecha Creación</th>
              <th>Fecha Firma</th>
              <th>Firma Certificada</th>
              <th>Estado</th>
            </tr>
          </thead>
        )}

        {tab === "devoluciones" && (
          <thead>
            <tr>
              <th>N° Acta Devolución</th>
              <th>Acta Origen</th>
              <th>Equipo</th>
              <th>Responsable</th>
              <th>Fecha Creación</th>
              <th>Fecha Firma</th>
              <th>Estado</th>
            </tr>
          </thead>
        )}

        {/* =====================================================
            BODY POR PESTAÑA
        ====================================================== */}
        <tbody>
          {tab === "equipos" &&
            (paginatedData as EquiposReportItem[]).map((item) => (
              <tr key={item.id}>
                <td><strong className="equipment-key">{item.inventoryKey}</strong></td>
                <td>{item.nombreEquipo}</td>
                <td>{item.marca || "—"}</td>
                <td>{item.asignadoA || "—"}</td>
                <td>{item.departamento || "—"}</td>
                <td>{item.empresa || "—"}</td>
                <td>{item.proveedor || "—"}</td>
                <td>
                  <span className={item.estatusEquipo?.toLowerCase() === "activo" ? "status-badge status-active" : item.estatusEquipo?.toLowerCase() === "asignado" ? "status-badge status-warning" : "status-badge status-inactive"}>
                    {item.estatusEquipo || "Sin estado"}
                  </span>
                </td>
              </tr>
            ))}

          {tab === "actas" &&
            (paginatedData as ActasReportItem[]).map((item) => (
              <tr key={item.id}>
                <td><strong>{item.numeroActa}</strong></td>
                <td><span className="badge-tipo">{item.tipoActa}</span></td>
                <td>{item.equipmentNombre || item.equipmentInventoryKey || "—"}</td>
                <td>{item.destinatarioNombre || "—"}</td>
                <td>{item.mensajeroNombre || "Sin asignar"}</td>
                <td>{formatDate(item.fechaCreacion)}</td>
                <td>{formatDate(item.fechaFirma)}</td>
                <td>
                  <span className={`acta-status-badge acta-status-${item.estado.toLowerCase()}`}>
                    {item.estado}
                  </span>
                </td>
              </tr>
            ))}

          {tab === "entregas" &&
            (paginatedData as EntregasReportItem[]).map((item) => (
              <tr key={item.id}>
                <td><strong>{item.numeroActa}</strong></td>
                <td>{item.equipmentNombre} ({item.equipmentInventoryKey})</td>
                <td>{item.mensajeroNombre || "Sin asignar"}</td>
                <td>{item.destinatarioNombre || "—"}</td>
                <td>{formatDate(item.fechaCreacion)}</td>
                <td>{formatDate(item.fechaFirma)}</td>
                <td>
                  <span style={{ color: item.firmado ? "#059669" : "#d97706", fontWeight: 700 }}>
                    {item.firmado ? "✓ Firmada" : "⏳ Pendiente"}
                  </span>
                </td>
                <td>
                  <span className={`acta-status-badge acta-status-${item.estado.toLowerCase()}`}>
                    {item.estado}
                  </span>
                </td>
              </tr>
            ))}

          {tab === "devoluciones" &&
            (paginatedData as DevolucionesReportItem[]).map((item) => (
              <tr key={item.id}>
                <td><strong>{item.numeroActa}</strong></td>
                <td>{item.actaOrigenNumero || (item.actaOrigenId ? "Acta vinculada" : "—")}</td>
                <td>{item.equipmentNombre} ({item.equipmentInventoryKey})</td>
                <td>{item.responsableEntrega || "Bodega / Custodia"}</td>
                <td>{formatDate(item.fechaCreacion)}</td>
                <td>{formatDate(item.fechaFirma)}</td>
                <td>
                  <span className={`acta-status-badge acta-status-${item.estado.toLowerCase()}`}>
                    {item.estado}
                  </span>
                </td>
              </tr>
            ))}
        </tbody>
      </table>

      {/* PAGINACIÓN */}
      <footer className="inventory-pagination" style={{ padding: "12px 16px" }}>
        <div>
          Mostrando <strong>{firstItem}-{lastItem}</strong> de <strong>{data.length}</strong> registros
        </div>
        <div className="inventory-pagination-controls">
          <button type="button" onClick={() => setCurrentPage((p) => Math.max(1, p - 1))} disabled={currentPage === 1}>
            ←
          </button>
          <span>
            {currentPage} <small>/ {totalPages}</small>
          </span>
          <button type="button" onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages}>
            →
          </button>
        </div>
      </footer>
    </div>
  );
}
