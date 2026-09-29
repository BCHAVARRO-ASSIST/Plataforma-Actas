import { useRef } from "react";
import type { ActaDetail } from "@/features/actas/actaService";
import {
  DocumentPdfIcon,
  CloseIcon,
} from "@/components/ui/Icons";
import { Modal } from "@/components/ui/Modal";
import { formatDateTime } from "@/shared/utils/date";
import { ASSIST_LOGO_BASE64 } from "@/shared/assets/assistLogoBase64";

interface OfficialActaModalProps {
  acta: ActaDetail;
  isOpen: boolean;
  onClose: () => void;
}

export function OfficialActaModal({ acta, isOpen, onClose }: OfficialActaModalProps) {
  const printRef = useRef<HTMLDivElement>(null);

  if (!isOpen) return null;

  const equipo = acta.equipo;
  const isDevolucion = acta.tipoActa === "Devolución" || acta.tipoActa?.toLowerCase().includes("devoluci");

  // Extraer firmas acumulativas en base64
  let firmaEntregaConsultor: string | null = null;
  let firmaDevolucionConsultor: string | null = null;
  if (acta.signatureDataJson) {
    try {
      const sigObj = JSON.parse(acta.signatureDataJson);
      firmaEntregaConsultor = sigObj.firmaEntregaConsultor || null;
      firmaDevolucionConsultor = sigObj.firmaDevolucionConsultor || null;
      const genericFirma = sigObj.firma || sigObj.signature || null;
      if (genericFirma) {
        if (isDevolucion) {
          firmaDevolucionConsultor = firmaDevolucionConsultor || genericFirma;
          firmaEntregaConsultor = firmaEntregaConsultor || genericFirma;
        } else {
          firmaEntregaConsultor = firmaEntregaConsultor || genericFirma;
        }
      }
    } catch {
      if (acta.signatureDataJson.startsWith("data:") || acta.signatureDataJson.length > 50) {
        if (isDevolucion) {
          firmaDevolucionConsultor = acta.signatureDataJson;
          firmaEntregaConsultor = acta.signatureDataJson;
        } else {
          firmaEntregaConsultor = acta.signatureDataJson;
        }
      }
    }
  }

  // Extraer observaciones acumulativas
  let observacionEntrega = acta.observacionesEntrega || "";
  let observacionDevolucion = acta.observacionesDevolucion || "";

  // Extraer Cédula / Documento de identidad (Prioridad 1: formDataJson manual -> Prioridad 2: destinatarioDocumento -> Prioridad 3: "—")
  let documentoCc = "";
  if (acta.formDataJson) {
    try {
      const formObj = JSON.parse(acta.formDataJson);
      documentoCc = formObj.numeroDocumento || formObj.cedula || formObj.documento || "";
    } catch {}
  }
  if (!documentoCc) {
    documentoCc = acta.destinatarioDocumento || "";
  }
  documentoCc = documentoCc || "—";

  // Extraer accesorios y trazabilidad (Entregado vs Devuelto)
  const canonicalAccessories: { key: string; label: string }[] = [
    { key: "mouse", label: "Mouse" },
    { key: "teclado", label: "Teclado" },
    { key: "base", label: "Base" },
    { key: "monitor", label: "Monitor" },
    { key: "guaya", label: "Guaya" },
    { key: "maletin", label: "Maletín" },
    { key: "discoDuroExterno", label: "Disco duro externo" },
  ];

  const accesoriosEstado: Record<string, { entregado: boolean; devuelto: boolean }> = {};
  canonicalAccessories.forEach(({ key }) => {
    accesoriosEstado[key] = { entregado: false, devuelto: false };
  });

  try {
    if (acta.formDataJson) {
      const formObj = JSON.parse(acta.formDataJson);
      if (!observacionEntrega) {
        observacionEntrega = formObj.observacionesEntrega || formObj.observacion || formObj.observaciones || "";
      }
      if (!observacionDevolucion) {
        observacionDevolucion = formObj.observacionesDevolucion || formObj.observacionDevolucion || "";
      }
      if (formObj.accesorios && typeof formObj.accesorios === "object" && !Array.isArray(formObj.accesorios)) {
        Object.entries(formObj.accesorios).forEach(([k, val]) => {
          const matchItem = canonicalAccessories.find(
            (c) => c.key.toLowerCase() === k.toLowerCase() || c.label.toLowerCase() === k.toLowerCase()
          );
          if (matchItem && typeof val === "object" && val !== null) {
            const vObj = val as { entregado?: boolean; devuelto?: boolean };
            accesoriosEstado[matchItem.key] = {
              entregado: Boolean(vObj.entregado),
              devuelto: Boolean(vObj.devuelto),
            };
          }
        });
      } else if (Array.isArray(formObj.accesorios)) {
        formObj.accesorios.forEach((item: string) => {
          const matchItem = canonicalAccessories.find(
            (c) => c.key.toLowerCase() === item.toLowerCase() || c.label.toLowerCase() === item.toLowerCase()
          );
          if (matchItem) {
            accesoriosEstado[matchItem.key] = {
              entregado: true,
              devuelto: isDevolucion,
            };
          }
        });
      }
    }
  } catch {
    // fallback
  }

  if (!observacionEntrega) {
    observacionEntrega = "Pendiente inspección física en laboratorio.";
  }
  if (!observacionDevolucion) {
    observacionDevolucion = "—";
  }

  const fechaCreacionFmt = formatDateTime(acta.fechaCreacion);
  const fechaFirmaFmt = formatDateTime(acta.fechaFirma, "Pendiente de firma");

  function handlePrint() {
    if (!printRef.current) return;
    const printWindow = window.open("", "_blank");
    if (!printWindow) {
      alert("Por favor habilita las ventanas emergentes para imprimir.");
      return;
    }

    const contentHtml = printRef.current.innerHTML;
    const fullHtml = `
      <!DOCTYPE html>
      <html lang="es">
        <head>
          <meta charset="utf-8" />
          <title>Acta_${acta.numeroActa}_ASSIST</title>
          <style>
            * { box-sizing: border-box; }
            body {
              font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
              margin: 0;
              padding: 16px;
              color: #0f172a;
              background: #ffffff;
              -webkit-print-color-adjust: exact;
              print-color-adjust: exact;
            }
            @media print {
              body { padding: 0; margin: 0; }
              @page { size: portrait; margin: 8mm; }
            }
          </style>
        </head>
        <body>
          <div>${contentHtml}</div>
        </body>
      </html>
    `;

    printWindow.document.write(fullHtml);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
      printWindow.print();
    }, 400);
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      size="custom"
      className="modal-overlay"
      contentClassName="modal-dialog acta-preview-modal"
      backdropStyle={{
        backgroundColor: "rgba(15, 23, 42, 0.75)",
        backdropFilter: "blur(4px)",
        WebkitBackdropFilter: "blur(4px)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 20000,
        padding: "24px",
      }}
      contentStyle={{
        background: "var(--bg-surface, #ffffff)",
        borderRadius: "12px",
        width: "100%",
        maxWidth: "900px",
        maxHeight: "92vh",
        display: "flex",
        flexDirection: "column",
        boxShadow: "0 25px 50px -12px rgba(0, 0, 0, 0.35)",
        overflow: "hidden",
        position: "relative",
        zIndex: 20001,
      }}
    >
      {/* BARRA DE HERRAMIENTAS SUPERIOR (NO SE IMPRIME) */}
      <div
        className="no-print modal-header acta-preview-header"
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
          flexShrink: 0,
        }}
      >
          <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
            <div style={{ width: "36px", height: "36px", borderRadius: "8px", background: "rgba(99, 102, 241, 0.2)", color: "#818cf8", display: "flex", alignItems: "center", justifyContent: "center" }}>
              <DocumentPdfIcon size={20} />
            </div>
            <div>
              <div style={{ fontSize: "14px", fontWeight: 700 }}>
                Documento Oficial P8-FR-003 — {acta.numeroActa}
              </div>
              <div style={{ fontSize: "12px", color: "#94a3b8" }}>
                {acta.tipoActa} de Hardware · ASSIST CONSULTORES DE SISTEMAS S.A.
              </div>
            </div>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
            <button
              type="button"
              onClick={handlePrint}
              style={{
                background: "var(--color-primary-600)",
                color: "#ffffff",
                border: "none",
                borderRadius: "8px",
                padding: "7px 14px",
                fontSize: "13px",
                fontWeight: 600,
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                transition: "all 0.15s ease",
              }}
              title="Imprimir o guardar documento como PDF"
            >
              <DocumentPdfIcon size={16} />
              <span>Imprimir / Guardar PDF</span>
            </button>

            <button
              type="button"
              onClick={onClose}
              style={{
                background: "transparent",
                color: "#94a3b8",
                border: "1px solid #475569",
                borderRadius: "8px",
                padding: "7px 12px",
                fontSize: "13px",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                transition: "all 0.15s ease",
              }}
              title="Cerrar vista previa"
            >
              <CloseIcon size={16} />
              <span>Cerrar</span>
            </button>
          </div>
        </div>

        {/* CUERPO DEL MODAL (DOCUMENTO OFICIAL SOBRE ESCRITORIO DIGITAL) */}
        <div className="modal-body acta-preview-body" style={{ flex: 1, overflowY: "auto", background: "var(--bg-app, #f1f5f9)", padding: "24px" }}>
          <div
            ref={printRef}
            style={{
              background: "#ffffff",
              maxWidth: "880px",
              margin: "0 auto",
              padding: "36px 44px",
              boxShadow: "0 10px 30px -5px rgba(15, 23, 42, 0.08), 0 0 0 1px rgba(15, 23, 42, 0.04)",
              borderRadius: "4px",
              color: "#0f172a",
              fontFamily: "var(--font-sans, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif)",
            }}
          >
            {/* 1. MEMBRETE CORPORATIVO OFICIAL ASSIST P8-FR-003 */}
            <div
              style={{
                border: "1.5px solid #1e293b",
                marginBottom: "14px",
                fontSize: "11px",
              }}
            >
              <div style={{ display: "grid", gridTemplateColumns: "200px 1fr", borderBottom: "1.5px solid #1e293b" }}>
                <div
                  style={{
                    padding: "8px 16px",
                    borderRight: "1.5px solid #1e293b",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    background: "#ffffff",
                  }}
                >
                  <img
                    src={ASSIST_LOGO_BASE64}
                    alt="ASSIST Consultores"
                    style={{ height: "42px", maxWidth: "100%", objectFit: "contain" }}
                  />
                </div>

                <div
                  style={{
                    padding: "10px 16px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontWeight: 800,
                    fontSize: "14.5px",
                    color: "#0f172a",
                    letterSpacing: "0.8px",
                    textTransform: "uppercase",
                  }}
                >
                  ACTA DE ENTREGA Y DEVOLUCIÓN
                </div>
              </div>

              <div
                style={{
                  display: "grid",
                  gridTemplateColumns: "1fr 1fr 1.5fr 1fr",
                  textAlign: "center",
                  fontSize: "10px",
                  background: "#f8fafc",
                  color: "#334155",
                }}
              >
                <div style={{ padding: "5px 4px", borderRight: "1px solid #1e293b", fontWeight: 700, color: "#0f172a" }}>
                  Código: P8-FR-003
                </div>
                <div style={{ padding: "5px 4px", borderRight: "1px solid #1e293b", fontWeight: 600 }}>
                  Versión: 2
                </div>
                <div style={{ padding: "5px 4px", borderRight: "1px solid #1e293b", fontWeight: 600 }}>
                  Fecha de actualización: 04/09/2024
                </div>
                <div style={{ padding: "5px 4px", fontWeight: 600 }}>
                  Página 1 de 1
                </div>
              </div>
            </div>

            {/* 2. DECLARACIÓN INICIAL DE SOPORTE */}
            <div style={{ fontSize: "11px", color: "#1e293b", marginBottom: "14px", lineHeight: "1.45" }}>
              El área de soporte de Assist Consultores de Sistemas S.A.{" "}
              <strong>Entrega</strong>{" "}
              <span style={{ fontWeight: 800, color: !isDevolucion ? "#1e40af" : "#94a3b8" }}>
                {!isDevolucion ? "[ X ]" : "[   ]"}
              </span>{" "}
              &nbsp;
              <strong>Recibe</strong>{" "}
              <span style={{ fontWeight: 800, color: isDevolucion ? "#1e40af" : "#94a3b8" }}>
                {isDevolucion ? "[ X ]" : "[   ]"}
              </span>{" "}
              &nbsp; el equipo de cómputo y/o accesorios con los siguientes datos anexos:
            </div>

            {/* 3. GRILLA PRINCIPAL: HARDWARE (IZQ) vs USUARIO Y ACCESORIOS (DER) */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1.15fr", gap: "16px", marginBottom: "14px" }}>
              {/* COLUMNA IZQUIERDA: ESPECIFICACIONES TÉCNICAS */}
              <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "10.5px" }}>
                <thead>
                  <tr>
                    <th
                      colSpan={2}
                      style={{
                        background: "#4b88be",
                        color: "#ffffff",
                        padding: "6px 8px",
                        textAlign: "center",
                        fontSize: "11px",
                        fontWeight: 700,
                        letterSpacing: "0.4px",
                        border: "1px solid #cbd5e1",
                      }}
                    >
                      ESPECIFICACIONES DEL HARDWARE
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700, width: "42%" }}>CPU Serial:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", color: "#1e40af", fontWeight: 700, fontFamily: "monospace", fontSize: "11px" }}>{equipo.inventoryKey}</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Monitor Serial:</td>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>—</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Cargador Serial:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>—</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Marca equipo:</td>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{equipo.marca || equipo.nombreEquipo}</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Marca monitor:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>—</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>MAC:</td>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>—</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Sistema Operativo:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>Windows 11 Pro / macOS</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Procesador:</td>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{equipo.procesador || "—"}</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Memoria Ram:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>{equipo.memoriaRam || "—"}</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Disco Duro:</td>
                    <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{equipo.discoDuro || "—"}</td>
                  </tr>
                  <tr>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Proveedor:</td>
                    <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>{equipo.proveedor || "Assist Consultores"}</td>
                  </tr>
                </tbody>
              </table>

              {/* COLUMNA DERECHA: INFORMACIÓN DEL USUARIO Y ACCESORIOS */}
              <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
                {/* TABLA USUARIO */}
                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "10.5px" }}>
                  <thead>
                    <tr>
                      <th
                        colSpan={2}
                        style={{
                          background: "#4b88be",
                          color: "#ffffff",
                          padding: "6px 8px",
                          textAlign: "center",
                          fontSize: "11px",
                          fontWeight: 700,
                          letterSpacing: "0.4px",
                          border: "1px solid #cbd5e1",
                        }}
                      >
                        INFORMACIÓN USUARIO (Quien recibe / devuelve el equipo)
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700, width: "42%" }}>Nombre del Usuario:</td>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700, color: "#0f172a" }}>{acta.destinatarioNombre || equipo.asignadoA || "Colaborador Asignado"}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Identificación (C.C):</td>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{documentoCc}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Fecha de entrega:</td>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>{fechaCreacionFmt}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Fecha devolución:</td>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{acta.tipoActa === "Devolución" ? fechaCreacionFmt : "—"}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1", fontWeight: 700 }}>Proyecto y/o Proceso:</td>
                      <td style={{ padding: "4px 8px", background: "#ffffff", border: "1px solid #cbd5e1" }}>{acta.destinatarioCargo || equipo.departamento || "Tecnología"}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1", fontWeight: 700 }}>Ciudad:</td>
                      <td style={{ padding: "4px 8px", background: "#edf5fb", border: "1px solid #cbd5e1" }}>{equipo.empresa || "Bogotá D.C."}</td>
                    </tr>
                  </tbody>
                </table>

                {/* TABLA ACCESORIOS */}
                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "10.5px" }}>
                  <thead>
                    <tr style={{ background: "#4b88be", color: "#ffffff", border: "1px solid #cbd5e1" }}>
                      <th style={{ padding: "5px 8px", textAlign: "left", fontSize: "10.5px" }}>ACCESORIOS</th>
                      <th style={{ padding: "5px 8px", textAlign: "center", fontSize: "10.5px", width: "85px" }}>Entregado</th>
                      <th style={{ padding: "5px 8px", textAlign: "center", fontSize: "10.5px", width: "85px" }}>Devuelto</th>
                    </tr>
                  </thead>
                  <tbody>
                    {canonicalAccessories.map((item, idx) => {
                      const isEven = idx % 2 === 1;
                      const estado = accesoriosEstado[item.key] || { entregado: false, devuelto: false };
                      const isEntregado = estado.entregado;
                      const isDevuelto = estado.devuelto;

                      return (
                        <tr key={item.key} style={{ background: isEven ? "#edf5fb" : "#ffffff", border: "1px solid #cbd5e1" }}>
                          <td style={{ padding: "3.5px 8px" }}>{item.label}</td>
                          <td style={{ padding: "3.5px 8px", textAlign: "center", fontWeight: 800, color: isEntregado ? "#1e40af" : "#94a3b8" }}>{isEntregado ? "[ X ]" : "[   ]"}</td>
                          <td style={{ padding: "3.5px 8px", textAlign: "center", fontWeight: 800, color: isDevuelto ? "#1e40af" : "#94a3b8" }}>{isDevuelto ? "[ X ]" : "[   ]"}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {/* 4. SEGUNDA FILA: SOFTWARE (IZQ) vs OBSERVACIONES Y CLÁUSULA (DER) */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1.15fr", gap: "16px", marginBottom: "14px" }}>
              {/* SOFTWARE INSTALADO */}
              <div>
                <div style={{ fontSize: "9px", color: "#475569", marginBottom: "5px", lineHeight: "1.35", textAlign: "justify" }}>
                  Listado de programas instalados, si necesita agregar un programa favor comunicar al área de soporte para su proceso, si el usuario instala programa(s) sin la debida autorización del área encargada, es responsabilidad del usuario del equipo ante una eventual auditoria. <strong>Ley 603 del 2000, Derecho de Propiedad Intelectual.</strong>
                </div>

                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "10px" }}>
                  <thead>
                    <tr style={{ background: "#4b88be", color: "#ffffff", border: "1px solid #cbd5e1" }}>
                      <th style={{ padding: "4.5px 8px", textAlign: "left" }}>LISTADO PROGRAMAS</th>
                      <th style={{ padding: "4.5px 8px", textAlign: "center", width: "80px" }}>INSTALADO</th>
                      <th style={{ padding: "4.5px 8px", textAlign: "center", width: "70px" }}>VERSION</th>
                    </tr>
                  </thead>
                  <tbody>
                    {[
                      { name: "IIB 10 / Broker 9/8/7", installed: false, version: "" },
                      { name: "Adobe Reader", installed: true, version: "DC" },
                      { name: "Soap UI", installed: false, version: "" },
                      { name: "Microsoft Office", installed: true, version: "365" },
                      { name: "Tortoise SVN", installed: false, version: "" },
                      { name: "Microsoft Project", installed: false, version: "" },
                      { name: "Microsoft Visio", installed: false, version: "" },
                      { name: "VmWare", installed: false, version: "" },
                      { name: "7zip", installed: true, version: "24.0" },
                      { name: "SQL Server", installed: false, version: "" },
                      { name: "Avast Antivirus", installed: true, version: "Corporate" },
                      { name: "Otro Software:", installed: false, version: "" },
                    ].map((s, idx) => (
                      <tr key={s.name} style={{ background: idx % 2 === 1 ? "#edf5fb" : "#ffffff", border: "1px solid #cbd5e1" }}>
                        <td style={{ padding: "2.5px 8px" }}>{s.name}</td>
                        <td style={{ padding: "2.5px 8px", textAlign: "center", fontWeight: 800, color: s.installed ? "#1e40af" : "#94a3b8" }}>{s.installed ? "[ X ]" : "[   ]"}</td>
                        <td style={{ padding: "2.5px 8px", textAlign: "center" }}>{s.version}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* OBSERVACIONES Y CLÁUSULA DE NÓMINA */}
              <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
                {/* OBSERVACIONES */}
                <div>
                  <div style={{ fontSize: "10.5px", fontWeight: 700, marginBottom: "3px", color: "#0f172a" }}>
                    Observaciones del estado de entrega / devolución del equipo:
                  </div>
                  <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "10px" }}>
                    <thead>
                      <tr style={{ background: "#edf5fb", border: "1px solid #cbd5e1" }}>
                        <th style={{ padding: "4px 8px", textAlign: "left", width: "50%", fontWeight: 700 }}>Entrega:</th>
                        <th style={{ padding: "4px 8px", textAlign: "left", width: "50%", fontWeight: 700 }}>Devolución:</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr style={{ border: "1px solid #cbd5e1" }}>
                        <td style={{ padding: "6px 8px", verticalAlign: "top", minHeight: "38px" }}>
                          {observacionEntrega}
                        </td>
                        <td style={{ padding: "6px 8px", verticalAlign: "top", minHeight: "38px", color: observacionDevolucion !== "—" ? "#0f172a" : "#64748b" }}>
                          {observacionDevolucion}
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                {/* CLÁUSULA DE NÓMINA */}
                <div style={{ border: "1px solid #cbd5e1", padding: "8px 10px", borderRadius: "4px", fontSize: "9.5px", lineHeight: "1.4", color: "#1e293b", textAlign: "justify", background: "#fafafa" }}>
                  <div style={{ fontWeight: 800, textTransform: "uppercase", marginBottom: "4px", color: "#0f172a", fontSize: "9.5px" }}>
                    AUTORIZACIÓN DE DESCUENTO POR NÓMINA O LIQUIDACIÓN EN CASO DE NO DEVOLUCIÓN, PÉRDIDA O DAÑO DE ELEMENTOS ENTREGADOS.
                  </div>
                  <p style={{ margin: "0 0 4px 0" }}>
                    Yo, <strong>{acta.destinatarioNombre || equipo.asignadoA || "Colaborador"}</strong> identificado(a) con cédula de ciudadanía No. <strong>{documentoCc}</strong>, por medio de la presente declaro que he recibido en condiciones óptimas y funcionales los elementos relacionados con anterioridad.
                  </p>
                  <p style={{ margin: "0 0 4px 0" }}>
                    Reconozco que estos elementos son propiedad de <strong>ASSIST CONSULTORES DE SISTEMAS S.A</strong> y que han sido entregados para el desarrollo de mis funciones laborales. Me comprometo a mantener en buen estado y hacer uso adecuado de dichos elementos.
                  </p>
                  <p style={{ margin: "0 0 6px 0" }}>
                    En caso de la no devolución, pérdida, hurto fuera de las instalaciones del cliente, oficina u hogar de los elementos entregados, daño o deterioro que no sea atribuible al desgaste normal por el uso, autorizo expresamente a <strong>ASSIST CONSULTORES DE SISTEMAS S.A</strong> para que, después de realizar una investigación interna que determine mi responsabilidad, se proceda a realizar el descuento correspondiente del valor de los elementos de mi salario, prestaciones sociales, liquidación final de prestaciones, o cualquier otro pago que me deba la empresa.
                  </p>
                  <p style={{ margin: 0, fontSize: "8.5px", fontStyle: "italic", color: "#64748b" }}>
                    Esta autorización se hace en cumplimiento de las normas laborales vigentes y es válida durante el tiempo que dure mi relación laboral con ASSIST CONSULTORES DE SISTEMAS S.A. He sido informado y entiendo completamente las condiciones establecidas en esta cláusula.
                  </p>
                </div>
              </div>
            </div>

            {/* 5. SECCIÓN DE FIRMAS */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px" }}>
              {/* FIRMAS DE ENTREGA */}
              <div style={{ border: "1px solid #cbd5e1", padding: "8px 12px", borderRadius: "4px", background: "#ffffff" }}>
                <div style={{ fontSize: "11px", fontWeight: 700, marginBottom: "3px", color: "#0f172a" }}>
                  Firmas de Entrega:
                </div>
                <div style={{ fontSize: "0.68rem", color: "#1e293b", marginBottom: "6px", lineHeight: "1.35", textAlign: "justify" }}>
                  Esta autorización se hace en cumplimiento de las normas laborales vigentes y es válida durante el tiempo que dure mi relación laboral con ASSIST CONSULTORES DE SISTEMAS S.A. He sido informado y entiendo completamente las condiciones establecidas en esta cláusula.
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                  <div style={{ border: "1px solid #e2e8f0", padding: "6px", textAlign: "center", borderRadius: "3px" }}>
                    <div style={{ fontSize: "10px", fontWeight: 700, color: "#64748b" }}>Entrega:</div>
                    <div style={{ height: "42px", display: "flex", alignItems: "center", justifyContent: "center" }}>
                      <img src="/brand/andres-trujillo-signature.png" alt="Firma Andrés Trujillo" style={{ maxHeight: "38px", maxWidth: "90%", objectFit: "contain" }} />
                    </div>
                    <div style={{ borderTop: "1px solid #94a3b8", paddingTop: "3px", fontSize: "9.5px" }}>
                      <div><strong>Nombre:</strong> Andrés Trujillo</div>
                      <div><strong>Cargo:</strong> Ingeniero Soporte</div>
                    </div>
                  </div>

                  <div style={{ border: "1px solid #e2e8f0", padding: "6px", textAlign: "center", borderRadius: "3px" }}>
                    <div style={{ fontSize: "10px", fontWeight: 700, color: "#64748b" }}>Recibe:</div>
                    <div style={{ height: "42px", display: "flex", alignItems: "center", justifyContent: "center" }}>
                      {firmaEntregaConsultor ? (
                        <img src={firmaEntregaConsultor} alt="Firma Entrega" style={{ maxHeight: "38px", maxWidth: "90%", objectFit: "contain" }} />
                      ) : (
                        <span style={{ fontSize: "10px", color: "#94a3b8", fontStyle: "italic" }}>
                          {isDevolucion ? "Firma registrada en entrega" : "Pendiente de firma"}
                        </span>
                      )}
                    </div>
                    <div style={{ borderTop: "1px solid #94a3b8", paddingTop: "3px", fontSize: "9.5px" }}>
                      <div><strong>Nombre:</strong> {acta.destinatarioNombre || equipo.asignadoA || "Colaborador"}</div>
                      <div><strong>Cargo:</strong> {acta.destinatarioCargo || equipo.departamento || "Tecnología"}</div>
                      <div style={{ fontSize: "8.5px", color: "#64748b" }}>Fecha firma: {fechaFirmaFmt}</div>
                    </div>
                  </div>
                </div>
              </div>

              {/* FIRMAS DE DEVOLUCIÓN */}
              <div style={{ border: "1px solid #cbd5e1", padding: "8px 12px", borderRadius: "4px", background: "#ffffff" }}>
                <div style={{ fontSize: "11px", fontWeight: 700, marginBottom: "2px", color: "#0f172a" }}>
                  Firmas de Devolución:
                </div>
                <div style={{ fontSize: "0.68rem", color: "#1e293b", marginBottom: "3px", lineHeight: "1.35" }}>
                  El equipo de soporte de la organización recibe a conformidad el equipo de cómputo devuelto por el colaborador:
                </div>
                <div style={{ fontSize: "9px", color: "#475569", marginBottom: "6px" }}>
                  Soporte recibe a conformidad: Si {isDevolucion ? "[ X ]" : "[   ]"} &nbsp; No [   ]
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                  <div style={{ border: "1px solid #e2e8f0", padding: "6px", textAlign: "center", borderRadius: "3px" }}>
                    <div style={{ fontSize: "10px", fontWeight: 700, color: "#64748b" }}>Entrega:</div>
                    <div style={{ height: "42px", display: "flex", alignItems: "center", justifyContent: "center" }}>
                      {isDevolucion ? (
                        firmaDevolucionConsultor ? (
                          <img src={firmaDevolucionConsultor} alt="Firma Devolución" style={{ maxHeight: "38px", maxWidth: "90%", objectFit: "contain" }} />
                        ) : (
                          <span style={{ fontSize: "10px", color: "#94a3b8", fontStyle: "italic" }}>Pendiente de firma</span>
                        )
                      ) : (
                        <div style={{ height: "42px" }} />
                      )}
                    </div>
                    <div style={{ borderTop: "1px solid #94a3b8", paddingTop: "3px", fontSize: "9.5px" }}>
                      <div><strong>Nombre:</strong> {isDevolucion ? (acta.destinatarioNombre || equipo.asignadoA || "Colaborador") : "_________________"}</div>
                      <div><strong>Cargo:</strong> {isDevolucion ? (acta.destinatarioCargo || equipo.departamento || "Consultoría") : "_________________"}</div>
                      {isDevolucion && <div style={{ fontSize: "8.5px", color: "#64748b" }}>Fecha firma: {fechaFirmaFmt}</div>}
                    </div>
                  </div>

                  <div style={{ border: "1px solid #e2e8f0", padding: "6px", textAlign: "center", borderRadius: "3px" }}>
                    <div style={{ fontSize: "10px", fontWeight: 700, color: "#64748b" }}>Recibe:</div>
                    <div style={{ height: "42px", display: "flex", alignItems: "center", justifyContent: "center" }}>
                      {isDevolucion ? (
                        <img src="/brand/andres-trujillo-signature.png" alt="Firma Andrés Trujillo" style={{ maxHeight: "38px", maxWidth: "90%", objectFit: "contain" }} />
                      ) : null}
                    </div>
                    <div style={{ borderTop: "1px solid #94a3b8", paddingTop: "3px", fontSize: "9.5px" }}>
                      <div><strong>Nombre:</strong> {isDevolucion ? "Andrés Trujillo" : "_________________"}</div>
                      <div><strong>Cargo:</strong> Ingeniero Soporte</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
    </Modal>
  );
}


