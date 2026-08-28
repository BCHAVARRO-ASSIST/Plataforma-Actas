import {
  useEffect,
  useMemo,
  useState,
} from "react";

import type {
  MensajeroActa,
} from "./mensajeroService";

import {
  getMisActas,
} from "./mensajeroService";

import {
  MensajeroActaTable,
} from "./components/MensajeroActaTable";

import "./components/mensajero.css";


// =====================================================
// PROPS
// =====================================================

interface MensajeroPageProps {
  onSelectActa: (
    id: string
  ) => void;
}


// =====================================================
// PÁGINA DEL MENSAJERO
// =====================================================

export function MensajeroPage({
  onSelectActa,
}: MensajeroPageProps) {

  const [
    actas,
    setActas,
  ] = useState<MensajeroActa[]>([]);

  const [
    loading,
    setLoading,
  ] = useState(true);

  const [
    error,
    setError,
  ] = useState<string | null>(null);


  // ===================================================
  // CARGAR ACTAS
  // ===================================================

  async function loadActas() {

    try {

      setLoading(true);
      setError(null);

      const data =
        await getMisActas();

      setActas(data);

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible cargar las actas."
      );

    } finally {

      setLoading(false);

    }
  }


  // ===================================================
  // CARGA INICIAL
  // ===================================================

  useEffect(() => {

    let mounted = true;

    async function load() {

      try {

        setLoading(true);
        setError(null);

        const data =
          await getMisActas();

        if (mounted) {
          setActas(data);
        }

      } catch (err) {

        if (mounted) {

          setError(
            err instanceof Error
              ? err.message
              : "No fue posible cargar las actas."
          );

        }

      } finally {

        if (mounted) {
          setLoading(false);
        }

      }

    }

    load();

    return () => {
      mounted = false;
    };

  }, []);


  // ===================================================
  // ESTADÍSTICAS
  // ===================================================

  const statistics =
    useMemo(() => {

      const total =
        actas.length;


      const pendientes =
        actas.filter(
          (acta) =>
            !acta.actaEntregada
        ).length;


      const entregadas =
        actas.filter(
          (acta) =>
            acta.actaEntregada
        ).length;


      const entregas =
        actas.filter(
          (acta) =>
            acta.tipoActa === "Entrega"
        ).length;


      const devoluciones =
        actas.filter(
          (acta) =>
            acta.tipoActa === "Devolución"
        ).length;


      return {
        total,
        pendientes,
        entregadas,
        entregas,
        devoluciones,
      };

    }, [actas]);


  const operaciones =
    statistics.entregas +
    statistics.devoluciones;


  // ===================================================
  // RENDER
  // ===================================================

  return (
    <main className="mensajero-page">


      {/* =================================================
          CABECERA PRINCIPAL
          ================================================= */}

      <header className="mensajero-page-header">

        <div className="mensajero-page-heading">

          <span className="mensajero-page-eyebrow">
            OPERACIÓN DE CAMPO Y MENSAJERÍA
          </span>

          <h1>Mis Actas Asignadas y Certificación de Entregas</h1>

          <p>
            Gestiona tus traslados asignados, consulta los detalles del equipo y destinatario, y recolecta la firma manuscrita digital para certificar la entrega.
          </p>

        </div>

        <div className="mensajero-page-actions">

          <div className="mensajero-live-status">

            <span className="mensajero-live-dot" />

            Turno operativo activo

          </div>

          <button
            type="button"
            className="mensajero-refresh-button"
            onClick={loadActas}
            disabled={loading}
          >

            <span
              className={
                loading
                  ? "mensajero-refresh-icon is-loading"
                  : "mensajero-refresh-icon"
              }
            >
              ↻
            </span>

            {loading
              ? "Actualizando..."
              : "Actualizar"}

          </button>

        </div>

      </header>





      {/* =================================================
          RESUMEN OPERATIVO
          ================================================= */}

      <section
        className="mensajero-summary"
        aria-label="Resumen operativo"
      >

        <div className="mensajero-summary-heading">

          <span>
            RESUMEN
          </span>

          <strong>
            Actividad de tus actas
          </strong>

        </div>


        <div className="mensajero-kpis">


          {/* TOTAL */}

          <article
            className="
              mensajero-kpi
              mensajero-kpi-primary
            "
          >

            <div className="mensajero-kpi-top">

              <span className="mensajero-kpi-label">
                Total
              </span>

              <span className="mensajero-kpi-marker" />

            </div>


            <strong className="mensajero-kpi-value">
              {statistics.total}
            </strong>


            <span className="mensajero-kpi-description">
              Actas asignadas
            </span>

          </article>


          {/* PENDIENTES */}

          <article
            className="
              mensajero-kpi
              mensajero-kpi-warning
            "
          >

            <div className="mensajero-kpi-top">

              <span className="mensajero-kpi-label">
                Pendientes
              </span>

              <span className="mensajero-kpi-marker" />

            </div>


            <strong className="mensajero-kpi-value">
              {statistics.pendientes}
            </strong>


            <span className="mensajero-kpi-description">
              Requieren atención
            </span>

          </article>


          {/* ENTREGADAS */}

          <article
            className="
              mensajero-kpi
              mensajero-kpi-success
            "
          >

            <div className="mensajero-kpi-top">

              <span className="mensajero-kpi-label">
                Entregadas
              </span>

              <span className="mensajero-kpi-marker" />

            </div>


            <strong className="mensajero-kpi-value">
              {statistics.entregadas}
            </strong>


            <span className="mensajero-kpi-description">
              Procesos completados
            </span>

          </article>


          {/* OPERACIONES */}

          <article
            className="
              mensajero-kpi
              mensajero-kpi-neutral
            "
          >

            <div className="mensajero-kpi-top">

              <span className="mensajero-kpi-label">
                Operaciones
              </span>

              <span className="mensajero-kpi-marker" />

            </div>


            <strong className="mensajero-kpi-value">
              {operaciones}
            </strong>


            <span className="mensajero-kpi-description">

              {statistics.entregas}{" "}
              entregas ·{" "}
              {statistics.devoluciones}{" "}
              devoluciones

            </span>

          </article>

        </div>

      </section>


      {/* =================================================
          ACTAS ASIGNADAS
          ================================================= */}

      <section className="mensajero-assignments">


        {/* =================================================
            CABECERA DE LA SECCIÓN
            ================================================= */}

        <header className="mensajero-section-header">

          <div className="mensajero-section-heading">

            <span className="mensajero-section-kicker">
              GESTIÓN DE ACTAS
            </span>


            <div className="mensajero-section-title-row">

              <h2>
                Actas asignadas
              </h2>


              <span className="mensajero-count">
                {statistics.total}
              </span>

            </div>


            <p>
              Revisa el estado y detalle de cada acta.
            </p>

          </div>


          {/* INDICADOR DE PENDIENTES */}

          <div className="mensajero-section-meta">

            <span className="mensajero-meta-dot" />

            <span>

              {statistics.pendientes}{" "}

              {statistics.pendientes === 1
                ? "pendiente"
                : "pendientes"}

            </span>

          </div>

        </header>


        {/* =================================================
            CONTENEDOR DE CONTENIDO
            ================================================= */}

        <div className="mensajero-table-shell">


          {/* =================================================
              CARGANDO
              ================================================= */}

          {loading && (

            <div className="mensajero-state">

              <div className="mensajero-loading-indicator">

                <span />
                <span />
                <span />

              </div>


              <div className="mensajero-state-content">

                <strong>
                  Cargando actas
                </strong>

                <p>
                  Consultando las asignaciones disponibles.
                </p>

              </div>

            </div>

          )}


          {/* =================================================
              ERROR
              ================================================= */}

          {!loading &&
            error && (

              <div
                className="
                  mensajero-state
                  mensajero-state-error
                "
              >

                <div className="mensajero-state-symbol">
                  !
                </div>


                <div className="mensajero-state-content">

                  <strong>
                    No fue posible cargar las actas
                  </strong>


                  <p>
                    {error}
                  </p>


                  <button
                    type="button"
                    className="mensajero-secondary-button"
                    onClick={loadActas}
                  >
                    Intentar nuevamente
                  </button>

                </div>

              </div>

            )}


          {/* =================================================
              SIN ACTAS
              ================================================= */}

          {!loading &&
            !error &&
            actas.length === 0 && (

              <div
                className="
                  mensajero-state
                  mensajero-state-empty
                "
              >

                <div className="mensajero-empty-graphic">

                  <span />

                </div>


                <div className="mensajero-state-content">

                  <strong>
                    No tienes actas asignadas
                  </strong>


                  <p>
                    Cuando se asigne una acta a tu usuario,
                    aparecerá automáticamente en esta sección.
                  </p>

                </div>

              </div>

            )}


          {/* =================================================
              TABLA
              ================================================= */}

          {!loading &&
            !error &&
            actas.length > 0 && (

              <MensajeroActaTable
                actas={actas}
                onSelect={onSelectActa}
              />

            )}

        </div>

      </section>

    </main>
  );
}

