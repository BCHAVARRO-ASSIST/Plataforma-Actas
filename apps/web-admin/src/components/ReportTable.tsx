import { useState, useMemo } from "react";
import type {
  ReportTab,
  EquiposReportItem,
  ActasReportItem,
  EntregasReportItem,
  DevolucionesReportItem,
} from "../features/reportes/reportesService";
import { UserIcon } from "./Icons";

interface ReportTableProps {
  tab: ReportTab;
  data: EquiposReportItem[] | ActasReportItem[] | EntregasReportItem[] | DevolucionesReportItem[];
}

const ITEMS_PER_PAGE = 15;

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
      <div style={{ padding: "48px 20px", textAlign: "center", color: "var(--text-muted)" }}>
        <p style={{ margin: "0 0 6px", fontWeight: 600, fontSize: "14px", color: "var(--text-primary)" }}>
          No se encontraron registros con los filtros seleccionados.
        </p>
        <span style={{ fontSize: "13px" }}>Modifica las fechas o términos de búsqueda para ver más información.</span>
      </div>
    );
  }

  const firstItem = (currentPage - 1) * ITEMS_PER_PAGE + 1;
  const lastItem = Math.min(currentPage * ITEMS_PER_PAGE, data.length);

  return (
    <div>
      <div className="table-wrapper" style={{ border: "none", borderRadius: 0, boxShadow: "none" }}>
        <table className="data-table">
          {/* Header por Pestaña */}
          {tab === "equipos" && (
            <thead>
              <tr>
                <th>Clave Inventario</th>
                <th>Nombre Equipo</th>
                <th>Marca</th>
                <th>Asignado a</th>
                <th>Departamento</th>
                <th>Empresa</th>
                <th>Proveedor</th>
                <th style={{ textAlign: "right" }}>Estado</th>
              </tr>
            </thead>
          )}

          {tab === "actas" && (
            <thead>
              <tr>
                <th>N° Acta</th>
                <th>Tipo</th>
                <th>Equipo Asociado</th>
                <th>Destinatario</th>
                <th>Mensajero / Entrega</th>
                <th>Fecha Emisión</th>
                <th>Fecha Firma</th>
                <th style={{ textAlign: "right" }}>Estado Legal</th>
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
                <th style={{ textAlign: "right" }}>Estado</th>
              </tr>
            </thead>
          )}

          {tab === "devoluciones" && (
            <thead>
              <tr>
                <th>N° Devolución</th>
                <th>Acta Origen</th>
                <th>Equipo</th>
                <th>Responsable</th>
                <th>Fecha Creación</th>
                <th>Fecha Firma</th>
                <th style={{ textAlign: "right" }}>Estado</th>
              </tr>
            </thead>
          )}

          {/* Body por Pestaña */}
          <tbody>
            {tab === "equipos" &&
              (paginatedData as EquiposReportItem[]).map((item) => {
                const isActivo = item.estatusEquipo?.toLowerCase() === "activo";
                const isAsignado = item.estatusEquipo?.toLowerCase() === "asignado";
                return (
                  <tr key={item.id}>
                    <td className="primary-cell">
                      <span style={{ fontFamily: "var(--font-mono)", color: "var(--color-primary-700)", fontWeight: 700 }}>
                        {item.inventoryKey}
                      </span>
                    </td>
                    <td>
                      <strong style={{ color: "var(--text-primary)" }}>{item.nombreEquipo}</strong>
                    </td>
                    <td>{item.marca || "—"}</td>
                    <td>
                      {item.asignadoA ? (
                        <span style={{ display: "inline-flex", alignItems: "center", gap: "5px" }}>
                          <UserIcon size={13} style={{ color: "var(--text-muted)" }} />
                          <span>{item.asignadoA}</span>
                        </span>
                      ) : "—"}
                    </td>
                    <td>{item.departamento || "—"}</td>
                    <td>{item.empresa || "—"}</td>
                    <td>{item.proveedor || "—"}</td>
                    <td style={{ textAlign: "right" }}>
                      <span className={isActivo ? "badge badge-success" : isAsignado ? "badge badge-info" : "badge badge-neutral"}>
                        <span className="badge-dot" />
                        {item.estatusEquipo || "Sin estado"}
                      </span>
                    </td>
                  </tr>
                );
              })}

            {tab === "actas" &&
              (paginatedData as ActasReportItem[]).map((item) => {
                const isFirmada = item.estado.toLowerCase() === "firmada" || item.estado.toLowerCase() === "finalizada";
                const isAnulada = item.estado.toLowerCase() === "anulada";
                return (
                  <tr key={item.id}>
                    <td className="primary-cell">
                      <span style={{ fontFamily: "var(--font-mono)", color: "var(--color-primary-700)", fontWeight: 700 }}>
                        {item.numeroActa}
                      </span>
                    </td>
                    <td>
                      <span className="badge badge-neutral">{item.tipoActa}</span>
                    </td>
                    <td>{item.equipmentNombre || item.equipmentInventoryKey || "—"}</td>
                    <td>{item.destinatarioNombre || "—"}</td>
                    <td>{item.mensajeroNombre || "Sin asignar"}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaCreacion)}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaFirma)}</td>
                    <td style={{ textAlign: "right" }}>
                      <span className={isFirmada ? "badge badge-success" : isAnulada ? "badge badge-danger" : "badge badge-warning"}>
                        <span className="badge-dot" />
                        {item.estado}
                      </span>
                    </td>
                  </tr>
                );
              })}

            {tab === "entregas" &&
              (paginatedData as EntregasReportItem[]).map((item) => {
                const isFirmada = item.estado.toLowerCase() === "firmada" || item.estado.toLowerCase() === "finalizada";
                const isAnulada = item.estado.toLowerCase() === "anulada";
                return (
                  <tr key={item.id}>
                    <td className="primary-cell">
                      <span style={{ fontFamily: "var(--font-mono)", color: "var(--color-primary-700)", fontWeight: 700 }}>
                        {item.numeroActa}
                      </span>
                    </td>
                    <td>
                      <strong style={{ color: "var(--text-primary)" }}>{item.equipmentNombre}</strong>
                      <span style={{ fontSize: "11px", color: "var(--text-muted)", display: "block", fontFamily: "var(--font-mono)" }}>
                        {item.equipmentInventoryKey}
                      </span>
                    </td>
                    <td>{item.mensajeroNombre || "Sin asignar"}</td>
                    <td>{item.destinatarioNombre || "—"}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaCreacion)}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaFirma)}</td>
                    <td>
                      <span className={item.firmado ? "badge badge-success" : "badge badge-warning"}>
                        <span className="badge-dot" />
                        {item.firmado ? "Certificada" : "Pendiente"}
                      </span>
                    </td>
                    <td style={{ textAlign: "right" }}>
                      <span className={isFirmada ? "badge badge-success" : isAnulada ? "badge badge-danger" : "badge badge-warning"}>
                        <span className="badge-dot" />
                        {item.estado}
                      </span>
                    </td>
                  </tr>
                );
              })}

            {tab === "devoluciones" &&
              (paginatedData as DevolucionesReportItem[]).map((item) => {
                const isFirmada = item.estado.toLowerCase() === "firmada" || item.estado.toLowerCase() === "finalizada";
                const isAnulada = item.estado.toLowerCase() === "anulada";
                return (
                  <tr key={item.id}>
                    <td className="primary-cell">
                      <span style={{ fontFamily: "var(--font-mono)", color: "var(--color-primary-700)", fontWeight: 700 }}>
                        {item.numeroActa}
                      </span>
                    </td>
                    <td>{item.actaOrigenNumero || (item.actaOrigenId ? "Acta vinculada" : "—")}</td>
                    <td>
                      <strong style={{ color: "var(--text-primary)" }}>{item.equipmentNombre}</strong>
                      <span style={{ fontSize: "11px", color: "var(--text-muted)", display: "block", fontFamily: "var(--font-mono)" }}>
                        {item.equipmentInventoryKey}
                      </span>
                    </td>
                    <td>{item.responsableEntrega || "Bodega / Custodia"}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaCreacion)}</td>
                    <td style={{ fontSize: "12.5px" }}>{formatDate(item.fechaFirma)}</td>
                    <td style={{ textAlign: "right" }}>
                      <span className={isFirmada ? "badge badge-success" : isAnulada ? "badge badge-danger" : "badge badge-warning"}>
                        <span className="badge-dot" />
                        {item.estado}
                      </span>
                    </td>
                  </tr>
                );
              })}
          </tbody>
        </table>
      </div>

      {/* Paginación */}
      {totalPages > 1 && (
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "16px 20px", borderTop: "1px solid var(--border-subtle)" }}>
          <span style={{ fontSize: "12.5px", color: "var(--text-muted)" }}>
            Mostrando <strong>{firstItem}-{lastItem}</strong> de <strong>{data.length}</strong> registros
          </span>
          <div style={{ display: "flex", gap: "6px" }}>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              disabled={currentPage === 1}
            >
              ← Anterior
            </button>
            <span style={{ padding: "6px 12px", fontSize: "12.5px", fontWeight: 600, color: "var(--text-secondary)" }}>
              Página {currentPage} de {totalPages}
            </span>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
            >
              Siguiente →
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
