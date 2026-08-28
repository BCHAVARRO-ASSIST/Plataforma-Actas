import {
  useEffect,
  useState,
} from "react";

import type {
  MensajeroActaDetail as MensajeroActaDetailType,
} from "../mensajeroService";

import {
  getMiActaById,
} from "../mensajeroService";

import {
  firmarActa,
} from "../../actas/actaService";

import {
  FirmaManuscrita,
} from "../../actas/components/FirmaManuscrita";

import {
  OfficialActaModal,
} from "../../actas/components/OfficialActaModal";

import "./mensajero.css";




// =====================================================
// PROPS
// =====================================================

interface MensajeroActaDetailProps {
  actaId: string;

  onBack: () => void;
}


// =====================================================
// FECHA
// =====================================================

function formatDate(
  value: string | null
): string {

  if (!value) {
    return "Pendiente";
  }

  const date =
    new Date(value);

  if (
    Number.isNaN(
      date.getTime()
    )
  ) {
    return "Fecha no disponible";
  }

  return date.toLocaleString(
    "es-CO",
    {
      dateStyle: "medium",
      timeStyle: "short",
    }
  );
}


// =====================================================
// OBSERVACIÃ“N
// =====================================================

function getObservation(
  formDataJson: string
): string {

  if (!formDataJson) {
    return "Sin observaciones registradas.";
  }

  try {

    const data =
      JSON.parse(
        formDataJson
      );

    return (
      data.observacion ??
      data.observaciones ??
      "Sin observaciones registradas."
    );

  } catch {

    return "Sin observaciones registradas.";

  }

}


// =====================================================
// CLASE DEL ESTADO
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
      return "mensajero-detail-status-borrador";

    case "firmada":
      return "mensajero-detail-status-firmada";

    case "entregada":
      return "mensajero-detail-status-entregada";

    case "finalizada":
      return "mensajero-detail-status-finalizada";

    case "anulada":
      return "mensajero-detail-status-anulada";

    default:
      return "mensajero-detail-status-borrador";

  }

}


// =====================================================
// COMPONENTE
// =====================================================

