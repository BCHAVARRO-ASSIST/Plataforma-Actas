import { useState } from "react";
import { FirmaManuscrita } from "@/components/ui/FirmaManuscrita";
import { firmarActa } from "@/features/actas/actaService";
import {
  ArrowLeftIcon,
  CheckCircleIcon,
  EquipmentIcon,
  LockIcon,
  ShieldIcon,
  UserIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";
import { formatDate } from "@/shared/utils/date";
import { NotificacionDestinatarios } from "@/features/mensajero/components/NotificacionDestinatarios";

/**
 * Propiedades para la vista de captura de firma manuscrita y certificación en campo.
 */
export interface FirmaConsultorViewProps {
  /**
   * Identificador único del acta en proceso de formalización.
   */
  actaId: string;

  /**
   * Consecutivo oficial del acta (ej. ACTA-2026-0001).
   */
  numeroActa: string;

  /**
   * Modalidad del trámite operativo ("Entrega" o "Devolución").
   */
  tipoActa: string;

  /**
   * Nombre completo del colaborador que suscribe la responsabilidad del activo.
   */
  consultorNombre: string;

  /**
   * Correo electrónico corporativo registrado para la notificación del acta firmada.
   */
  consultorCorreo?: string | null;

  /**
   * Cargo o departamento del colaborador según la estructura organizacional.
   */
  consultorCargo?: string | null;

  /**
   * Denominación o modelo comercial del hardware entregado o devuelto.
   */
  equipoNombre: string;

  /**
   * Fabricante del equipo de cómputo o periférico.
   */
  equipoMarca?: string | null;

  /**
   * Serial de fábrica o placa de inventario corporativo del activo tecnológico.
   */
  equipoSerialOPlaca: string;

  /**
   * Ficha técnica o especificaciones complementarias del hardware.
   */
  equipoSpecs?: string | null;

  /**
   * Callback para regresar a la vista anterior cancelando el flujo de firma sin alterar el borrador.
   */
  onBack: () => void;

  /**
   * Callback invocado tras la persistencia exitosa de la firma y confirmación del trámite en servidor.
   */
  onSignedSuccess: () => Promise<void> | void;
}

/**
 * Vista de interfaz móvil y de escritorio para la captura de rúbrica biométrica manuscrita y confirmación de entrega en sitio.
 *
 * @remarks
 * **Propósito Operativo y Validez Jurídica:**
 * Esta interfaz está diseñada para operar tanto en dispositivos móviles en campo (utilizados por mensajería)
 * como en puestos de trabajo de entrega. Sus responsabilidades críticas incluyen:
 * 1. **Revisión de Custodia:** Exposición clara y sin ambigüedades de los datos del colaborador y el activo asignado antes de firmar.
 * 2. **Captura Manuscrita Directa:** Recolección de trazos táctiles o mediante puntero sobre canvas HTML5 normalizados a Data-URI.
 * 3. **Consentimiento Expreso ISO 27001:** Exigencia obligatoria del checkbox de aceptación de la cláusula de confidencialidad
 *    y buen uso de herramientas de trabajo conforme a la normativa colombiana (Ley 603 de 2000).
 * 4. **Canal de Notificación en Campo:** Integración con {@link NotificacionDestinatarios} para despachar la copia certificada
 *    en tiempo real al finalizar el acto presencial de entrega.
 *
 * @param props - Propiedades de inicialización de la vista {@link FirmaConsultorViewProps}.
 */
export function FirmaConsultorView({
  actaId,
  numeroActa,
  tipoActa,
  consultorNombre,
  consultorCorreo,
  consultorCargo,
  equipoNombre,
  equipoMarca,
  equipoSerialOPlaca,
  equipoSpecs,
  onBack,
  onSignedSuccess,
}: FirmaConsultorViewProps) {
  const [currentFirma, setCurrentFirma] = useState<string | null>(null);
  const [observaciones, setObservaciones] = useState("");
  const [consentAccepted, setConsentAccepted] = useState(true);
  const [signing, setSigning] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [signedCompleted, setSignedCompleted] = useState(false);
  const [correosAdicionales, setCorreosAdicionales] = useState<string[]>([]);

  const isDevolucion = tipoActa.toLowerCase().includes("devoluci");

  async function handleConfirmSignature() {
    if (!currentFirma) {
      setError("Por favor dibuje su firma en el recuadro antes de confirmar.");
      return;
    }

    if (!consentAccepted) {
      setError("Debe marcar la casilla de aceptación para proceder con la firma.");
      return;
    }

    try {
      setSigning(true);
      setError(null);

      await firmarActa(actaId, {
        signatureDataJson: JSON.stringify({ firma: currentFirma }),
        observacion: observaciones.trim() || undefined,
        correosNotificacionAdicionales: correosAdicionales.length > 0 ? correosAdicionales : undefined,
      });

      setSignedCompleted(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al procesar la firma del acta.");
      setSigning(false);
    }
  }

  async function handleFinish() {
    await onSignedSuccess();
  }

  // Pantalla de Éxito / Confirmación
  if (signedCompleted) {
    return (
      <div className="firma-consultor-overlay">
        <div className="firma-consultor-success-card">
          <div className="firma-success-badge">
            <CheckCircleIcon size={48} />
          </div>

          <span className="firma-success-tag">Certificación Digital Completada</span>
          <h2 className="firma-success-title">¡Acta Firmada Exitosamente!</h2>

          <p className="firma-success-desc">
            La firma manuscrita de <strong>{consultorNombre}</strong> ha sido registrada
            y vinculada oficialmente al acta <strong>{numeroActa}</strong>.
          </p>

          <div className="firma-success-receipt">
            <div className="firma-receipt-row">
              <span>Acta:</span>
              <strong>{numeroActa}</strong>
            </div>
            <div className="firma-receipt-row">
              <span>Operación:</span>
              <strong>{tipoActa}</strong>
            </div>
            <div className="firma-receipt-row">
              <span>Equipo:</span>
              <strong>{equipoNombre} ({equipoSerialOPlaca})</strong>
            </div>
            <div className="firma-receipt-row">
              <span>Consultor:</span>
              <strong>{consultorNombre}</strong>
            </div>
            <div className="firma-receipt-row">
              <span>Fecha / Hora:</span>
              <strong>{new Date().toLocaleString("es-CO", { dateStyle: "medium", timeStyle: "short" })}</strong>
            </div>
          </div>

          <button
            type="button"
            className="btn btn-primary"
            onClick={handleFinish}
            style={{ width: "100%", padding: "14px", fontSize: "15px", fontWeight: 700, borderRadius: "var(--radius-lg)" }}
          >
            Finalizar y Volver al Sistema
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="firma-consultor-overlay">
      <div className="firma-consultor-container">
        {/* Topbar de la Vista Individual */}
        <header className="firma-consultor-topbar">
          <button
            type="button"
            className="btn btn-ghost btn-sm firma-btn-cancel"
            onClick={onBack}
            disabled={signing}
          >
            <ArrowLeftIcon size={16} />
            <span>Cancelar / Volver</span>
          </button>

          <div className="firma-topbar-brand">
            <div className="app-brand-mark" style={{ width: "26px", height: "26px", fontSize: "11px" }}>PA</div>
            <span className="firma-brand-title">Certificación y Firma de Activo TI</span>
          </div>

          <span className="badge badge-info" style={{ fontFamily: "var(--font-mono)", fontSize: "12px" }}>
            {numeroActa}
          </span>
        </header>

        {/* Alertas */}
        <AutoDismissAlert
          type="error"
          message={error}
          duration={5000}
          onDismiss={() => setError(null)}
        />

        {/* Contenido Principal */}
        <main className="firma-consultor-main">
          {/* Banner de Bienvenida y Propósito */}
          <div className="firma-banner">
            <div className="firma-banner-header">
              <span className={`badge ${isDevolucion ? "badge-warning" : "badge-primary"}`} style={{ fontWeight: 700 }}>
                ACTA DE {isDevolucion ? "DEVOLUCIÓN" : "ENTREGA"}
              </span>
              <span className="firma-banner-date">
                {formatDate(new Date(), { day: "2-digit", month: "long", year: "numeric" })}
              </span>
            </div>
            <h1 className="firma-banner-title">
              {isDevolucion ? "Devolución y Entrega de Hardware" : "Recepción y Asignación de Hardware"}
            </h1>
            <p className="firma-banner-desc">
              Por favor revise la información del colaborador y las características del activo antes de estampar su firma digital en la parte inferior.
            </p>
          </div>

          {/* Tarjetas de Información: Consultor y Equipo */}
          <div className="firma-cards-grid">
            {/* Tarjeta del Consultor */}
            <div className="firma-info-card">
              <div className="firma-card-header">
                <UserIcon size={18} style={{ color: "var(--color-primary-600)" }} />
                <h3>Consultor / Colaborador</h3>
              </div>
              <div className="firma-card-body">
                <div className="firma-field">
                  <span className="firma-label">Nombre Completo</span>
                  <strong className="firma-value-primary">{consultorNombre}</strong>
                </div>
                {consultorCorreo && (
                  <div className="firma-field">
                    <span className="firma-label">Correo Corporativo</span>
                    <span className="firma-value">{consultorCorreo}</span>
                  </div>
                )}
                <div className="firma-field">
                  <span className="firma-label">Departamento / Área</span>
                  <span className="firma-value">{consultorCargo || "Consultoría / Operaciones"}</span>
                </div>
              </div>
            </div>

            {/* Tarjeta del Equipo */}
            <div className="firma-info-card">
              <div className="firma-card-header">
                <EquipmentIcon size={18} style={{ color: "var(--color-primary-600)" }} />
                <h3>Detalle del Activo Tecnológico</h3>
              </div>
              <div className="firma-card-body">
                <div className="firma-field">
                  <span className="firma-label">Equipo / Modelo</span>
                  <strong className="firma-value-primary">{equipoNombre}</strong>
                </div>
                <div className="firma-field-row">
                  <div>
                    <span className="firma-label">Marca</span>
                    <span className="firma-value">{equipoMarca || "No especificada"}</span>
                  </div>
                  <div>
                    <span className="firma-label">Placa / Serial</span>
                    <span className="firma-value-mono">{equipoSerialOPlaca}</span>
                  </div>
                </div>
                {equipoSpecs && (
                  <div className="firma-field">
                    <span className="firma-label">Especificaciones de Hardware</span>
                    <span className="firma-value" style={{ fontSize: "12px" }}>{equipoSpecs}</span>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Cláusula Legal de Consentimiento */}
          <div className="firma-clause-box">
            <div className="firma-clause-icon">
              <ShieldIcon size={20} />
            </div>
            <div className="firma-clause-content">
              <h4>Declaración de Conformidad y Responsabilidad</h4>
              <p>
                {isDevolucion
                  ? "Certifico mediante mi firma que realizo la entrega física y formal del equipo de cómputo y accesorios listados en este documento a la organización, libre de contraseñas de booteo o bloqueos personales."
                  : "Declaro haber recibido a entera satisfacción el activo tecnológico detallado en este documento, en perfecto estado de funcionamiento, asumiendo la responsabilidad y custodia del mismo conforme a los estándares y políticas de seguridad de la información corporativas."}
              </p>
            </div>
          </div>

          {/* Observaciones Opcionales */}
          <div className="form-group" style={{ marginBottom: "18px" }}>
            <label className="form-label" htmlFor="consultorObservaciones" style={{ fontWeight: 600, color: "var(--text-primary)" }}>
              Observaciones del Consultor (Opcional)
            </label>
            <input
              id="consultorObservaciones"
              type="text"
              className="form-input"
              placeholder="Ej. Recibido con cargador original y mouse en buen estado..."
              value={observaciones}
              onChange={(e) => setObservaciones(e.target.value)}
              disabled={signing}
            />
          </div>

          {/* Notificación de Destinatarios y Copias */}
          <NotificacionDestinatarios
            consultorCorreo={consultorCorreo}
            consultorNombre={consultorNombre}
            onAdditionalEmailsChange={setCorreosAdicionales}
            disabled={signing}
          />

          {/* Lienzo de Firma Digital Manuscrita */}
          <div className="firma-section-box">
            <div className="firma-section-title">
              Firma Digital Manuscrita del Consultor
            </div>
            <p className="firma-section-hint">
              Utilice su dedo, lápiz táctil o ratón para firmar en el espacio en blanco:
            </p>

            <FirmaManuscrita
              onFirmaChange={(firma) => setCurrentFirma(firma)}
              disabled={signing}
            />
          </div>

          {/* Casilla de Consentimiento */}
          <div className="firma-consent-wrapper">
            <label className="firma-checkbox-label">
              <input
                type="checkbox"
                className="firma-checkbox"
                checked={consentAccepted}
                onChange={(e) => setConsentAccepted(e.target.checked)}
                disabled={signing}
              />
              <span>
                Confirmo que los datos consignados son verídicos y acepto los términos de esta acta.
              </span>
            </label>
          </div>

          {/* Botones de Acción */}
          <div className="firma-actions-bar">
            <button
              type="button"
              className="btn btn-secondary firma-btn-secondary"
              onClick={onBack}
              disabled={signing}
            >
              Cancelar
            </button>

            <button
              type="button"
              className="btn btn-primary firma-btn-submit"
              onClick={handleConfirmSignature}
              disabled={signing || !currentFirma || !consentAccepted}
            >
              {signing ? (
                <span style={{ display: "inline-flex", alignItems: "center", gap: "8px" }}>
                  <span className="spinner" style={{ width: "16px", height: "16px", border: "2px solid #fff", borderTopColor: "transparent", borderRadius: "50%", animation: "spin 0.8s linear infinite" }} />
                  Certificando y Firmando...
                </span>
              ) : (
                <span style={{ display: "inline-flex", alignItems: "center", gap: "8px" }}>
                  <LockIcon size={16} />
                  <span>Confirmar y Firmar Acta</span>
                </span>
              )}
            </button>
          </div>
        </main>
      </div>
    </div>
  );
}
