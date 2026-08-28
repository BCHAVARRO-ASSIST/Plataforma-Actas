import type {
  MensajeroActa,
} from "../mensajeroService";


// =====================================================
// PROPS
// =====================================================

interface MensajeroActaTableProps {
  actas: MensajeroActa[];

  onSelect: (
    id: string
  ) => void;
}


// =====================================================
// FECHA
// =====================================================

function formatDate(
  value: string
): {
  date: string;
  time: string;
} {

  const date =
    new Date(value);

  if (
    Number.isNaN(
      date.getTime()
    )
  ) {

    return {
      date: "Fecha no disponible",
      time: "",
    };

  }

  return {
    date: date.toLocaleDateString(
      "es-CO",
      {
        day: "2-digit",
        month: "short",
        year: "numeric",
      }
    ),

    time: date.toLocaleTimeString(
      "es-CO",
      {
        hour: "2-digit",
        minute: "2-digit",
      }
    ),
  };

}


// =====================================================
// ESTADO
// =====================================================

function getStatusClass(
  estado: string
): string {

  const normalized =
    estado
      .toLowerCase()
      .normalize("NFD")
      .replace(
        /[\u0300-\u036f]/g,
        ""
      );

  switch (normalized) {

    case "borrador":
      return "mensajero-status-borrador";

    case "firmada":
      return "mensajero-status-firmada";

    case "entregada":
      return "mensajero-status-entregada";

    case "finalizada":
      return "mensajero-status-finalizada";

    case "anulada":
      return "mensajero-status-anulada";

    default:
      return "mensajero-status-borrador";

  }

}


// =====================================================
// TIPO DE ACTA
// =====================================================

function getActaTypeClass(
  tipoActa: string
): string {

  return tipoActa === "Entrega"
    ? "mensajero-acta-type-entrega"
    : "mensajero-acta-type-devolucion";

}


// =====================================================
// COMPONENTE
// =====================================================

export function MensajeroActaTable({
  actas,
  onSelect,
}: MensajeroActaTableProps) {

  return (

    <div className="mensajero-actas-list">

      {/* =================================================
          ENCABEZADO
      ================================================= */}

      <div
        className="mensajero-acta-row mensajero-acta-header"
        aria-hidden="true"
      >

        <div className="mensajero-header-acta">
          <span className="mensajero-acta-info-label">
            Acta
          </span>
        </div>


        <div className="mensajero-header-equipo">
          <span className="mensajero-acta-info-label">
            Equipo
          </span>
        </div>


        <div className="mensajero-header-estado">
          <span className="mensajero-acta-info-label">
            Estado
          </span>
        </div>


        <div className="mensajero-header-entrega">
          <span className="mensajero-acta-info-label">
            Entrega
          </span>
        </div>


        <div className="mensajero-header-accion">
          <span className="mensajero-acta-info-label">
            Acción
          </span>
        </div>

      </div>


      {/* =================================================
          ACTAS
      ================================================= */}

      {actas.map(
        (acta) => {

          const fecha =
            formatDate(
              acta.fechaCreacion
            );


          const typeClass =
            getActaTypeClass(
              acta.tipoActa
            );


          return (

            <article
              key={acta.id}
              className="mensajero-acta-row mensajero-acta-data-row"
            >

              {/* =========================================
                  ACTA
              ========================================= */}

              <div className="mensajero-acta-main">

                <div
                  className={`mensajero-acta-icon ${typeClass}`}
                  aria-hidden="true"
                >

                  <span className="mensajero-acta-icon-line" />
                  <span className="mensajero-acta-icon-line" />
                  <span className="mensajero-acta-icon-line" />

                </div>


                <div className="mensajero-acta-main-content">

                  <strong className="mensajero-acta-number">
                    {acta.numeroActa}
                  </strong>


                  <span className="mensajero-acta-type">
                    {acta.tipoActa}
                  </span>

                </div>

              </div>


              {/* =========================================
                  EQUIPO
              ========================================= */}

              <div className="mensajero-acta-info">

                <span className="mensajero-acta-info-label">
                  Inventario
                </span>


                <strong
                  className="mensajero-acta-info-value mensajero-acta-inventory"
                  title={acta.inventoryKey ?? undefined}
                >
                  {acta.inventoryKey ?? "Sin inventario"}
                </strong>


                {acta.nombreEquipo && (

                  <span
                    className="mensajero-acta-info-value mensajero-acta-equipment"
                    title={acta.nombreEquipo ?? undefined}
                  >
                    {acta.nombreEquipo}
                  </span>

                )}


              </div>


              {/* =========================================
                  ESTADO
              ========================================= */}

              <div className="mensajero-acta-status-cell">

                <span
                  className={`mensajero-status ${getStatusClass(
                    acta.estado
                  )}`}
                >

                  <span
                    className="mensajero-status-dot"
                    aria-hidden="true"
                  />

                  {acta.estado}

                </span>

              </div>


              {/* =========================================
                  ENTREGA
              ========================================= */}

              <div className="mensajero-acta-delivery-cell">

                <span
                  className={`mensajero-delivery ${
                    acta.actaEntregada
                      ? "mensajero-delivery-complete"
                      : "mensajero-delivery-pending"
                  }`}
                >

                  <span
                    className="mensajero-delivery-dot"
                    aria-hidden="true"
                  />

                  {acta.actaEntregada
                    ? "Entregada"
                    : "Pendiente"}

                </span>


                <div className="mensajero-date">

                  <strong>
                    {fecha.date}
                  </strong>

                  <span>
                    {fecha.time}
                  </span>

                </div>

              </div>


              {/* =========================================
                  ACCIÓN
              ========================================= */}

              <div className="mensajero-acta-action">

                <button
                  type="button"
                  className="mensajero-detail-button"
                  onClick={() =>
                    onSelect(acta.id)
                  }
                  aria-label={`Ver detalle del acta ${acta.numeroActa}`}
                >

                  <span className="mensajero-detail-button-text">
                    Ver detalle
                  </span>

                  <span
                    className="mensajero-detail-arrow"
                    aria-hidden="true"
                  >
                    â†’
                  </span>

                </button>

              </div>

            </article>

          );

        }
      )}

    </div>

  );

}
