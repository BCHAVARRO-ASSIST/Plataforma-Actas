import { useState } from "react";
import type {
  ActaDetail as ActaDetailType,
} from "../actaService";
import { finalizarActa, anularActa, descargarActaDocumento, generarActaDocumento } from "../actaService";
import { useAuth } from "../../auth/AuthContext";
import { PERMISSIONS, hasPermission } from "../../auth/permissions";
import { OfficialActaModal } from "./OfficialActaModal";


// =====================================================
// PROPS
// =====================================================

interface ActaDetailProps {
  acta: ActaDetailType;
  onBack: () => void;
  onRefresh?: () => Promise<void>;
}


// =====================================================
// FORMATEAR FECHA
// =====================================================

function formatDate(
  value: string | null
): string {
  if (!value) {
    return "—";
  }

  return new Date(
    value
  ).toLocaleString("es-CO", {
    dateStyle: "medium",
    timeStyle: "short",
  });
}


// =====================================================
// OBTENER OBSERVACIÓN
// =====================================================

function getObservation(
  formDataJson: string
): string {
  try {
    const data =
      JSON.parse(
        formDataJson
      );

    return (
      data.observacion ||
      data.observaciones ||
      "—"
    );
  } catch {
    return "—";
  }
}


// =====================================================
// COMPONENTE
// =====================================================

