import { useEffect, useState } from "react";
import type { DeliveryActaDetail as DeliveryActaDetailType } from "../deliveryService";
import { getDeliveryActaById } from "../deliveryService";
import { type ActaDetail } from "@/features/actas/actaService";
import { OfficialActaModal } from "@/features/actas/components/OfficialActaModal";
import { ConsultantSignatureView } from "@/features/actas/components/ConsultantSignatureView";
import {
  EquipmentIcon,
  UserIcon,
  MessengerIcon,
  SignatureIcon,
  DocumentPdfIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";

export interface DeliveryActaDetailProps {
  actaId: string;
  onBack: () => void;
}
export type CourierActaDetailProps = DeliveryActaDetailProps;
export type MensajeroActaDetailProps = DeliveryActaDetailProps;

function formatDate(value: string | null): string {
  if (!value) return "Pendiente";
  try {
    return new Date(value).toLocaleString("es-CO", { dateStyle: "medium", timeStyle: "short" });
  } catch {
    return value;
  }
}

export function DeliveryActaDetail({ actaId, onBack }: DeliveryActaDetailProps) {
  const [acta, setActa] = useState<DeliveryActaDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showConsultorView, setShowConsultorView] = useState(false);
  const [showPdfModal, setShowPdfModal] = useState(false);

  async function loadDetail() {
    try {
      setLoading(true);
      setError(null);
      const data = await getDeliveryActaById(actaId);
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

  if (showConsultorView && acta) {
    return (
      <ConsultantSignatureView
        actaId={actaId}
        numeroActa={acta.numeroActa || "ACT-ASIGNADA"}
        tipoActa={acta.tipoActa}
        consultorNombre={acta.destinatarioNombre || acta.equipo?.asignadoA || "Consultor Asignado"}
        consultorCorreo={acta.destinatarioCorreo || (acta.equipo?.departamento ? `${acta.equipo.departamento.toLowerCase()}@assist.com.co` : null)}
        consultorCargo={acta.destinatarioCargo || acta.equipo?.departamento}
        equipoNombre={acta.equipo?.nombreEquipo || acta.nombreEquipo || "Equipo TI"}
        equipoMarca={acta.equipo?.marca}
        equipoSerialOPlaca={acta.equipo?.inventoryKey || acta.inventoryKey || "Sin placa"}
        equipoSpecs={[acta.equipo?.procesador, acta.equipo?.memoriaRam, acta.equipo?.discoDuro].filter(Boolean).join(" · ")}
        onBack={() => setShowConsultorView(false)}
        onSignedSuccess={async () => {
          setShowConsultorView(false);
          await loadDetail();
        }}
      />
    );
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
          ) : (
            <div style={{ textAlign: "center", padding: "28px 16px", display: "flex", flexDirection: "column", alignItems: "center", gap: "12px" }}>
              <div style={{ width: "52px", height: "52px", borderRadius: "50%", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center", boxShadow: "0 0 0 6px var(--color-primary-100)" }}>
                <SignatureIcon size={26} />
              </div>
              <div>
                <h4 style={{ margin: "0 0 4px", fontSize: "16px", fontWeight: 700, color: "var(--text-primary)" }}>Firma Individual del Consultor</h4>
                <p style={{ margin: 0, fontSize: "13px", color: "var(--text-muted)", maxWidth: "460px" }}>
                  Entregue o muestre el dispositivo a <strong>{acta.equipo?.asignadoA || "el consultor"}</strong> para que revise los datos del activo y estampe su firma digital en la vista individual.
                </p>
              </div>
              <button
                type="button"
                className="btn btn-primary btn-lg"
                onClick={() => setShowConsultorView(true)}
                style={{ display: "inline-flex", alignItems: "center", gap: "8px", marginTop: "6px", padding: "12px 26px", fontSize: "14.5px", fontWeight: 700, borderRadius: "var(--radius-pill)" }}
              >
                <SignatureIcon size={18} />
                <span>Abrir Vista de Firma del Consultor</span>
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

// Retrocompatibilidad
export { DeliveryActaDetail as CourierActaDetail, DeliveryActaDetail as MensajeroActaDetail };


