import type { RecentActivityItem } from "../features/dashboard/dashboardService";
import { UserIcon, ArrowRightIcon } from "./Icons";

interface RecentActasCardProps {
  items: RecentActivityItem[];
  onNavigateToActas: () => void;
}

function formatDate(val: string): string {
  try {
    return new Date(val).toLocaleDateString("es-CO", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return val;
  }
}

export function RecentActasCard({ items, onNavigateToActas }: RecentActasCardProps) {
  return (
    <div className="card" style={{ marginTop: '24px' }}>
      {/* Header */}
      <div className="card-header">
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            TRAZABILIDAD Y REGISTRO
          </span>
          <h2 style={{ margin: "2px 0 0", fontSize: "16px", fontWeight: 700, color: "var(--text-primary)" }}>
            Últimas Actas Emitidas
          </h2>
        </div>

        <button
          type="button"
          onClick={onNavigateToActas}
          className="btn btn-ghost btn-sm"
          style={{ color: 'var(--color-primary-600)', fontWeight: 600, display: 'inline-flex', alignItems: 'center', gap: '6px' }}
        >
          <span>Ver todas las actas</span>
          <ArrowRightIcon size={14} />
        </button>
      </div>

      {/* Table */}
      {items.length === 0 ? (
        <div style={{ padding: "36px 20px", textAlign: "center", color: "var(--text-muted)", fontSize: "13.5px" }}>
          No hay actas registradas recientemente en el sistema.
        </div>
      ) : (
        <div className="table-wrapper" style={{ border: 'none', borderRadius: 0, boxShadow: 'none' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>Documento</th>
                <th>Detalle Hardware</th>
                <th>Responsable</th>
                <th>Fecha</th>
                <th style={{ textAlign: "right" }}>Estado</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => {
                const isFirmada = item.estado.toLowerCase() === "firmada" || item.estado.toLowerCase() === "finalizada";
                const isAnulada = item.estado.toLowerCase() === "anulada";

                return (
                  <tr key={item.id}>
                    <td className="primary-cell">
                      <span>{item.titulo}</span>
                      <span style={{ fontSize: "11px", color: "var(--text-muted)", display: "block", fontWeight: 400 }}>{item.tipoEvento}</span>
                    </td>

                    <td>{item.descripcion}</td>

                    <td style={{ fontWeight: 500 }}>
                      <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                        <UserIcon size={14} style={{ color: 'var(--text-muted)' }} />
                        {item.responsable || "Sistema"}
                      </span>
                    </td>

                    <td style={{ fontSize: "12px", whiteSpace: "nowrap" }}>
                      {formatDate(item.fecha)}
                    </td>

                    <td style={{ textAlign: "right" }}>
                      <span
                        className={
                          isFirmada
                            ? "badge badge-success"
                            : isAnulada
                            ? "badge badge-danger"
                            : "badge badge-warning"
                        }
                      >
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
      )}
    </div>
  );
}