export function ActaDetail({
  acta,
  onBack,
  onRefresh,
}: ActaDetailProps) {
  const { token } = useAuth();
  const equipo = acta.equipo;

  const [loadingAction, setLoadingAction] = useState(false);
  const [downloadingDoc, setDownloadingDoc] = useState(false);
  const [generatingDoc, setGeneratingDoc] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Modal exportación oficial
  const [isOfficialModalOpen, setIsOfficialModalOpen] = useState(false);

  // Anular modal state
  const [isAnularModalOpen, setIsAnularModalOpen] = useState(false);
  const [motivoAnulacion, setMotivoAnulacion] = useState("");

  const canEdit = hasPermission(token, PERMISSIONS.ACTAS_EDITAR);
  const canExport = hasPermission(token, PERMISSIONS.ACTAS_EXPORTAR) || hasPermission(token, PERMISSIONS.ACTAS_CONSULTAR);

  // Extraer firma si existe
  let signatureImage: string | null = null;
  if (acta.signatureDataJson) {
    try {
      const sig = JSON.parse(acta.signatureDataJson);
      signatureImage = sig.firma || sig.signature || null;
    } catch {
      signatureImage = null;
    }
  }

  async function handleDescargarPdf() {
    try {
      setDownloadingDoc(true);
      setActionError(null);
      await descargarActaDocumento(acta.id, acta.numeroActa);
      setSuccessMessage("Documento oficial PDF descargado correctamente.");
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Error al descargar el documento oficial.");
    } finally {
      setDownloadingDoc(false);
    }
  }

  async function handleGenerarDocumento() {
    try {
      setGeneratingDoc(true);
      setActionError(null);
      const res = await generarActaDocumento(acta.id);
      setSuccessMessage(`Documento oficial generado y persistido con éxito (${(res.tamanoBytes / 1024).toFixed(1)} KB).`);
      if (onRefresh) await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Error al generar el documento oficial.");
    } finally {
      setGeneratingDoc(false);
    }
  }

  async function handleFinalizar() {
    if (!window.confirm("¿Estás seguro de que deseas finalizar este acta? Una vez finalizada no podrá modificarse.")) {
      return;
    }

    try {
      setLoadingAction(true);
      setActionError(null);
      await finalizarActa(acta.id);
      setSuccessMessage("Acta finalizada correctamente.");
      if (onRefresh) await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Error al finalizar el acta.");
    } finally {
      setLoadingAction(false);
    }
  }

  async function handleConfirmAnular(e: React.FormEvent) {
    e.preventDefault();
    if (!motivoAnulacion.trim()) {
      setActionError("El motivo de anulación es obligatorio.");
      return;
    }

    try {
      setLoadingAction(true);
      setActionError(null);
      await anularActa(acta.id, motivoAnulacion.trim());
      setSuccessMessage("Acta anulada correctamente.");
      setIsAnularModalOpen(false);
      if (onRefresh) await onRefresh();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Error al anular el acta.");
    } finally {
      setLoadingAction(false);
    }
  }

  return (
    <section className="acta-detail-page">

      {/* =================================================
          BARRA SUPERIOR
      ================================================== */}

      <div className="acta-detail-topbar">
        <button
          type="button"
          className="acta-back-button"
          onClick={onBack}
        >
          ←  Volver a Actas
        </button>
      </div>

      {/* MENSAJES DE ESTADO */}
      {successMessage && (
        <div className="usuarios-success" style={{ marginBottom: "20px" }}>
          <span>✓</span>
          {successMessage}
          <button type="button" onClick={() => setSuccessMessage(null)} aria-label="Cerrar">
            ×
          </button>
        </div>
      )}

      {actionError && (
        <div className="actas-error" style={{ marginBottom: "20px" }}>
          {actionError}
        </div>
      )}

      {/* =================================================
          ENCABEZADO
      ================================================== */}

      <div className="acta-detail-header">
        <div>
          <span className="page-eyebrow">
            Gestión documental · Supervisión
          </span>

          <h1>
            Detalle del Acta {acta.numeroActa}
          </h1>

          <p>
            Información completa del acta, equipo asociado y estado de entrega.
          </p>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "10px", flexWrap: "wrap" }}>
          {/* BOTÓN VISTA PREVIA OFICIAL */}
          {canExport && (
            <button
              type="button"
              onClick={() => setIsOfficialModalOpen(true)}
              style={{
                background: "#0284c7",
                color: "#ffffff",
                border: "none",
                borderRadius: "8px",
                padding: "8px 16px",
                fontSize: "13px",
                fontWeight: 700,
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                minHeight: "38px",
                boxShadow: "0 2px 4px rgba(2, 132, 199, 0.2)",
              }}
            >
              <span>ðŸ‘ï¸</span> Vista Previa
            </button>
          )}

          {/* BOTÓN DESCARGAR PDF OFICIAL */}
          {canExport && (
            <button
              type="button"
              onClick={handleDescargarPdf}
              disabled={downloadingDoc}
              style={{
                background: "#16a34a",
                color: "#ffffff",
                border: "none",
                borderRadius: "8px",
                padding: "8px 16px",
                fontSize: "13px",
                fontWeight: 700,
                cursor: downloadingDoc ? "wait" : "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                minHeight: "38px",
                boxShadow: "0 2px 4px rgba(22, 163, 74, 0.25)",
              }}
            >
              <span>ðŸ“¥</span> {downloadingDoc ? "Descargando..." : (acta.estado === "Firmada" ? "Descargar Documento Firmado" : "Descargar PDF")}
            </button>
          )}

          <span
            className={
              `acta-status-badge acta-status-${acta.estado.toLowerCase()}`
            }
          >
            {acta.estado}
          </span>

          {canEdit && acta.estado === "Firmada" && (
            <button
              type="button"
              className="actas-primary"
              onClick={handleFinalizar}
              disabled={loadingAction}
              style={{ minHeight: "38px", padding: "8px 16px", fontSize: "13px" }}
            >
              {loadingAction ? "Finalizando..." : "✓ Finalizar Acta"}
            </button>
          )}

          {canEdit && acta.estado !== "Finalizada" && acta.estado !== "Anulada" && (
            <button
              type="button"
              onClick={() => {
                setMotivoAnulacion("");
                setIsAnularModalOpen(true);
              }}
              disabled={loadingAction}
              className="usuario-delete-button"
              style={{ padding: "8px 14px", fontSize: "13px", height: "38px" }}
            >
              Anular Acta
            </button>
          )}
        </div>
      </div>


      {/* =================================================
          INFORMACIÓN
      ================================================== */}

      <div className="acta-detail-grid">

        {/* =================================================
            INFORMACIÓN DEL ACTA
        ================================================== */}

        <section className="acta-detail-card">
          <div className="acta-card-header">
            <h2>Información del Acta</h2>
          </div>

          <div className="acta-info-grid">
            <div className="acta-info-item">
              <span>ID del acta</span>
              <strong>{acta.id}</strong>
            </div>

            <div className="acta-info-item">
              <span>Número de acta</span>
              <strong>{acta.numeroActa}</strong>
            </div>

            <div className="acta-info-item">
              <span>Tipo de acta</span>
              <strong>{acta.tipoActa}</strong>
            </div>

            <div className="acta-info-item">
              <span>Estado</span>
              <strong>{acta.estado}</strong>
            </div>

            <div className="acta-info-item">
              <span>Acta entregada</span>
              <strong>{acta.actaEntregada ? "Sí" : "No"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Consultor / Receptor</span>
              <strong>{acta.destinatarioNombre || acta.destinatarioUsuarioId || "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Correo Consultor</span>
              <strong>{acta.destinatarioCorreo || "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Mensajero / Entrega</span>
              <strong>{acta.mensajeroNombre || (acta.mensajeroUsuarioId ? `ID: ${acta.mensajeroUsuarioId}` : "Sin asignar")}</strong>
            </div>

            <div className="acta-info-item">
              <span>Fecha de creación</span>
              <strong>{formatDate(acta.fechaCreacion)}</strong>
            </div>

            <div className="acta-info-item">
              <span>Fecha de firma</span>
              <strong>{formatDate(acta.fechaFirma)}</strong>
            </div>

            <div className="acta-info-item">
              <span>Fecha de finalización</span>
              <strong>{formatDate(acta.fechaFinalizacion)}</strong>
            </div>
          </div>
        </section>


        {/* =================================================
            EQUIPO
        ================================================== */}

        <section className="acta-detail-card">
          <div className="acta-card-header">
            <h2>Equipo Asociado</h2>
          </div>

          <div className="acta-info-grid">
            <div className="acta-info-item">
              <span>Inventario</span>
              <strong>{equipo.inventoryKey}</strong>
            </div>

            <div className="acta-info-item">
              <span>Nombre</span>
              <strong>{equipo.nombreEquipo}</strong>
            </div>

            <div className="acta-info-item">
              <span>Marca</span>
              <strong>{equipo.marca ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Procesador</span>
              <strong>{equipo.procesador ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Memoria RAM</span>
              <strong>{equipo.memoriaRam ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Disco duro</span>
              <strong>{equipo.discoDuro ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Empresa</span>
              <strong>{equipo.empresa ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Asignado a</span>
              <strong>{equipo.asignadoA ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Departamento</span>
              <strong>{equipo.departamento ?? "—"}</strong>
            </div>

            <div className="acta-info-item">
              <span>Estado del equipo</span>
              <strong>{equipo.estatusEquipo ?? "—"}</strong>
            </div>
          </div>
        </section>


        {/* =================================================
            OBSERVACIONES
        ================================================== */}

        <section className="acta-detail-card acta-detail-full">
          <div className="acta-card-header">
            <h2>Observaciones</h2>
          </div>

          <div className="acta-observation">
            {getObservation(acta.formDataJson)}
          </div>
        </section>


        {/* =================================================
            DOCUMENTO OFICIAL Y CERTIFICACIÓN
        ================================================== */}

        <section className="acta-detail-card acta-detail-full">
          <div className="acta-card-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <h2>ðŸ“„ Documento Oficial de Compañía (TI-FOR-ACTA-001)</h2>
            <div style={{ display: "flex", gap: "8px" }}>
              {canExport && (
                <button
                  type="button"
                  onClick={() => setIsOfficialModalOpen(true)}
                  style={{
                    background: "#0284c7",
                    color: "#ffffff",
                    border: "none",
                    borderRadius: "6px",
                    padding: "6px 12px",
                    fontSize: "12px",
                    fontWeight: 700,
                    cursor: "pointer",
                  }}
                >
                  ðŸ‘ï¸ Vista Previa PDF
                </button>
              )}
              {canExport && (
                <button
                  type="button"
                  onClick={handleDescargarPdf}
                  disabled={downloadingDoc}
                  style={{
                    background: "#16a34a",
                    color: "#ffffff",
                    border: "none",
                    borderRadius: "6px",
                    padding: "6px 12px",
                    fontSize: "12px",
                    fontWeight: 700,
                    cursor: downloadingDoc ? "wait" : "pointer",
                  }}
                >
                  ðŸ“¥ {downloadingDoc ? "Descargando..." : "Descargar PDF"}
                </button>
              )}
              {canEdit && (
                <button
                  type="button"
                  onClick={handleGenerarDocumento}
                  disabled={generatingDoc}
                  style={{
                    background: "#475569",
                    color: "#ffffff",
                    border: "none",
                    borderRadius: "6px",
                    padding: "6px 12px",
                    fontSize: "12px",
                    fontWeight: 600,
                    cursor: generatingDoc ? "wait" : "pointer",
                  }}
                >
                  ðŸ”„ {generatingDoc ? "Generando..." : "Sincronizar Documento"}
                </button>
              )}
            </div>
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "20px", marginTop: "14px" }}>
            {/* VISTA DE FIRMA */}
            <div style={{ border: "1px solid #e2e8f0", borderRadius: "10px", padding: "16px", background: "#f8fafc" }}>
              <div style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", marginBottom: "8px" }}>
                Firma Manuscrita Digitalizada:
              </div>
              <div style={{ height: "90px", display: "flex", alignItems: "center", justifyContent: "center", background: "#ffffff", borderRadius: "8px", border: "1px dashed #cbd5e1" }}>
                {signatureImage ? (
                  <img src={signatureImage} alt="Firma del Consultor" style={{ maxHeight: "80px", maxWidth: "90%", objectFit: "contain" }} />
                ) : (
                  <span style={{ fontSize: "12px", color: acta.estado === "Borrador" ? "#94a3b8" : "#166534", fontStyle: acta.estado === "Borrador" ? "italic" : "normal", fontWeight: 600 }}>
                    {acta.estado === "Borrador" ? "Firma pendiente por recolectar en campo" : "✓ Firma registrada y certificada"}
                  </span>
                )}
              </div>
              <div style={{ fontSize: "11px", color: "#64748b", marginTop: "8px" }}>
                Fecha de Firma: {formatDate(acta.fechaFirma)}
              </div>
            </div>

            {/* INTEGRIDAD CRIPTOGRÁFICA */}
            <div style={{ border: "1px solid #e2e8f0", borderRadius: "10px", padding: "16px", background: "#f8fafc", display: "flex", flexDirection: "column", justifyContent: "space-between" }}>
              <div>
                <div style={{ fontSize: "12px", fontWeight: 700, color: "#64748b", marginBottom: "6px" }}>
                  Integridad Criptográfica (SHA-256):
                </div>
                <div style={{ fontSize: "11px", fontFamily: "monospace", color: "#0f172a", wordBreak: "break-all", background: "#ffffff", padding: "8px", borderRadius: "6px", border: "1px solid #e2e8f0" }}>
                  {acta.hashSha256 || "Se generará automáticamente tras la firma digital."}
                </div>
              </div>

              <div style={{ fontSize: "11px", color: "#64748b", marginTop: "10px" }}>
                Documento generado conforme al estándar <strong>TI-FOR-ACTA-001 (ISO 27001)</strong>.
              </div>
            </div>
          </div>
        </section>

        {acta.motivoAnulacion && (
          <section className="acta-detail-card acta-detail-full">
            <div className="acta-card-header">
              <h2>Motivo de anulación</h2>
            </div>

            <div className="acta-observation" style={{ color: "#991b1b" }}>
              {acta.motivoAnulacion}
            </div>
          </section>
        )}

      </div>

      {/* MODAL FORMATO OFICIAL DE LA COMPAÑÍA */}
      <OfficialActaModal
        acta={acta}
        isOpen={isOfficialModalOpen}
        onClose={() => setIsOfficialModalOpen(false)}
      />

      {/* MODAL ANULAR ACTA */}
      {isAnularModalOpen && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: "rgba(15, 23, 42, 0.65)",
            backdropFilter: "blur(4px)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 9999,
            padding: "20px",
          }}
          onClick={() => setIsAnularModalOpen(false)}
        >
          <div
            style={{
              background: "#ffffff",
              borderRadius: "14px",
              width: "100%",
              maxWidth: "480px",
              padding: "24px",
              boxShadow: "0 20px 25px -5px rgba(0, 0, 0, 0.1)",
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "14px" }}>
              <h2 style={{ margin: 0, fontSize: "18px", fontWeight: 800, color: "#0f172a" }}>Anulación de Acta</h2>
              <button
                type="button"
                onClick={() => setIsAnularModalOpen(false)}
                style={{ background: "none", border: "none", fontSize: "16px", cursor: "pointer", color: "#64748b" }}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleConfirmAnular}>
              <p style={{ fontSize: "13px", color: "#475569", margin: "0 0 14px 0" }}>
                Ingresa el motivo detallado de la anulación para el acta <strong>{acta.numeroActa}</strong>. Si el equipo estaba asignado, regresará automáticamente a estado activo.
              </p>

              <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#475569", marginBottom: "6px" }}>
                Motivo de anulación *
              </label>
              <textarea
                required
                rows={3}
                value={motivoAnulacion}
                onChange={(e) => setMotivoAnulacion(e.target.value)}
                placeholder="Ej. Error en número de serie del equipo asignado..."
                style={{ width: "100%", padding: "10px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px", fontFamily: "inherit" }}
              />

              <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginTop: "18px" }}>
                <button
                  type="button"
                  onClick={() => setIsAnularModalOpen(false)}
                  disabled={loadingAction}
                  style={{ padding: "8px 16px", background: "#ffffff", border: "1px solid #cbd5e1", borderRadius: "8px", color: "#475569", fontWeight: 600, fontSize: "13px", cursor: "pointer" }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={loadingAction}
                  className="usuario-delete-button"
                  style={{ padding: "8px 18px", fontSize: "13px", cursor: "pointer" }}
                >
                  {loadingAction ? "Anulando..." : "Confirmar Anulación"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

    </section>
  );
}
