import { ArrowRightIcon } from "@/components/ui/Icons";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { formatDateParts } from "@/shared/utils/date";

export type BaseActa = {
  id: string;
  numeroActa: string;
  tipoActa: string;
  nombreEquipo: string | null;
  inventoryKey: string | null;
  estado: string;
  actaEntregada: boolean;
  fechaCreacion: string;
};

interface ActaTableProps {
  actas: BaseActa[];
  onSelect: (id: string) => void;
  variant?: "admin" | "mensajero";
}

export function ActaTable({ actas, onSelect, variant = "admin" }: ActaTableProps) {
  const isMensajero = variant === "mensajero";

  return (
    <div className="table-wrapper">
      <table className="data-table">
        <thead>
          <tr>
            <th>{isMensajero ? "Acta / Traslado" : "Número de Acta"}</th>
            <th>Tipo</th>
            <th>{isMensajero ? "Equipo de Hardware" : "Equipo Asociado"}</th>
            <th>{isMensajero ? "Estado" : "Estado Legal"}</th>
            <th>{isMensajero ? "Entrega" : "Entrega Física"}</th>
            <th>{isMensajero ? "Fecha" : "Fecha de Emisión"}</th>
            <th style={{ textAlign: "right" }}>Acciones</th>
          </tr>
        </thead>

        <tbody>
          {actas.length === 0 ? (
            <tr>
              <td colSpan={7} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
                {isMensajero
                  ? "No tienes actas de entrega asignadas en este momento."
                  : "No se encontraron actas registradas."}
              </td>
            </tr>
          ) : (
            actas.map((acta) => {
              const formatted = formatDateParts(acta.fechaCreacion);

              return (
                <tr key={acta.id}>
                  <td className="primary-cell">
                    <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--color-primary-700)', fontWeight: 700 }}>
                      {acta.numeroActa || (isMensajero ? "ACT-ASIGNADA" : "ACT-PENDIENTE")}
                    </span>
                  </td>

                  <td>
                    <span className="badge badge-neutral">
                      {acta.tipoActa}
                    </span>
                  </td>

                  <td>
                    <strong style={{ color: "var(--text-primary)", display: "block" }}>{acta.nombreEquipo ?? "Equipo sin asignar"}</strong>
                    <span style={{ fontSize: "11.5px", color: "var(--text-muted)", fontFamily: 'var(--font-mono)' }}>{acta.inventoryKey ?? "—"}</span>
                  </td>

                  <td>
                    <StatusBadge status={acta.estado} />
                  </td>

                  <td>
                    <StatusBadge
                      status={acta.actaEntregada ? "Entregada" : "Pendiente"}
                      variant={acta.actaEntregada ? "success" : "neutral"}
                    />
                  </td>

                  <td>
                    <div style={{ fontSize: "12.5px", color: "var(--text-primary)", fontWeight: 500 }}>
                      {formatted.date}
                    </div>
                    <div style={{ fontSize: "11px", color: "var(--text-muted)" }}>
                      {formatted.time}
                    </div>
                  </td>

                  <td style={{ textAlign: "right" }}>
                    <button
                      type="button"
                      className={`btn btn-sm ${isMensajero ? "btn-primary" : "btn-secondary"}`}
                      onClick={() => onSelect(acta.id)}
                      style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
                    >
                      <span>{isMensajero ? "Gestionar entrega" : "Ver detalle"}</span>
                      <ArrowRightIcon size={isMensajero ? 14 : 13} />
                    </button>
                  </td>
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}