export function MensajeroActaDetail({
  actaId,
  onBack,
}: MensajeroActaDetailProps) {

  const [
    acta,
    setActa,
  ] =
    useState<MensajeroActaDetailType | null>(
      null
    );


  const [
    loading,
    setLoading,
  ] =
    useState(true);


  const [
    error,
    setError,
  ] =
    useState<string | null>(
      null
    );

  const [
    signatureData,
    setSignatureData,
  ] =
    useState<string | null>(
      null
    );

  const [
    firmando,
    setFirmando,
  ] =
    useState(false);

  const [
    errorFirma,
    setErrorFirma,
  ] =
    useState<string | null>(
      null
    );

  const [
    retryCount,
    setRetryCount,
  ] =
    useState(0);

  const [
    isOfficialModalOpen,
    setIsOfficialModalOpen,
  ] =
    useState(false);


  // ===================================================
  // FIRMAR ACTA
  // ===================================================

  async function handleFirmar() {
    setErrorFirma(null);

    if (!signatureData) {
      setErrorFirma("Debe realizar la firma manuscrita antes de continuar.");
      return;
    }

    if (!acta || acta.estado !== "Borrador") {
      setErrorFirma("El acta no se encuentra en estado Borrador.");
      return;
    }

    try {
      setFirmando(true);

      const signatureDataJson = JSON.stringify({
        tipo: "firma-manuscrita",
        firma: signatureData,
        origen: "web-mensajero",
        fecha: new Date().toISOString(),
      });

      const resultado = await firmarActa(acta.id, {
        signatureDataJson,
      });

      setActa({
        ...acta,
        ...resultado,
        equipo: acta.equipo,
      });

      setSignatureData(null);
    } catch (err) {
      setErrorFirma(
        err instanceof Error
          ? err.message
          : "No fue posible firmar el acta."
      );
    } finally {
      setFirmando(false);
    }
  }



  // ===================================================
  // CARGAR ACTA
  // ===================================================

  useEffect(() => {

    let mounted = true;

    async function loadActa() {

      try {

        setLoading(true);

        setError(null);

        const data =
          await getMiActaById(
            actaId
          );

        if (mounted) {
          setActa(data);
        }

      } catch (err) {

        if (!mounted) {
          return;
        }

        setError(
          err instanceof Error
            ? err.message
            : "No fue posible cargar el acta."
        );

      } finally {

        if (mounted) {
          setLoading(false);
        }

      }

    }

    loadActa();

    return () => {
      mounted = false;
    };

  }, [
    actaId,
    retryCount,
  ]);


  // ===================================================
  // LOADING
  // ===================================================

  if (loading) {

    return (

      <section className="mensajero-detail-page">

        <button
          type="button"
          className="mensajero-back-button"
          onClick={onBack}
        >
          â† Volver a mis actas
        </button>


        <div className="mensajero-detail-loading">

          <div className="mensajero-detail-loading-spinner" />

          <strong>
            Cargando acta
          </strong>

          <p>
            Estamos consultando la informaciÃ³n
            del documento.
          </p>

        </div>

      </section>

    );

  }


  // ===================================================
  // ERROR
  // ===================================================

  if (
    error ||
    !acta
  ) {

    return (

      <section className="mensajero-detail-page">

        <button
          type="button"
          className="mensajero-back-button"
          onClick={onBack}
        >
          â† Volver a mis actas
        </button>


        <header className="mensajero-detail-header">

          <div>

            <span className="mensajero-detail-eyebrow">
              ACTA
            </span>

            <h1>
              Detalle del acta
            </h1>

            <p>
              No pudimos consultar la informaciÃ³n
              solicitada.
            </p>

          </div>

        </header>


        <section className="mensajero-detail-error">

          <div className="mensajero-error-icon">
            !
          </div>


          <div>

            <span className="mensajero-detail-eyebrow">
              INFORMACIÃ“N NO DISPONIBLE
            </span>

            <h2>
              No fue posible obtener el detalle
              del acta
            </h2>

            <p>
              {error ??
                "El acta no estÃ¡ disponible actualmente."}
            </p>


            <div className="mensajero-error-actions">

              <button
                type="button"
                className="mensajero-primary-button"
                onClick={() =>
                  setRetryCount(
                    previous =>
                      previous + 1
                  )
                }
              >
                Intentar nuevamente
              </button>


              <button
                type="button"
                className="mensajero-secondary-button"
                onClick={onBack}
              >
                â† Volver a mis actas
              </button>

            </div>

          </div>

        </section>

      </section>

    );

  }


  const equipo =
    acta.equipo;


  const statusClass =
    getStatusClass(
      acta.estado
    );


  // ===================================================
  // VISTA PRINCIPAL
  // ===================================================

  return (

    <section className="mensajero-detail-page">

      {/* =================================================
          NAVEGACIÃ“N
          ================================================= */}

      <div className="mensajero-detail-navigation">

        <button
          type="button"
          className="mensajero-back-button"
          onClick={onBack}
        >
          <span>
            â†
          </span>

          Volver a mis actas

        </button>

      </div>


      {/* =================================================
          HEADER
          ================================================= */}

      <header className="mensajero-detail-header">

        <div className="mensajero-detail-header-main">

          <span className="mensajero-detail-eyebrow">
            ACTA DE{" "}
            {acta.tipoActa.toUpperCase()}
          </span>


          <div className="mensajero-detail-title-row">

            <h1>
              {acta.numeroActa}
            </h1>

            <span
              className={`mensajero-detail-status ${statusClass}`}
            >

              <span className="mensajero-detail-status-dot" />

              {acta.estado}

            </span>

          </div>


          <p>
            Documento generado el{" "}
            <strong>
              {formatDate(
                acta.fechaCreacion
              )}
            </strong>
          </p>

        </div>


        <div className="mensajero-detail-header-meta">
          <span>
            ENTREGA
          </span>

          <strong
            className={
              acta.actaEntregada
                ? "is-complete"
                : "is-pending"
            }
          >
            {acta.actaEntregada
              ? "Registrada"
              : "Pendiente"}
          </strong>

          <button
            type="button"
            onClick={() => setIsOfficialModalOpen(true)}
            style={{
              marginTop: "8px",
              background: "#0284c7",
              color: "#ffffff",
              border: "none",
              borderRadius: "8px",
              padding: "7px 12px",
              fontSize: "12px",
              fontWeight: 700,
              cursor: "pointer",
              display: "inline-flex",
              alignItems: "center",
              gap: "5px",
            }}
          >
            <span>ðŸ“„</span> Formato Oficial PDF
          </button>
        </div>


      </header>


      {/* =================================================
          IDENTIFICACIÃ“N
          ================================================= */}

      <section className="mensajero-detail-identity">

        <div className="mensajero-detail-identity-item">

          <span>
            ACTA
          </span>

          <strong>
            {acta.numeroActa}
          </strong>

        </div>


        <div className="mensajero-detail-identity-divider" />


        <div className="mensajero-detail-identity-item">

          <span>
            INVENTARIO
          </span>

          <strong>
            {equipo.inventoryKey}
          </strong>

        </div>


        <div className="mensajero-detail-identity-divider" />


        <div className="mensajero-detail-identity-item">

          <span>
            TIPO
          </span>

          <strong>
            {acta.tipoActa}
          </strong>

        </div>


        <div className="mensajero-detail-identity-divider" />


        <div className="mensajero-detail-identity-item">

          <span>
            ENTREGA
          </span>

          <strong>
            {acta.actaEntregada
              ? "Registrada"
              : "Pendiente"}
          </strong>

        </div>

      </section>


      {/* =================================================
          CONTENIDO
          ================================================= */}

      <div className="mensajero-detail-grid">


        {/* =================================================
            EQUIPO
            ================================================= */}

        <article className="mensajero-detail-card mensajero-detail-equipment-card">

          <div className="mensajero-card-heading">

            <div>

              <span>
                INVENTARIO
              </span>

              <h2>
                Equipo asociado
              </h2>

            </div>

          </div>


          <div className="mensajero-equipment-title">

            <strong>
              {equipo.nombreEquipo}
            </strong>

            <span>
              {equipo.inventoryKey}
            </span>

          </div>


          <div className="mensajero-info-grid">

            <div>
              <span>
                Marca
              </span>

              <strong>
                {equipo.marca ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Procesador
              </span>

              <strong>
                {equipo.procesador ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Memoria RAM
              </span>

              <strong>
                {equipo.memoriaRam ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Disco duro
              </span>

              <strong>
                {equipo.discoDuro ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Empresa
              </span>

              <strong>
                {equipo.empresa ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Asignado a
              </span>

              <strong>
                {equipo.asignadoA ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Proveedor
              </span>

              <strong>
                {equipo.proveedor ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Departamento
              </span>

              <strong>
                {equipo.departamento ?? "â€”"}
              </strong>
            </div>


            <div>
              <span>
                Estado del equipo
              </span>

              <strong>
                {equipo.estatusEquipo ?? "â€”"}
              </strong>
            </div>

          </div>

        </article>


        {/* =================================================
            INFORMACIÃ“N DEL ACTA
            ================================================= */}

        <article className="mensajero-detail-card">

          <div className="mensajero-card-heading">

            <div>

              <span>
                DOCUMENTO
              </span>

              <h2>
                InformaciÃ³n del acta
              </h2>

            </div>

          </div>


          <div className="mensajero-info-grid">

            <div>
              <span>
                NÃºmero
              </span>

              <strong>
                {acta.numeroActa}
              </strong>
            </div>


            <div>
              <span>
                Estado
              </span>

              <strong>
                {acta.estado}
              </strong>
            </div>


            <div>
              <span>
                Tipo de acta
              </span>

              <strong>
                {acta.tipoActa}
              </strong>
            </div>


            <div>
              <span>
                Entregada
              </span>

              <strong>
                {acta.actaEntregada
                  ? "SÃ­"
                  : "No"}
              </strong>
            </div>


            <div>
              <span>
                Usuario creador
              </span>

              <strong>
                {acta.usuarioId}
              </strong>
            </div>


            <div>
              <span>
                Destinatario
              </span>

              <strong>
                {acta.destinatarioUsuarioId ??
                  "No asignado"}
              </strong>
            </div>


            <div>
              <span>
                Mensajero
              </span>

              <strong>
                {acta.mensajeroUsuarioId ??
                  "No asignado"}
              </strong>
            </div>


            <div>
              <span>
                Dispositivo
              </span>

              <strong>
                {acta.deviceId ??
                  "No registrado"}
              </strong>
            </div>

          </div>

        </article>


        {/* =================================================
            OBSERVACIONES
            ================================================= */}

        <article className="mensajero-detail-card mensajero-detail-full">

          <div className="mensajero-card-heading">

            <div>

              <span>
                REGISTRO
              </span>

              <h2>
                Observaciones
              </h2>

            </div>

          </div>


          <div className="mensajero-observation">

            {getObservation(
              acta.formDataJson
            )}

          </div>

        </article>


        {/* =================================================
            TRAZABILIDAD
            ================================================= */}

        <article className="mensajero-detail-card mensajero-detail-full">

          <div className="mensajero-card-heading">

            <div>

              <span>
                TRAZABILIDAD
              </span>

              <h2>
                Historial del acta
              </h2>

            </div>

          </div>


          <div className="mensajero-timeline">


            <div
              className={
                acta.fechaCreacion
                  ? "timeline-step completed"
                  : "timeline-step pending"
              }
            >

              <div className="step-content">

                <span>
                  CreaciÃ³n
                </span>

                <strong>
                  {formatDate(
                    acta.fechaCreacion
                  )}
                </strong>

              </div>

            </div>


            <div
              className={
                acta.fechaFirma
                  ? "timeline-step completed"
                  : "timeline-step pending"
              }
            >

              <div className="step-content">

                <span>
                  Firma
                </span>

                <strong>
                  {formatDate(
                    acta.fechaFirma
                  )}
                </strong>

              </div>

            </div>


            <div
              className={
                acta.fechaFinalizacion
                  ? "timeline-step completed"
                  : "timeline-step pending"
              }
            >

              <div className="step-content">

                <span>
                  FinalizaciÃ³n
                </span>

                <strong>
                  {formatDate(
                    acta.fechaFinalizacion
                  )}
                </strong>

              </div>

            </div>


            <div
              className={
                acta.fechaAnulacion
                  ? "timeline-step canceled"
                  : "timeline-step pending"
              }
            >

              <div className="step-content">

                <span>
                  AnulaciÃ³n
                </span>

                <strong>
                  {formatDate(
                    acta.fechaAnulacion
                  )}
                </strong>

              </div>

            </div>

          </div>

        </article>


        {/* =================================================
            MOTIVO DE ANULACIÃ“N
            ================================================= */}

        {acta.motivoAnulacion && (

          <article className="mensajero-detail-card mensajero-detail-full">

            <div className="mensajero-card-heading">

              <div>

                <span>
                  ESTADO
                </span>

                <h2>
                  Motivo de anulaciÃ³n
                </h2>

              </div>

            </div>


            <div className="mensajero-observation">
              {acta.motivoAnulacion}
            </div>

          </article>

        )}


        {/* =================================================
            FIRMA MANUSCRITA (SOLO BORRADOR)
            ================================================= */}

        {acta.estado === "Borrador" && (

          <article className="mensajero-detail-card mensajero-detail-full">

            <div className="mensajero-card-heading">

              <div>

                <span>
                  FIRMA DE ENTREGA
                </span>

                <h2>
                  Firma manuscrita del receptor
                </h2>

              </div>

            </div>

            <p style={{ margin: "0.5rem 0 1rem 0", color: "#6b7280" }}>
              Solicite al destinatario realizar su firma dentro del recuadro para certificar la entrega.
            </p>

            <FirmaManuscrita
              onFirmaChange={setSignatureData}
              disabled={firmando}
            />

            {errorFirma && (
              <div
                role="alert"
                style={{
                  marginTop: "1rem",
                  padding: "0.75rem",
                  borderRadius: "0.5rem",
                  background: "#fee2e2",
                  color: "#991b1b",
                }}
              >
                {errorFirma}
              </div>
            )}

            <div
              style={{
                marginTop: "1.25rem",
                display: "flex",
                justifyContent: "flex-end",
              }}
            >
              <button
                type="button"
                className="mensajero-primary-button"
                onClick={handleFirmar}
                disabled={firmando || !signatureData}
              >
                {firmando ? "Registrando firma..." : "âœ“ Confirmar entrega y firmar"}
              </button>
            </div>

          </article>

        )}




      </div>

      <OfficialActaModal
        acta={acta}
        isOpen={isOfficialModalOpen}
        onClose={() => setIsOfficialModalOpen(false)}
      />
    </section>
  );
}

