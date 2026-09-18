import { useState } from "react";
import type {
  ActaDetail as ActaDetailType,
} from "@/features/actas/actaService";
import { finalizarActa, anularActa, descargarActaDocumento, generarActaDocumento } from "@/features/actas/actaService";
import { useAuth } from "@/features/auth/useAuth";
import { PERMISSIONS, hasPermission } from "@/features/auth/permissions";
import { OfficialActaModal } from "./OfficialActaModal";
import { FirmaConsultorView } from "./FirmaConsultorView";
import {
  EyeIcon,
  DownloadIcon,
  DocumentPdfIcon,
  RefreshIcon,
  CheckIcon,
  CloseIcon,
  ArrowLeftIcon,
  ActaIcon,
  EquipmentIcon,
  UserIcon,
  MessengerIcon,
  CalendarIcon,
  ClockIcon,
  SignatureIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";

// =====================================================
// PROPS
// =====================================================

interface ActaDetailProps {
  acta: ActaDetailType;
  onBack: () => void;
  onRefresh?: () => Promise<void>;
}

// =====================================================
// ICONO COPIAR
// =====================================================

function CopyIcon({ size = 13 }: { size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
    </svg>
  );
}

// =====================================================
// FORMATEAR FECHA
// =====================================================

function formatDate(value: string | null): string {
  if (!value) {
    return "—";
  }

  try {
    return new Date(value).toLocaleString("es-CO", {
      dateStyle: "medium",
      timeStyle: "short",
    });
  } catch {
    return value;
  }
}

// =====================================================
// OBTENER OBSERVACIÓN
// =====================================================

function getObservation(formDataJson: string): string {
  try {
    const data = JSON.parse(formDataJson);
    return data.observacion || data.observaciones || "";
  } catch {
    return "";
  }
}

// =====================================================
// COMPONENTE PRINCIPAL
// =====================================================

export function ActaDetail({
  acta,
  onBack,
  onRefresh,
}: ActaDetailProps) {
  const { user } = useAuth();
  const equipo = acta.equipo;

  const [loadingAction, setLoadingAction] = useState(false);
  const [downloadingDoc, setDownloadingDoc] = useState(false);
  const [generatingDoc, setGeneratingDoc] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Copied feedback
  const [copiedField, setCopiedField] = useState<string | null>(null);

  // Modal exportación oficial
  const [isOfficialModalOpen, setIsOfficialModalOpen] = useState(false);

  // Anular modal state
  const [isAnularModalOpen, setIsAnularModalOpen] = useState(false);
  const [motivoAnulacion, setMotivoAnulacion] = useState("");

  // Vista individual de firma para el consultor
  const [showFirmaConsultorView, setShowFirmaConsultorView] = useState(false);

  const canEdit = hasPermission(user, PERMISSIONS.ACTAS_EDITAR);
  const canExport = hasPermission(user, PERMISSIONS.ACTAS_EXPORTAR) || hasPermission(user, PERMISSIONS.ACTAS_CONSULTAR);

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

  const observationText = getObservation(acta.formDataJson);

  if (showFirmaConsultorView) {
    return (
      <FirmaConsultorView
        actaId={acta.id}
        numeroActa={acta.numeroActa}
        tipoActa={acta.tipoActa}
        consultorNombre={acta.destinatarioNombre || acta.equipo?.asignadoA || "Consultor Asignado"}
        consultorCorreo={acta.destinatarioCorreo}
        consultorCargo={acta.destinatarioCargo || acta.equipo?.departamento}
        equipoNombre={acta.equipo?.nombreEquipo || acta.nombreEquipo || "Equipo TI"}
        equipoMarca={acta.equipo?.marca}
        equipoSerialOPlaca={acta.equipo?.inventoryKey || acta.inventoryKey || "Sin placa"}
        equipoSpecs={[acta.equipo?.procesador, acta.equipo?.memoriaRam, acta.equipo?.discoDuro].filter(Boolean).join(" · ")}
        onBack={() => setShowFirmaConsultorView(false)}
        onSignedSuccess={async () => {
          setShowFirmaConsultorView(false);
          if (onRefresh) await onRefresh();
        }}
      />
    );
  }

  function handleCopy(text: string, field: string) {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text);
      setCopiedField(field);
      setTimeout(() => setCopiedField(null), 2000);
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
          <ArrowLeftIcon size={16} />
          <span>Volver a Actas</span>
        </button>

        <div className="acta-topbar-meta">
          <span
            style={{
              fontSize: "12px",
              fontWeight: 600,
              color: "var(--text-muted)",
              background: "var(--color-slate-100)",
              padding: "4px 10px",
              borderRadius: "var(--radius-pill)",
              border: "1px solid var(--border-default)",
            }}
          >
            Tipo: <strong style={{ color: "var(--text-primary)" }}>{acta.tipoActa}</strong>
          </span>
        </div>
      </div>

      {/* MENSAJES DE ESTADO */}
      <AutoDismissAlert
        type="success"
        message={successMessage}
        duration={4500}
        onDismiss={() => setSuccessMessage(null)}
      />

      <AutoDismissAlert
        type="error"
        message={actionError}
        duration={5000}
        onDismiss={() => setActionError(null)}
      />

      {/* =================================================
          ENCABEZADO HERO
      ================================================== */}

      <div className="acta-detail-header">
        <div className="acta-header-info">
          <span className="acta-header-eyebrow">
            <span className="acta-header-eyebrow-dot" />
            Gestión documental · Supervisión de Actas
          </span>

          <h1 className="acta-header-title">
            <span>Detalle del Acta</span>
            <span
              style={{
                color: "var(--color-primary-600)",
                background: "var(--color-primary-50)",
                padding: "2px 10px",
                borderRadius: "var(--radius-sm)",
                border: "1px solid var(--color-primary-200)",
                fontSize: "22px",
                fontFamily: "var(--font-mono)",
              }}
            >
              {acta.numeroActa}
            </span>
          </h1>

          <p className="acta-header-subtitle">
            Información completa del acta, equipo tecnológico asociado y trazabilidad de entrega.
          </p>
        </div>

        <div className="acta-header-actions">
          {/* BOTÓN VISTA PREVIA OFICIAL */}
          {canExport && (
            <button
              type="button"
              className="acta-btn-preview"
              onClick={() => setIsOfficialModalOpen(true)}
              title="Ver formato oficial del acta"
            >
              <EyeIcon size={16} />
              <span>Vista Previa</span>
            </button>
          )}

          {/* BOTÓN DESCARGAR PDF OFICIAL */}
          {canExport && (
            <button
              type="button"
              className="acta-btn-download"
              onClick={handleDescargarPdf}
              disabled={downloadingDoc}
              title="Descargar PDF generado"
            >
              <DownloadIcon size={16} />
              <span>{downloadingDoc ? "Descargando..." : (acta.estado === "Firmada" ? "Descargar Documento Firmado" : "Descargar PDF")}</span>
            </button>
          )}

          {/* BOTÓN FIRMAR CON EL CONSULTOR (SI BORRADOR) */}
          {canEdit && acta.estado === "Borrador" && (
            <button
              type="button"
              className="btn btn-primary btn-sm"
              onClick={() => setShowFirmaConsultorView(true)}
              style={{ display: "inline-flex", alignItems: "center", gap: "6px", fontWeight: 700, padding: "8px 16px" }}
              title="Abrir vista individual de firma para el consultor"
            >
              <SignatureIcon size={15} />
              <span>Firmar con el Consultor</span>
            </button>
          )}

          {/* BADGE DE ESTADO DINÁMICO */}
          <span className={`acta-status-badge acta-status-${acta.estado.toLowerCase()}`}>
            <span className="acta-status-badge-dot" />
            {acta.estado}
          </span>

          {/* BOTÓN FINALIZAR (SOLO SI FIRMADA) */}
          {canEdit && acta.estado === "Firmada" && (
            <button
              type="button"
              className="acta-btn-finalize"
              onClick={handleFinalizar}
              disabled={loadingAction}
            >
              <CheckIcon size={15} />
              <span>{loadingAction ? "Finalizando..." : "Finalizar Acta"}</span>
            </button>
          )}

          {/* BOTÓN ANULAR */}
          {canEdit && acta.estado !== "Finalizada" && acta.estado !== "Anulada" && (
            <button
              type="button"
              onClick={() => {
                setMotivoAnulacion("");
                setIsAnularModalOpen(true);
              }}
              disabled={loadingAction}
              className="acta-btn-cancel"
            >
              <CloseIcon size={14} />
              <span>Anular Acta</span>
            </button>
          )}
        </div>
      </div>


      {/* =================================================
          GRID DE INFORMACIÓN PRINCIPAL
      ================================================== */}

      <div className="acta-detail-grid">

        {/* =================================================
            TARJETA 1: INFORMACIÓN DEL ACTA
        ================================================== */}

        <section className="acta-detail-card">
          <div className="acta-card-header">
            <div className="acta-card-title-group">
              <div className="acta-card-icon-wrap acta-icon-blue">
                <ActaIcon size={18} />
              </div>
              <h2>Información del Acta</h2>
            </div>
            <span
              style={{
                fontSize: "11.5px",
                color: "var(--text-muted)",
                fontWeight: 600,
              }}
            >
              Folio oficial
            </span>
          </div>

          <div className="acta-info-grid">
            {/* ID DEL ACTA */}
            <div className="acta-info-item">
              <span>ID del Acta</span>
              <strong>
                <span className="acta-code-pill" title={acta.id}>
                  {acta.id.substring(0, 13)}...
                </span>
                <button
                  type="button"
                  className="acta-copy-btn"
                  onClick={() => handleCopy(acta.id, "id")}
                  title="Copiar ID completo"
                >
                  <CopyIcon size={13} />
                </button>
                {copiedField === "id" && <span className="acta-copied-tooltip">¡Copiado!</span>}
              </strong>
            </div>

            {/* NÚMERO DE ACTA */}
            <div className="acta-info-item">
              <span>Número de Acta</span>
              <strong>
                <span
                  style={{
                    color: "var(--color-primary-700)",
                    fontWeight: 700,
                    fontFamily: "var(--font-mono)",
                  }}
                >
                  {acta.numeroActa}
                </span>
                <button
                  type="button"
                  className="acta-copy-btn"
                  onClick={() => handleCopy(acta.numeroActa, "numeroActa")}
                  title="Copiar número de acta"
                >
                  <CopyIcon size={13} />
                </button>
                {copiedField === "numeroActa" && <span className="acta-copied-tooltip">¡Copiado!</span>}
              </strong>
            </div>

            {/* TIPO DE ACTA */}
            <div className="acta-info-item">
              <span>Tipo de Acta</span>
              <strong>
                <span
                  style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: "5px",
                    background: acta.tipoActa === "Entrega" ? "var(--color-primary-50)" : "#fffbeb",
                    color: acta.tipoActa === "Entrega" ? "var(--color-primary-700)" : "#b45309",
                    padding: "2px 8px",
                    borderRadius: "var(--radius-sm)",
                    fontSize: "12.5px",
                    fontWeight: 700,
                  }}
                >
                  {acta.tipoActa}
                </span>
              </strong>
            </div>

            {/* ESTADO */}
            <div className="acta-info-item">
              <span>Estado</span>
              <strong>
                <span className={`acta-status-badge acta-status-${acta.estado.toLowerCase()}`} style={{ padding: "2px 10px", fontSize: "11.5px" }}>
                  <span className="acta-status-badge-dot" />
                  {acta.estado}
                </span>
              </strong>
            </div>

            {/* ACTA ENTREGADA */}
            <div className="acta-info-item">
              <span>Acta Entregada</span>
              <strong>
                {acta.actaEntregada ? (
                  <span style={{ color: "var(--color-success-700)", display: "inline-flex", alignItems: "center", gap: "4px" }}>
                    <CheckIcon size={15} /> Sí
                  </span>
                ) : (
                  <span style={{ color: "var(--color-warning-700)", display: "inline-flex", alignItems: "center", gap: "4px" }}>
                    <ClockIcon size={14} /> No
                  </span>
                )}
              </strong>
            </div>

            {/* CONSULTOR / RECEPTOR */}
            <div className="acta-info-item">
              <span>Consultor / Receptor</span>
              <strong>
                <UserIcon size={15} style={{ color: "var(--color-slate-400)", flexShrink: 0 }} />
                <span>{acta.destinatarioNombre || acta.destinatarioUsuarioId || "—"}</span>
              </strong>
            </div>

            {/* CORREO CONSULTOR */}
            <div className="acta-info-item">
              <span>Correo Consultor</span>
              <strong>
                <span style={{ color: "var(--color-primary-600)", fontSize: "13px" }}>
                  {acta.destinatarioCorreo || "—"}
                </span>
              </strong>
            </div>

            {/* MENSAJERO / ENTREGA */}
            <div className="acta-info-item">
              <span>Mensajero / Entrega</span>
              <strong>
                <MessengerIcon size={15} style={{ color: "var(--color-slate-400)", flexShrink: 0 }} />
                <span>{acta.mensajeroNombre || (acta.mensajeroUsuarioId ? `ID: ${acta.mensajeroUsuarioId}` : "Sin asignar")}</span>
              </strong>
            </div>

            {/* FECHA DE CREACIÓN */}
            <div className="acta-info-item">
              <span>Fecha de Creación</span>
              <strong>
                <CalendarIcon size={14} style={{ color: "var(--color-slate-400)", flexShrink: 0 }} />
                <span>{formatDate(acta.fechaCreacion)}</span>
              </strong>
            </div>

            {/* FECHA DE FIRMA */}
            <div className="acta-info-item">
              <span>Fecha de Firma</span>
              <strong>
                <CalendarIcon size={14} style={{ color: "var(--color-slate-400)", flexShrink: 0 }} />
                <span>{formatDate(acta.fechaFirma)}</span>
              </strong>
            </div>

            {/* FECHA DE FINALIZACIÓN */}
            <div className="acta-info-item" style={{ gridColumn: "span 2" }}>
              <span>Fecha de Finalización</span>
              <strong>
                <CalendarIcon size={14} style={{ color: "var(--color-slate-400)", flexShrink: 0 }} />
                <span>{formatDate(acta.fechaFinalizacion)}</span>
              </strong>
            </div>
          </div>
        </section>


        {/* =================================================
            TARJETA 2: EQUIPO ASOCIADO
        ================================================== */}

        <section className="acta-detail-card">
          <div className="acta-card-header">
            <div className="acta-card-title-group">
              <div className="acta-card-icon-wrap acta-icon-teal">
                <EquipmentIcon size={18} />
              </div>
              <h2>Equipo Asociado</h2>
            </div>
            <span
              style={{
                fontSize: "11.5px",
                color: "var(--text-muted)",
                fontWeight: 600,
              }}
            >
              Inventario de TI
            </span>
          </div>

          <div className="acta-info-grid">
            {/* INVENTARIO */}
            <div className="acta-info-item">
              <span>Inventario (SKU / Serial)</span>
              <strong>
                <span className="acta-code-pill" style={{ color: "var(--color-primary-700)", background: "var(--color-primary-50)", borderColor: "var(--color-primary-200)" }}>
                  {equipo.inventoryKey}
                </span>
                <button
                  type="button"
                  className="acta-copy-btn"
                  onClick={() => handleCopy(equipo.inventoryKey, "inv")}
                  title="Copiar código de inventario"
                >
                  <CopyIcon size={13} />
                </button>
                {copiedField === "inv" && <span className="acta-copied-tooltip">¡Copiado!</span>}
              </strong>
            </div>

            {/* NOMBRE */}
            <div className="acta-info-item">
              <span>Nombre del Equipo</span>
              <strong style={{ color: "var(--text-primary)" }}>{equipo.nombreEquipo}</strong>
            </div>

            {/* MARCA */}
            <div className="acta-info-item">
              <span>Marca</span>
              <strong>{equipo.marca ?? "—"}</strong>
            </div>

            {/* PROCESADOR */}
            <div className="acta-info-item">
              <span>Procesador</span>
              <strong>{equipo.procesador ?? "—"}</strong>
            </div>

            {/* MEMORIA RAM */}
            <div className="acta-info-item">
              <span>Memoria RAM</span>
              <strong>{equipo.memoriaRam ?? "—"}</strong>
            </div>

            {/* DISCO DURO */}
            <div className="acta-info-item">
              <span>Disco Duro</span>
              <strong>{equipo.discoDuro ?? "—"}</strong>
            </div>

            {/* EMPRESA / SEDE */}
            <div className="acta-info-item">
              <span>Empresa / Sede</span>
              <strong>{equipo.empresa ?? "—"}</strong>
            </div>

            {/* ASIGNADO A */}
            <div className="acta-info-item">
              <span>Asignado A</span>
              <strong>{equipo.asignadoA ?? "—"}</strong>
            </div>

            {/* DEPARTAMENTO */}
            <div className="acta-info-item">
              <span>Departamento</span>
              <strong>{equipo.departamento ?? "—"}</strong>
            </div>

            {/* ESTADO DEL EQUIPO */}
            <div className="acta-info-item">
              <span>Estado del Equipo</span>
              <strong>
                <span
                  style={{
                    background: equipo.estatusEquipo === "Activo" ? "var(--color-success-50)" : "var(--color-slate-100)",
                    color: equipo.estatusEquipo === "Activo" ? "var(--color-success-700)" : "var(--color-slate-700)",
                    padding: "2px 8px",
                    borderRadius: "var(--radius-sm)",
                    fontSize: "12px",
                    fontWeight: 700,
                  }}
                >
                  {equipo.estatusEquipo ?? "—"}
                </span>
              </strong>
            </div>
          </div>
        </section>


        {/* =================================================
            TARJETA 3: OBSERVACIONES
        ================================================== */}

        <section className="acta-detail-card acta-detail-full">
          <div className="acta-card-header">
            <div className="acta-card-title-group">
              <div className="acta-card-icon-wrap acta-icon-purple">
                <ActaIcon size={18} />
              </div>
              <h2>Observaciones Registradas</h2>
            </div>
          </div>

          <div className="acta-observation-box">
            {observationText ? (
              <div className="acta-observation">
                {observationText}
              </div>
            ) : (
              <div className="acta-observation acta-observation-empty">
                Sin observaciones adicionales registradas para este acta.
              </div>
            )}
          </div>
        </section>


        {/* =================================================
            TARJETA 4: DOCUMENTO OFICIAL Y FIRMA
            (Sección de SHA-256 e ISO eliminada conforme a solicitud)
        ================================================== */}

        <section className="acta-detail-card acta-detail-full">
          <div className="acta-card-header">
            <div className="acta-card-title-group">
              <div className="acta-card-icon-wrap acta-icon-emerald">
                <DocumentPdfIcon size={20} />
              </div>
              <div>
                <h2>Documento Oficial de Compañía (TI-FOR-ACTA-001)</h2>
                <span style={{ fontSize: "12px", color: "var(--text-muted)" }}>
                  Certificación formal y firma manuscrita digitalizada
                </span>
              </div>
            </div>

            <div style={{ display: "flex", alignItems: "center", gap: "8px", flexWrap: "wrap" }}>
              {canExport && (
                <button
                  type="button"
                  className="acta-btn-preview"
                  onClick={() => setIsOfficialModalOpen(true)}
                  style={{ minHeight: "34px", padding: "6px 14px", fontSize: "12.5px" }}
                >
                  <EyeIcon size={14} />
                  <span>Vista Previa PDF</span>
                </button>
              )}

              {canExport && (
                <button
                  type="button"
                  className="acta-btn-download"
                  onClick={handleDescargarPdf}
                  disabled={downloadingDoc}
                  style={{ minHeight: "34px", padding: "6px 14px", fontSize: "12.5px" }}
                >
                  <DownloadIcon size={14} />
                  <span>{downloadingDoc ? "Descargando..." : "Descargar PDF"}</span>
                </button>
              )}

              {canEdit && (
                <button
                  type="button"
                  className="acta-btn-sync"
                  onClick={handleGenerarDocumento}
                  disabled={generatingDoc}
                >
                  <RefreshIcon size={14} />
                  <span>{generatingDoc ? "Generando..." : "Sincronizar Documento"}</span>
                </button>
              )}
            </div>
          </div>

          <div className="acta-signature-panel">
            <div className="acta-signature-card">
              <div className="acta-signature-header">
                <span className="acta-signature-title">
                  Firma Manuscrita Digitalizada:
                </span>
                {signatureImage ? (
                  <span
                    style={{
                      display: "inline-flex",
                      alignItems: "center",
                      gap: "5px",
                      background: "var(--color-success-50)",
                      color: "var(--color-success-700)",
                      padding: "3px 10px",
                      borderRadius: "var(--radius-pill)",
                      fontSize: "12px",
                      fontWeight: 700,
                      border: "1px solid var(--color-success-100)",
                    }}
                  >
                    <CheckIcon size={14} />
                    Firma registrada y verificada
                  </span>
                ) : (
                  <span
                    style={{
                      display: "inline-flex",
                      alignItems: "center",
                      gap: "5px",
                      background: "var(--color-warning-50)",
                      color: "var(--color-warning-700)",
                      padding: "3px 10px",
                      borderRadius: "var(--radius-pill)",
                      fontSize: "12px",
                      fontWeight: 600,
                      border: "1px solid var(--color-warning-100)",
                    }}
                  >
                    <ClockIcon size={13} />
                    Pendiente de firma
                  </span>
                )}
              </div>

              <div className={`acta-signature-viewport ${signatureImage ? "signed" : ""}`}>
                {signatureImage ? (
                  <img
                    src={signatureImage}
                    alt="Firma del Consultor"
                    className="acta-signature-img"
                  />
                ) : (
                  <div className="acta-signature-empty">
                    <span style={{ fontSize: "13px", fontWeight: 600, color: "var(--text-secondary)" }}>
                      Firma pendiente por recolectar en campo
                    </span>
                    <span style={{ fontSize: "12px", color: "var(--text-muted)" }}>
                      El mensajero recolectará la firma digital en el dispositivo móvil durante la entrega o devolución.
                    </span>
                  </div>
                )}
              </div>

              <div className="acta-signature-footer">
                <span>
                  <strong>Fecha de Firma:</strong> {formatDate(acta.fechaFirma)}
                </span>
                <span>
                  <strong>Firmante:</strong> {acta.destinatarioNombre || "Consultor Receptor"}
                </span>
              </div>
            </div>
          </div>
        </section>


        {/* =================================================
            TARJETA 5: MOTIVO DE ANULACIÓN (SI APLICA)
        ================================================== */}

        {acta.motivoAnulacion && (
          <section className="acta-detail-card acta-detail-full" style={{ borderColor: "#fecdd3" }}>
            <div className="acta-card-header" style={{ background: "var(--color-danger-50)" }}>
              <div className="acta-card-title-group">
                <div className="acta-card-icon-wrap" style={{ background: "#ffe4e6", color: "var(--color-danger-600)" }}>
                  <CloseIcon size={18} />
                </div>
                <h2 style={{ color: "var(--color-danger-700)" }}>Motivo de Anulación del Acta</h2>
              </div>
            </div>

            <div className="acta-observation-box">
              <div className="acta-observation" style={{ borderColor: "var(--color-danger-500)", background: "#fff1f2", color: "var(--color-danger-700)" }}>
                {acta.motivoAnulacion}
              </div>
            </div>
          </section>
        )}

      </div>

      {/* =================================================
          MODAL FORMATO OFICIAL DE LA COMPAÑÍA
      ================================================== */}
      <OfficialActaModal
        acta={acta}
        isOpen={isOfficialModalOpen}
        onClose={() => setIsOfficialModalOpen(false)}
      />

      {/* =================================================
          MODAL ANULAR ACTA
      ================================================== */}
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
            animation: "actaFadeIn 0.2s ease",
          }}
          onClick={() => setIsAnularModalOpen(false)}
        >
          <div
            style={{
              background: "#ffffff",
              borderRadius: "16px",
              width: "100%",
              maxWidth: "480px",
              padding: "26px",
              boxShadow: "0 25px 50px -12px rgba(15, 23, 42, 0.25)",
              border: "1px solid var(--border-default)",
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
              <h2 style={{ margin: 0, fontSize: "18px", fontWeight: 800, color: "var(--text-primary)" }}>
                Anulación de Acta
              </h2>
              <button
                type="button"
                onClick={() => setIsAnularModalOpen(false)}
                style={{
                  background: "var(--color-slate-100)",
                  border: "none",
                  borderRadius: "var(--radius-sm)",
                  padding: "6px",
                  cursor: "pointer",
                  color: "var(--text-muted)",
                  display: "flex",
                  alignItems: "center",
                }}
              >
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleConfirmAnular}>
              <p style={{ fontSize: "13.5px", color: "var(--text-secondary)", margin: "0 0 16px 0", lineHeight: 1.5 }}>
                Ingresa el motivo detallado de la anulación para el acta <strong style={{ color: "var(--text-primary)" }}>{acta.numeroActa}</strong>. Si el equipo estaba asignado, regresará automáticamente a estado activo.
              </p>

              <label
                htmlFor="motivoAnulacion"
                style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "var(--text-secondary)", marginBottom: "6px", textTransform: "uppercase", letterSpacing: "0.5px" }}
              >
                Motivo de anulación *
              </label>
              <textarea
                id="motivoAnulacion"
                name="motivoAnulacion"
                required
                rows={3}
                value={motivoAnulacion}
                onChange={(e) => setMotivoAnulacion(e.target.value)}
                placeholder="Ej. Error en número de serie del equipo asignado..."
                style={{
                  width: "100%",
                  padding: "12px",
                  border: "1px solid var(--border-strong)",
                  borderRadius: "var(--radius-md)",
                  fontSize: "14px",
                  fontFamily: "inherit",
                  boxSizing: "border-box",
                  outline: "none",
                }}
              />

              <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginTop: "20px" }}>
                <button
                  type="button"
                  onClick={() => setIsAnularModalOpen(false)}
                  disabled={loadingAction}
                  style={{
                    padding: "8px 16px",
                    background: "var(--bg-surface)",
                    border: "1px solid var(--border-default)",
                    borderRadius: "var(--radius-md)",
                    color: "var(--text-secondary)",
                    fontWeight: 600,
                    fontSize: "13px",
                    cursor: "pointer",
                  }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={loadingAction}
                  style={{
                    padding: "8px 18px",
                    background: "var(--color-danger-600)",
                    color: "#ffffff",
                    border: "none",
                    borderRadius: "var(--radius-md)",
                    fontSize: "13px",
                    fontWeight: 700,
                    cursor: "pointer",
                    boxShadow: "0 2px 4px rgba(225, 29, 72, 0.25)",
                  }}
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

