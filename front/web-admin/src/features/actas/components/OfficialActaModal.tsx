import { useEffect, useRef, useState } from "react";
import type { ActaDetail } from "../types/acta";
import { descargarActaDocumento, previewActaDocumento } from "../services/actaService";

interface OfficialActaModalProps {
  acta: ActaDetail;
  isOpen: boolean;
  onClose: () => void;
}

export function OfficialActaModal({ acta, isOpen, onClose }: OfficialActaModalProps) {
  const printRef = useRef<HTMLDivElement>(null);
  const [viewMode, setViewMode] = useState<"pdf" | "html">("pdf");
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [loadingPdf, setLoadingPdf] = useState(false);
  const [downloading, setDownloading] = useState(false);
  const [pdfError, setPdfError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      if (pdfUrl) {
        window.URL.revokeObjectURL(pdfUrl);
        setPdfUrl(null);
      }
      return;
    }

    let isMounted = true;
    async function loadPdfPreview() {
      try {
        setLoadingPdf(true);
        setPdfError(null);
        const blob = await previewActaDocumento(acta.id);
        if (isMounted) {
          const url = window.URL.createObjectURL(blob);
          setPdfUrl(url);
        }
      } catch (err) {
        if (isMounted) {
          setPdfError(err instanceof Error ? err.message : "Error al cargar la vista previa PDF.");
          setViewMode("html");
        }
      } finally {
        if (isMounted) {
          setLoadingPdf(false);
        }
      }
    }

    loadPdfPreview();

    return () => {
      isMounted = false;
      if (pdfUrl) {
        window.URL.revokeObjectURL(pdfUrl);
      }
    };
  }, [isOpen, acta.id]);

  if (!isOpen) return null;

  const equipo = acta.equipo;

  // Extraer firma en base64
  let signatureImage: string | null = null;
  if (acta.signatureDataJson) {
    try {
      const sigObj = JSON.parse(acta.signatureDataJson);
      signatureImage = sigObj.firma || sigObj.signature || null;
    } catch {
      signatureImage = null;
    }
  }

  // Extraer observaciones
  let observacion = "Sin observaciones registradas.";
  let accesorios: string[] = [];
  try {
    if (acta.formDataJson) {
      const formObj = JSON.parse(acta.formDataJson);
      observacion = formObj.observacion || formObj.observaciones || "Sin observaciones registradas.";
      if (Array.isArray(formObj.accesorios)) {
        accesorios = formObj.accesorios;
      }
    }
  } catch {
    observacion = "Sin observaciones registradas.";
  }

  const fechaCreacionFmt = acta.fechaCreacion
    ? new Date(acta.fechaCreacion).toLocaleDateString("es-CO", {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "—";

  const fechaFirmaFmt = acta.fechaFirma
    ? new Date(acta.fechaFirma).toLocaleDateString("es-CO", {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "Pendiente de firma";

  async function handleDownloadPdf() {
    try {
      setDownloading(true);
      await descargarActaDocumento(acta.id, acta.numeroActa);
    } catch (err) {
      alert(err instanceof Error ? err.message : "Error al descargar el PDF.");
    } finally {
      setDownloading(false);
    }
  }

  function handlePrint() {
    window.print();
  }

  return (
    <div
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: "rgba(15, 23, 42, 0.8)",
        backdropFilter: "blur(6px)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 99999,
        padding: "16px",
      }}
      onClick={onClose}
    >
      <div
        style={{
          background: "#ffffff",
          borderRadius: "16px",
          width: "100%",
          maxWidth: "960px",
          height: "92vh",
          display: "flex",
          flexDirection: "column",
          boxShadow: "0 25px 50px -12px rgba(0, 0, 0, 0.4)",
          overflow: "hidden",
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* BARRA DE HERRAMIENTAS SUPERIOR (NO SE IMPRIME) */}
        <div
          className="no-print"
          style={{
            padding: "14px 24px",
            background: "#0f172a",
            color: "#ffffff",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            borderBottom: "1px solid #334155",
            flexWrap: "wrap",
            gap: "12px",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
            <span style={{ fontSize: "22px" }}>📄</span>
            <div>
              <div style={{ fontSize: "14px", fontWeight: 700 }}>
                Documento Oficial TI-FOR-ACTA-001 — {acta.numeroActa}
              </div>
              <div style={{ fontSize: "12px", color: "#94a3b8" }}>
                {acta.tipoActa} de Hardware · Certificación digital y trazabilidad
              </div>
            </div>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
            {/* TOGGLE VISTA */}
            <div
              style={{
                display: "flex",
                background: "#1e293b",
                borderRadius: "8px",
                padding: "2px",
                border: "1px solid #334155",
              }}
            >
              <button
                type="button"
                onClick={() => setViewMode("pdf")}
                style={{
                  background: viewMode === "pdf" ? "#0284c7" : "transparent",
                  color: "#ffffff",
                  border: "none",
                  borderRadius: "6px",
                  padding: "6px 12px",
                  fontSize: "12px",
                  fontWeight: 600,
                  cursor: "pointer",
                }}
              >
                Visor PDF
              </button>
              <button
                type="button"
                onClick={() => setViewMode("html")}
                style={{
                  background: viewMode === "html" ? "#0284c7" : "transparent",
                  color: "#ffffff",
                  border: "none",
                  borderRadius: "6px",
                  padding: "6px 12px",
                  fontSize: "12px",
                  fontWeight: 600,
                  cursor: "pointer",
                }}
              >
                Formato HTML
              </button>
            </div>

            {/* BOTÓN DESCARGA DIRECTA */}
            <button
              type="button"
              onClick={handleDownloadPdf}
              disabled={downloading}
              style={{
                background: "#16a34a",
                color: "#ffffff",
                border: "none",
                borderRadius: "8px",
                padding: "8px 16px",
                fontSize: "13px",
                fontWeight: 700,
                cursor: downloading ? "wait" : "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                boxShadow: "0 2px 4px rgba(22, 163, 74, 0.3)",
              }}
            >
              <span>📥</span> {downloading ? "Descargando..." : "Descargar PDF"}
            </button>

            {/* BOTÓN IMPRIMIR */}
            {viewMode === "html" && (
              <button
                type="button"
                onClick={handlePrint}
                style={{
                  background: "#0284c7",
                  color: "#ffffff",
                  border: "none",
                  borderRadius: "8px",
                  padding: "8px 14px",
                  fontSize: "13px",
                  fontWeight: 700,
                  cursor: "pointer",
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
                <span>🖨</span> Imprimir
              </button>
            )}

            <button
              type="button"
              onClick={onClose}
              style={{
                background: "transparent",
                color: "#94a3b8",
                border: "1px solid #475569",
                borderRadius: "8px",
                padding: "8px 14px",
                fontSize: "13px",
                cursor: "pointer",
              }}
            >
              ✕ Cerrar
            </button>
          </div>
        </div>

        {/* CUERPO DEL MODAL */}
        <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden", position: "relative" }}>
          {viewMode === "pdf" ? (
            <div style={{ flex: 1, width: "100%", height: "100%", background: "#525659", display: "flex", flexDirection: "column" }}>
              {loadingPdf && (
                <div style={{ flex: 1, display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", color: "#ffffff", gap: "12px" }}>
                  <div style={{ fontSize: "28px" }}>⏳</div>
                  <div style={{ fontSize: "14px", fontWeight: 600 }}>Generando documento oficial PDF...</div>
                </div>
              )}

              {pdfError && (
                <div style={{ padding: "20px", color: "#f87171", background: "#1e293b", textAlign: "center" }}>
                  <p>{pdfError}</p>
                  <button
                    type="button"
                    onClick={() => setViewMode("html")}
                    style={{ background: "#0284c7", color: "#fff", border: "none", padding: "6px 12px", borderRadius: "6px", cursor: "pointer", marginTop: "8px" }}
                  >
                    Ver versión HTML institucional
                  </button>
                </div>
              )}

              {!loadingPdf && pdfUrl && (
                <iframe
                  src={pdfUrl}
                  title={`Vista Previa Acta ${acta.numeroActa}`}
                  style={{ width: "100%", height: "100%", border: "none" }}
                />
              )}
            </div>
          ) : (
            /* CONTENIDO DEL ACTA OFICIAL EN FORMATO HTML (IMPRIMIBLE) */
            <div
              style={{
                flex: 1,
                overflowY: "auto",
                padding: "30px 40px",
                background: "#ffffff",
              }}
              ref={printRef}
            >
              {/* MEMBRETE CORPORATIVO */}
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  borderBottom: "3px solid #0f172a",
                  paddingBottom: "16px",
                  marginBottom: "20px",
                }}
              >
                <div>
                  <div style={{ fontSize: "18px", fontWeight: 900, color: "#0f172a", letterSpacing: "0.5px" }}>
                    PLATAFORMA DE GESTIÓN Y CONTROL DE ACTIVOS TECNOLÓGICOS
                  </div>
                  <div style={{ fontSize: "12px", color: "#475569", fontWeight: 600, marginTop: "2px" }}>
                    DEPARTAMENTO DE TECNOLOGÍA E INFRAESTRUCTURA (TI)
                  </div>
                </div>

                <div style={{ textAlign: "right", fontSize: "11px", color: "#64748b" }}>
                  <div><strong>CÓDIGO:</strong> TI-FOR-ACTA-001</div>
                  <div><strong>VERSIÓN:</strong> 3.2 · ISO 27001</div>
                  <div><strong>PÁGINA:</strong> 1 de 1</div>
                </div>
              </div>

              {/* TÍTULO PRINCIPAL */}
              <div
                style={{
                  background: "#f1f5f9",
                  border: "1px solid #cbd5e1",
                  borderRadius: "8px",
                  padding: "12px 18px",
                  textAlign: "center",
                  marginBottom: "22px",
                }}
              >
                <h1 style={{ margin: 0, fontSize: "16px", fontWeight: 900, color: "#0f172a", textTransform: "uppercase" }}>
                  ACTA OFICIAL DE {acta.tipoActa === "Devolución" ? "DEVOLUCIÓN Y RECEPCIÓN" : "ENTREGA Y ASIGNACIÓN"} DE EQUIPO TECNOLÓGICO
                </h1>
                <div style={{ fontSize: "12px", color: "#475569", marginTop: "4px" }}>
                  N° DE CONTROL: <strong style={{ color: "#0f172a" }}>{acta.numeroActa}</strong>
                  {acta.actaOrigenNumero && (
                    <span style={{ marginLeft: "14px" }}>
                      (Acta de Entrega de Origen: <strong>{acta.actaOrigenNumero}</strong>)
                    </span>
                  )}
                </div>
              </div>

              {/* CUADRO DE DATOS GENERALES */}
              <div style={{ marginBottom: "20px" }}>
                <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a", textTransform: "uppercase", marginBottom: "8px", borderBottom: "1px solid #e2e8f0", paddingBottom: "4px" }}>
                  1. INFORMACIÓN DE LA OPERACIÓN Y PARTICIPANTES
                </div>

                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "12px" }}>
                  <tbody>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700, width: "22%" }}>Fecha de Emisión:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1", width: "28%" }}>{fechaCreacionFmt}</td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700, width: "22%" }}>Estado del Acta:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1", width: "28%" }}>
                        <strong>{acta.estado.toUpperCase()}</strong>
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Receptor / Consultor:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                        <strong>{acta.destinatarioNombre || equipo.asignadoA || "Consultor Asignado"}</strong>
                      </td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Correo / Cargo:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                        {acta.destinatarioCorreo || "—"} ({acta.destinatarioCargo || equipo.departamento || "Tecnología"})
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Responsable Entrega / Mensajero:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                        {acta.mensajeroNombre || "Personal de Logística TI"}
                      </td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Empresa / Sede:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                        {equipo.empresa || "Sede Principal"}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              {/* CUADRO TÉCNICO DEL EQUIPO */}
              <div style={{ marginBottom: "20px" }}>
                <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a", textTransform: "uppercase", marginBottom: "8px", borderBottom: "1px solid #e2e8f0", paddingBottom: "4px" }}>
                  2. ESPECIFICACIONES TÉCNICAS DEL HARDWARE Y ACCESORIOS
                </div>

                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "12px" }}>
                  <tbody>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700, width: "22%" }}>Código Inventario / Serie:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1", width: "28%" }}>
                        <strong style={{ color: "#0284c7" }}>{equipo.inventoryKey}</strong>
                      </td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700, width: "22%" }}>Nombre / Modelo:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1", width: "28%" }}>
                        <strong>{equipo.nombreEquipo}</strong>
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Marca:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>{equipo.marca || "—"}</td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Procesador:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>{equipo.procesador || "—"}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Memoria RAM:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>{equipo.memoriaRam || "—"}</td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Almacenamiento:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>{equipo.discoDuro || "—"}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Departamento:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>{equipo.departamento || "Tecnología"}</td>
                      <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Estado Operativo:</td>
                      <td style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                        <span style={{ fontWeight: 700, color: "#166534" }}>{equipo.estatusEquipo || "Activo"}</span>
                      </td>
                    </tr>
                    {accesorios.length > 0 && (
                      <tr>
                        <td style={{ padding: "6px 8px", background: "#f8fafc", border: "1px solid #cbd5e1", fontWeight: 700 }}>Accesorios Entregados:</td>
                        <td colSpan={3} style={{ padding: "6px 8px", border: "1px solid #cbd5e1" }}>
                          {accesorios.join(" · ")}
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>

              {/* OBSERVACIONES REGISTRADAS */}
              <div style={{ marginBottom: "20px" }}>
                <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a", textTransform: "uppercase", marginBottom: "8px", borderBottom: "1px solid #e2e8f0", paddingBottom: "4px" }}>
                  3. OBSERVACIONES DE ENTREGA / CONDICIÓN FÍSICA
                </div>
                <div
                  style={{
                    border: "1px solid #cbd5e1",
                    borderRadius: "6px",
                    padding: "10px 14px",
                    fontSize: "12px",
                    background: "#f8fafc",
                    color: "#1e293b",
                    lineHeight: "1.5",
                  }}
                >
                  {observacion}
                </div>
              </div>

              {/* CLÁUSULAS OFICIALES Y COMPROMISO DE DEVOLUCIÓN */}
              <div style={{ marginBottom: "24px" }}>
                <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a", textTransform: "uppercase", marginBottom: "8px", borderBottom: "1px solid #e2e8f0", paddingBottom: "4px" }}>
                  4. TÉRMINOS, CONDICIONES Y COMPROMISO DE DEVOLUCIÓN
                </div>
                <div
                  style={{
                    fontSize: "11px",
                    color: "#475569",
                    lineHeight: "1.6",
                    textAlign: "justify",
                    border: "1px solid #e2e8f0",
                    padding: "10px 12px",
                    borderRadius: "6px",
                    background: "#ffffff",
                  }}
                >
                  <p style={{ margin: "0 0 6px 0" }}>
                    <strong>A. CUSTODIA Y BUEN USO:</strong> El colaborador / consultor declara haber recibido el equipo anteriormente descrito en perfecto estado técnico y funcional, asumiendo su custodia, cuidado y utilización estricta conforme a las políticas de seguridad de la información corporativas.
                  </p>
                  <p style={{ margin: "0 0 6px 0" }}>
                    <strong>B. COMPROMISO FORMAL DE DEVOLUCIÓN:</strong> De esta misma acta se derivará el proceso de devolución. El colaborador se compromete expresamente a <strong>restituir el mismo activo tecnológico y sus accesorios</strong> a la organización cuando finalice su relación contractual, cuando se realice un cambio o cuando el área de TI lo requiera.
                  </p>
                  <p style={{ margin: 0 }}>
                    <strong>C. VALIDEZ JURÍDICA Y DIGITAL:</strong> La firma manuscrita digitalizada estampada en este documento y su hash SHA-256 constituyen plena prueba de recepción y aceptación de los términos establecidos.
                  </p>
                </div>
              </div>

              {/* SECCIÓN DE FIRMA Y CERTIFICACIÓN */}
              <div style={{ marginBottom: "22px" }}>
                <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a", textTransform: "uppercase", marginBottom: "12px", borderBottom: "1px solid #e2e8f0", paddingBottom: "4px" }}>
                  5. FIRMA DE CONFORMIDAD Y REGISTRO DIGITAL
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "24px" }}>
                  {/* FIRMA DEL CONSULTOR */}
                  <div
                    style={{
                      border: "1px solid #cbd5e1",
                      borderRadius: "8px",
                      padding: "12px",
                      textAlign: "center",
                      background: "#ffffff",
                    }}
                  >
                    <div style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", marginBottom: "8px" }}>
                      FIRMA DEL RECEPTOR / CONSULTOR
                    </div>

                    <div
                      style={{
                        height: "100px",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        borderBottom: "1px dashed #94a3b8",
                        marginBottom: "8px",
                        background: "#f8fafc",
                      }}
                    >
                      {signatureImage ? (
                        <img
                          src={signatureImage}
                          alt="Firma del Consultor"
                          style={{ maxHeight: "85px", maxWidth: "90%", objectFit: "contain" }}
                        />
                      ) : (
                        <span style={{ fontSize: "12px", color: "#94a3b8", fontStyle: "italic" }}>
                          {acta.estado === "Borrador" ? "Firma pendiente por recolectar" : "Firma registrada en sistema"}
                        </span>
                      )}
                    </div>

                    <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a" }}>
                      {acta.destinatarioNombre || equipo.asignadoA || "Consultor / Usuario"}
                    </div>
                    <div style={{ fontSize: "11px", color: "#64748b" }}>
                      Fecha y Hora de Firma: {fechaFirmaFmt}
                    </div>
                  </div>

                  {/* RESPONSABLE DE ENTREGA Y CERTIFICACIÓN */}
                  <div
                    style={{
                      border: "1px solid #cbd5e1",
                      borderRadius: "8px",
                      padding: "12px",
                      textAlign: "center",
                      background: "#ffffff",
                    }}
                  >
                    <div style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", marginBottom: "8px" }}>
                      RESPONSABLE DE ENTREGA / LOGÍSTICA TI
                    </div>

                    <div
                      style={{
                        height: "100px",
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                        justifyContent: "center",
                        borderBottom: "1px dashed #94a3b8",
                        marginBottom: "8px",
                        background: "#f8fafc",
                      }}
                    >
                      <span style={{ fontSize: "28px" }}>🛡️</span>
                      <span style={{ fontSize: "11px", color: "#166534", fontWeight: 700, marginTop: "4px" }}>
                        DOCUMENTO AUTORIZADO Y VALIDADO
                      </span>
                    </div>

                    <div style={{ fontSize: "12px", fontWeight: 800, color: "#0f172a" }}>
                      {acta.mensajeroNombre || "Operador de Logística TI"}
                    </div>
                    <div style={{ fontSize: "11px", color: "#64748b" }}>
                      Emisión: {fechaCreacionFmt}
                    </div>
                  </div>
                </div>
              </div>

              {/* SECCIÓN DE DEVOLUCIÓN CUANDO LLEGUE EL DÍA */}
              {acta.tipoActa === "Entrega" && (
                <div
                  style={{
                    border: "1px dashed #94a3b8",
                    borderRadius: "8px",
                    padding: "12px 14px",
                    background: "#fbfcfd",
                    marginTop: "16px",
                  }}
                >
                  <div style={{ fontSize: "11px", fontWeight: 800, color: "#475569", textTransform: "uppercase", marginBottom: "6px" }}>
                    6. DILIGENCIAMIENTO AL MOMENTO DE LA DEVOLUCIÓN FÍSICA FUTURA
                  </div>
                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "12px", fontSize: "11px", color: "#64748b" }}>
                    <div>Fecha Devolución: ____________________</div>
                    <div>Estado de Retorno: [ ] Óptimo  [ ] Avería</div>
                    <div>Recibido por (Firma TI): ____________________</div>
                  </div>
                </div>
              )}

              {/* PIE DE PÁGINA CON HASH SHA-256 */}
              {acta.hashSha256 && (
                <div
                  style={{
                    marginTop: "20px",
                    paddingTop: "12px",
                    borderTop: "1px solid #e2e8f0",
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    fontSize: "10px",
                    color: "#64748b",
                  }}
                >
                  <div>
                    <strong>INTEGRIDAD CRIPTOGRÁFICA (SHA-256):</strong>
                    <div style={{ fontFamily: "monospace", fontSize: "9px", color: "#0f172a", wordBreak: "break-all" }}>
                      {acta.hashSha256}
                    </div>
                  </div>
                  <div style={{ textAlign: "right", minWidth: "140px" }}>
                    Certificado Digitalmente
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
