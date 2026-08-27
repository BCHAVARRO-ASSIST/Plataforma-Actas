import type { Acta } from "../types/acta";

interface ActaTableProps {
  actas: Acta[];
  onSelect: (id: string) => void;
}

function getStatusClass(status: string) {
  switch (status.toLowerCase()) {
    case "borrador":
      return "acta-status-draft";

    case "finalizada":
      return "acta-status-finished";

    case "anulada":
      return "acta-status-cancelled";

    default:
      return "acta-status-default";
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString(
    "es-CO",
    {
      day: "2-digit",
      month: "short",
      year: "numeric",
    }
  );
}

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString(
    "es-CO",
    {
      hour: "2-digit",
      minute: "2-digit",
    }
  );
}

export function ActaTable({
  actas,
  onSelect,
}: ActaTableProps) {
  return (
    <div className="actas-table-wrapper">
      <table className="actas-table">
        <thead>
          <tr>
            <th>Acta</th>
            <th>Equipo</th>
            <th>Estado</th>
            <th>Entrega</th>
            <th>Fecha</th>
            <th>Acción</th>
          </tr>
        </thead>

        <tbody>
          {actas.map((acta) => (
            <tr key={acta.id}>

              {/* ACTA */}

              <td>
                <div className="acta-identity">

                  <div className="acta-icon">
                    A
                  </div>

                  <div>
                    <strong>
                      {acta.numeroActa}
                    </strong>

                    <span>
                      {acta.tipoActa}
                    </span>
                  </div>

                </div>
              </td>

              {/* EQUIPO */}

              <td>
                <div className="acta-equipment">

                  <strong>
                    {acta.inventoryKey ??
                      "Sin inventario"}
                  </strong>

                  <span>
                    {acta.nombreEquipo ??
                      "Equipo sin nombre"}
                  </span>

                </div>
              </td>


              {/* ESTADO */}

              <td>
                <span
                  className={`acta-status ${getStatusClass(
                    acta.estado
                  )}`}
                >
                  <span />

                  {acta.estado}
                </span>
              </td>

              {/* ENTREGA */}

              <td>
                <span
                  className={
                    acta.actaEntregada
                      ? "acta-delivery delivered"
                      : "acta-delivery pending"
                  }
                >
                  <span />

                  {acta.actaEntregada
                    ? "Entregada"
                    : "Pendiente"}
                </span>
              </td>

              {/* FECHA */}

              <td>
                <div className="acta-date">

                  <strong>
                    {formatDate(
                      acta.fechaCreacion
                    )}
                  </strong>

                  <span>
                    {formatTime(
                      acta.fechaCreacion
                    )}
                  </span>

                </div>
              </td>

              {/* ACCIÓN */}

              <td>
                <button
                  type="button"
                  className="acta-view-button"
                  onClick={() =>
                    onSelect(acta.id)
                  }
                >
                  Ver detalle

                  <span>
                    →
                  </span>
                </button>
              </td>

            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}