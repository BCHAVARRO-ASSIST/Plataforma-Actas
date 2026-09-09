import { useEffect, useState } from "react";
import type { MensajeroActaDetail as MensajeroActaDetailType } from "../features/mensajero/mensajeroService";
import { getMiActaById } from "../features/mensajero/mensajeroService";
import { firmarActa, type ActaDetail } from "../features/actas/actaService";
import { FirmaManuscrita } from "./FirmaManuscrita";
import { OfficialActaModal } from "./OfficialActaModal";
import {
  EquipmentIcon,
  UserIcon,
  MessengerIcon,
  SignatureIcon,
  DocumentPdfIcon,
} from "./Icons";
import { AutoDismissAlert } from "./AutoDismissAlert";

interface MensajeroActaDetailProps {
  actaId: string;
  onBack: () => void;
}

function formatDate(value: string | null): string {
  if (!value) return "Pendiente";
  try {
    return new Date(value).toLocaleString("es-CO", { dateStyle: "medium", timeStyle: "short" });
  } catch {
    return value;
  }
}

export function MensajeroActaDetail({ actaId, onBack }: MensajeroActaDetailProps) {
  const [acta, setActa] = useState<MensajeroActaDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [signing, setSigning] = useState(false);
  const [currentFirma, setCurrentFirma] = useState<string | null>(null);
  const [showSignaturePad, setShowSignaturePad] = useState(false);
  const [showPdfModal, setShowPdfModal] = useState(false);

  const [observacionesConsultor, setObservacionesConsultor] = useState("");

  async function loadDetail() {
    try {
      setLoading(true);
      setError(null);
      const data = await getMiActaById(actaId);
      setActa(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los detalles del acta.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadDetail();
  }, [actaId]);

  async function handleSaveSignature() {
    if (!currentFirma) {
      setError("Debes dibujar una firma antes de confirmar.");
      return;
    }

    try {
      setSigning(true);
      setError(null);
      await firmarActa(actaId, {
        signatureDataJson: currentFirma,
        observacion: observacionesConsultor.trim() || undefined,
      });
      setSuccess("Firma digital y observaciones registradas con éxito.");
      setShowSignaturePad(false);
      await loadDetail();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al registrar la firma.");
    } finally {
      setSigning(false);
    }
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando detalle de entrega...</span>
      </div>
    );
  }

  if (!acta) {
    return (
      <div className="card" style={{ padding: "32px", textAlign: "center" }}>
        <p style={{ color: "var(--text-muted)", marginBottom: "16px" }}>No fue posible encontrar la información del acta.</p>
        <button type="button" className="btn btn-secondary" onClick={onBack}>← Volver a mis entregas</button>
      </div>
    );
  }

  const isFirmada = acta.estado.toLowerCase() === "firmada" || acta.estado.toLowerCase() === "finalizada";

  return (
    <div>
      {/* Top Navigation */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "20px" }}>
        <button type="button" className="btn btn-ghost btn-sm" onClick={onBack}>
          ← Volver a mis entregas
        </button>

        <div style={{ display: "flex", gap: "8px" }}>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={() => setShowPdfModal(true)}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
          >
            <DocumentPdfIcon size={14} />
            <span>Ver Documento Oficial</span>
          </button>
        </div>
      </div>

      <AutoDismissAlert
        type="success"
        message={success}
        duration={4500}
        onDismiss={() => setSuccess(null)}
      />

      <AutoDismissAlert
        type="error"
        message={error}
        duration={5000}
        onDismiss={() => setError(null)}
      />

      {/* Hero Header Card */}
      <div className="mensajero-hero-card">
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--color-primary-300)", textTransform: "uppercase", letterSpacing: "1px" }}>
            ACTA DE ASIGNACIÓN / TRASLADO
          </span>
          <h1 style={{ fontSize: "26px", fontWeight: 800, margin: "4px 0", color: "#fff" }}>
            {acta.numeroActa || "ACTA EN CURSO"}
          </h1>
          <p style={{ margin: 0, fontSize: "13px", color: "var(--color-slate-300)" }}>
            Emitida el {formatDate(acta.fechaCreacion)} · Tipo: {acta.tipoActa}
          </p>
        </div>

        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          <span className={isFirmada ? "badge badge-success" : "badge badge-warning"}>
            <span className="badge-dot" />
            {acta.estado}
          </span>
          <span className={acta.actaEntregada ? "badge badge-success" : "badge badge-neutral"}>
            <span className="badge-dot" />
            {acta.actaEntregada ? "Entregada" : "Pendiente"}
          </span>
        </div>
      </div>

      {/* Detail Grid */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px", marginBottom: "24px" }}>
        {/* Hardware details */}
        <div className="card">
          <div className="card-header">
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <EquipmentIcon size={16} style={{ color: "var(--color-primary-600)" }} />
              <h3>Equipo Asignado</h3>
            </div>
            <span className="badge badge-neutral" style={{ fontFamily: 'var(--font-mono)' }}>{acta.equipo?.inventoryKey || acta.inventoryKey || "Sin placa"}</span>
          </div>
          <div className="card-body" style={{ display: "flex", flexDirection: "column", gap: "10px", fontSize: "13.5px" }}>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Nombre / Modelo</span>
              <strong style={{ color: "var(--text-primary)" }}>{acta.equipo?.nombreEquipo || acta.nombreEquipo}</strong>
            </div>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Marca / Fabricante</span>
              <span>{acta.equipo?.marca || "No especificada"}</span>
            </div>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Especificaciones (CPU / RAM / Disco)</span>
              <span style={{ color: "var(--text-secondary)" }}>
                {[acta.equipo?.procesador, acta.equipo?.memoriaRam, acta.equipo?.discoDuro].filter(Boolean).join(" · ") || "Estándar"}
              </span>
            </div>
          </div>
        </div>

        {/* Destinatario Details */}
        <div className="card">
          <div className="card-header">
            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
              <UserIcon size={16} style={{ color: "var(--color-primary-600)" }} />
              <h3>Destinatario / Receptor</h3>
            </div>
            <span className="badge badge-info">{acta.equipo?.departamento || "Empresa"}</span>
          </div>
          <div className="card-body" style={{ display: "flex", flexDirection: "column", gap: "10px", fontSize: "13.5px" }}>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Nombre / Asignado</span>
              <strong style={{ color: "var(--text-primary)" }}>{acta.equipo?.asignadoA || "Colaborador Asignado"}</strong>
            </div>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Empresa</span>
              <span>{acta.equipo?.empresa || "—"}</span>
            </div>
            <div>
              <span style={{ color: "var(--text-muted)", fontSize: "12px", display: "block" }}>Custodia y Entrega</span>
              <span style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
                <MessengerIcon size={14} style={{ color: "var(--text-muted)" }} />
                <span>Entrega en campo</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Signature Section */}
      <div className="card" style={{ marginBottom: "24px" }}>
        <div className="card-header">
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <SignatureIcon size={16} style={{ color: "var(--color-primary-600)" }} />
            <h3>Certificación y Firma Digital Manuscrita</h3>
          </div>
          {isFirmada ? (
            <span className="badge badge-success">Documento Certificado</span>
          ) : (
            <span className="badge badge-warning">Pendiente de firma</span>
          )}
        </div>

        <div className="card-body">
          {isFirmada ? (
            <div style={{ textAlign: "center", padding: "20px", color: "var(--color-success-700)" }}>
              El acta ya fue firmada digitalmente y validada el {formatDate(acta.fechaFirma)}.
            </div>
          ) : showSignaturePad ? (
            <div style={{ display: "flex", flexDirection: "column", gap: "18px" }}>
              {/* Cartel Informativo Institucional */}
              <div
                style={{
                  background: "var(--color-primary-50, #eff6ff)",
                  border: "1px solid var(--color-primary-100, #dbeafe)",
                  borderRadius: "10px",
                  padding: "12px 16px",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  flexWrap: "wrap",
                  gap: "10px",
                }}
              >
                <div>
                  <div style={{ fontSize: "13px", fontWeight: 700, color: "var(--color-primary-900, #1e3a8a)" }}>
                    Certificación de Entrega · ASSIST P8-FR-003
                  </div>
                  <div style={{ fontSize: "12px", color: "var(--color-primary-700, #1d4ed8)", marginTop: "2px" }}>
                    Firmante: <strong>{acta.equipo?.asignadoA || "Colaborador Asignado"}</strong> ({acta.equipo?.departamento || "Tecnología"})
                  </div>
                </div>
                <span className="badge badge-info" style={{ fontSize: "11.5px" }}>
                  Firma Digital Requerida
                </span>
              </div>

              {/* Observaciones Opcionales */}
              <div className="form-group" style={{ marginBottom: "0" }}>
                <label
                  className="form-label"
                  htmlFor="observacionesConsultor"
                  style={{ fontWeight: 700, fontSize: "13px", color: "var(--text-primary)", display: "flex", justifyContent: "space-between" }}
                >
                  <span>Observaciones del Receptor / Estado del Equipo</span>
                  <span style={{ fontWeight: 400, color: "var(--text-muted)", fontSize: "11.5px" }}>Opcional</span>
                </label>
                <textarea
                  id="observacionesConsultor"
                  name="observacionesConsultor"
                  className="form-input"
                  rows={2}
                  value={observacionesConsultor}
                  onChange={(e) => setObservacionesConsultor(e.target.value)}
                  placeholder="Comentarios sobre el estado físico, cargador, accesorios o novedades de entrega..."
                  disabled={signing}
                  style={{ width: "100%", height: "auto", padding: "10px 12px", fontSize: "13px", borderRadius: "8px" }}
                />
              </div>

              {/* Panel Delimitado de Firma Manuscrita */}
              <div>
                <FirmaManuscrita
                  onFirmaChange={setCurrentFirma}
                  disabled={signing}
                />
              </div>

              {/* Botonera de Acción */}
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  paddingTop: "12px",
                  borderTop: "1px solid var(--border-subtle, #f1f5f9)",
                }}
              >
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowSignaturePad(false)}
                  disabled={signing}
                >
                  Cancelar
                </button>

                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={handleSaveSignature}
                  disabled={signing || !currentFirma}
                  style={{ display: "inline-flex", alignItems: "center", gap: "8px", padding: "10px 20px" }}
                >
                  <SignatureIcon size={16} />
                  <span>{signing ? "Validando y guardando firma..." : "Confirmar y firmar acta"}</span>
                </button>
              </div>
            </div>
          ) : (
            <div style={{ textAlign: "center", padding: "24px 0" }}>
              <p style={{ color: "var(--text-muted)", fontSize: "13.5px", marginBottom: "16px" }}>
                El acta se encuentra en estado borrador. Recolecta la firma del colaborador en campo para certificar la entrega.
              </p>
              <button
                type="button"
                className="btn btn-primary btn-lg"
                onClick={() => setShowSignaturePad(true)}
                style={{ display: "inline-flex", alignItems: "center", gap: "8px", margin: "0 auto" }}
              >
                <SignatureIcon size={18} />
                <span>Abrir Panel de Firma Digital</span>
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Official Acta PDF Modal */}
      {showPdfModal && (
        <OfficialActaModal
          acta={acta as unknown as ActaDetail}
          isOpen={showPdfModal}
          onClose={() => setShowPdfModal(false)}
        />
      )}
    </div>
  );
}
