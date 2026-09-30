import { apiPost } from "../../shared/api/httpClient";
import {
  ASSIST_LOGO_BASE64,
  ASSIST_COMPANY_NAME,
  ASSIST_SYSTEM_NAME,
} from "../../shared/assets/assistLogoBase64";

export type ReportTab = "equipos" | "actas" | "entregas" | "devoluciones";

export interface ReportFilter {
  fechaInicio?: string | null;
  fechaFin?: string | null;
  estado?: string | null;
  tipoActa?: string | null;
  equipmentId?: string | null;
  mensajeroId?: number | null;
  empresa?: string | null;
  marca?: string | null;
  search?: string | null;
}

export interface EquiposReportItem {
  id: string;
  inventoryKey: string;
  nombreEquipo: string;
  empresa: string | null;
  asignadoA: string | null;
  proveedor: string | null;
  marca: string | null;
  procesador: string | null;
  discoDuro: string | null;
  memoriaRam: string | null;
  departamento: string | null;
  estatusEquipo: string | null;
  actaEntregada: boolean;
  tipoMantenimiento: string | null;
  fechaMantenimiento: string | null;
}

export interface ActasReportItem {
  id: string;
  numeroActa: string;
  tipoActa: string;
  estado: string;
  equipmentId: string;
  inventoryKey: string | null;
  nombreEquipo: string | null;
  usuarioId: number;
  creadorNombre: string | null;
  destinatarioUsuarioId: number | null;
  destinatarioNombre: string | null;
  mensajeroUsuarioId: number | null;
  mensajeroNombre: string | null;
  actaEntregada: boolean;
  fechaCreacion: string;
  fechaFirma: string | null;
  fechaFinalizacion: string | null;
  fechaAnulacion: string | null;
  actaOrigenId: string | null;
}

export interface EntregasReportItem {
  id: string;
  numeroActa: string;
  inventoryKey: string;
  nombreEquipo: string;
  mensajeroNombre: string | null;
  destinatarioNombre: string | null;
  estado: string;
  firmado: boolean;
  fechaCreacion: string;
  fechaFirma: string | null;
}

export interface DevolucionesReportItem {
  id: string;
  numeroActa: string;
  actaOrigenId: string | null;
  actaOrigenNumero: string | null;
  inventoryKey: string;
  nombreEquipo: string;
  responsableEntrega: string | null;
  estado: string;
  fechaCreacion: string;
  fechaFirma: string | null;
}

export async function getEquiposReport(filter: ReportFilter): Promise<EquiposReportItem[]> {
  return apiPost<EquiposReportItem[]>("/reportes/equipos", filter, {
    errorMessage: "No fue posible cargar el reporte de equipos.",
  });
}

export async function getActasReport(filter: ReportFilter): Promise<ActasReportItem[]> {
  return apiPost<ActasReportItem[]>("/reportes/actas", filter, {
    errorMessage: "No fue posible cargar el reporte de actas.",
  });
}

export async function getEntregasReport(filter: ReportFilter): Promise<EntregasReportItem[]> {
  return apiPost<EntregasReportItem[]>("/reportes/entregas", filter, {
    errorMessage: "No fue posible cargar el reporte de entregas.",
  });
}

export async function getDevolucionesReport(filter: ReportFilter): Promise<DevolucionesReportItem[]> {
  return apiPost<DevolucionesReportItem[]>("/reportes/devoluciones", filter, {
    errorMessage: "No fue posible cargar el reporte de devoluciones.",
  });
}

export function exportToExcel(
  filename: string,
  title: string,
  headers: string[],
  rows: (string | number | boolean | null | undefined)[][],
  subtitle: string = ASSIST_SYSTEM_NAME
) {
  const timestamp = new Date().toLocaleString("es-CO");

  const excelHtml = `
    <html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40">
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <!--[if gte mso 9]>
        <xml>
          <x:ExcelWorkbook>
            <x:ExcelWorksheets>
              <x:ExcelWorksheet>
                <x:Name>${filename.slice(0, 31).replace(/[:\\\\/\\?\\*\\[\\]]/g, "_")}</x:Name>
                <x:WorksheetOptions>
                  <x:DisplayGridlines/>
                </x:WorksheetOptions>
              </x:ExcelWorksheet>
            </x:ExcelWorksheets>
          </x:ExcelWorkbook>
        </xml>
        <![endif]-->
        <style>
          body { font-family: 'Segoe UI', Calibri, Arial, sans-serif; font-size: 10pt; color: #0f172a; }
          .banner-main { background-color: #0f172a; color: #ffffff; font-size: 15pt; font-weight: bold; padding: 12px 10px; border-bottom: 2pt solid #2563eb; }
          .banner-sub { background-color: #1e293b; color: #cbd5e1; font-size: 10pt; padding: 6px 10px; }
          .banner-title { background-color: #f1f5f9; color: #0f172a; font-size: 12pt; font-weight: bold; padding: 8px 10px; border-bottom: 1pt solid #cbd5e1; }
          .banner-meta { background-color: #f8fafc; color: #475569; font-size: 9pt; padding: 6px 10px; border-bottom: 1pt solid #e2e8f0; }
          th { background-color: #1e293b; color: #ffffff; font-weight: bold; text-align: left; padding: 8px 6px; border: 0.5pt solid #0f172a; font-size: 9.5pt; text-transform: uppercase; }
          td { padding: 6px 8px; border: 0.5pt solid #cbd5e1; font-size: 9.5pt; mso-number-format: "\\@"; vertical-align: middle; }
          .row-even { background-color: #f8fafc; }
          .code-col { font-family: Consolas, monospace; font-weight: bold; color: #1d4ed8; }
          .status-col { text-align: center; font-weight: bold; }
          .footer-note { background-color: #f8fafc; color: #64748b; font-size: 8.5pt; font-style: italic; padding: 8px 10px; border-top: 1pt solid #cbd5e1; }
        </style>
      </head>
      <body>
        <table>
          <tr>
            <td colspan="${headers.length}" class="banner-main">
              ${ASSIST_COMPANY_NAME}
            </td>
          </tr>
          <tr>
            <td colspan="${headers.length}" class="banner-sub">
              ${subtitle}
            </td>
          </tr>
          <tr>
            <td colspan="${headers.length}" class="banner-title">
              REPORTE OFICIAL: ${title.toUpperCase()}
            </td>
          </tr>
          <tr>
            <td colspan="${headers.length}" class="banner-meta">
              Fecha y Hora de Emisión: ${timestamp} &nbsp;|&nbsp; Total de Registros: ${rows.length} &nbsp;|&nbsp; Clasificación: Documento de Uso Interno Exclusivo
            </td>
          </tr>
          <tr><td colspan="${headers.length}" style="height: 8px; border: none;"></td></tr>
          <thead>
            <tr>
              ${headers.map((h) => `<th>${h}</th>`).join("")}
            </tr>
          </thead>
          <tbody>
            ${rows
              .map(
                (row, rIdx) => `
              <tr class="${rIdx % 2 === 1 ? "row-even" : ""}">
                ${row
                  .map((cell, cIdx) => {
                    const isFirst = cIdx === 0;
                    const isLast = cIdx === headers.length - 1;
                    const cellVal = cell !== null && cell !== undefined && String(cell).trim() !== "" ? String(cell) : "—";
                    const cellClass = isFirst ? "code-col" : isLast ? "status-col" : "";
                    return `<td class="${cellClass}">${cellVal}</td>`;
                  })
                  .join("")}
              </tr>`
              )
              .join("")}
          </tbody>
          <tr><td colspan="${headers.length}" style="height: 10px; border: none;"></td></tr>
          <tr>
            <td colspan="${headers.length}" class="footer-note">
              Documento corporativo generado por la Plataforma de Gestión de Actas · Prohibida su alteración o uso externo sin autorización expresa de ${ASSIST_COMPANY_NAME}.
            </td>
          </tr>
        </table>
      </body>
    </html>
  `;

  const blob = new Blob([excelHtml], { type: "application/vnd.ms-excel;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.setAttribute("href", url);
  link.setAttribute("download", `${filename}_${new Date().toISOString().slice(0, 10)}.xlsx`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

export function exportToPdf(
  title: string,
  headers: string[],
  rows: (string | number | boolean | null | undefined)[][],
  subtitle: string = ASSIST_SYSTEM_NAME
) {
  const printWindow = window.open("", "_blank");
  if (!printWindow) {
    console.warn("No se pudo abrir la ventana emergente para exportar a PDF. Por favor habilita las ventanas emergentes en tu navegador.");
    return;
  }

  const timestamp = new Date().toLocaleString("es-CO");

  const html = `
    <!DOCTYPE html>
    <html lang="es">
      <head>
        <meta charset="utf-8" />
        <title>${title} - ${ASSIST_COMPANY_NAME}</title>
        <style>
          * { box-sizing: border-box; }
          body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            margin: 0;
            padding: 20px 24px;
            color: #0f172a;
            font-size: 10px;
            background: #ffffff;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
          }

          .no-print-bar {
            background: #0f172a;
            color: #ffffff;
            padding: 10px 16px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-radius: 6px;
            margin-bottom: 20px;
            box-shadow: 0 4px 12px rgba(15, 23, 42, 0.15);
          }
          .no-print-bar .btn-print {
            background: #2563eb;
            color: #ffffff;
            border: none;
            padding: 7px 16px;
            font-size: 12px;
            font-weight: 700;
            border-radius: 4px;
            cursor: pointer;
            transition: background 0.15s ease;
          }
          .no-print-bar .btn-print:hover {
            background: #1d4ed8;
          }
          .no-print-bar .btn-close {
            background: transparent;
            color: #94a3b8;
            border: 1px solid #475569;
            padding: 6px 12px;
            font-size: 12px;
            border-radius: 4px;
            cursor: pointer;
            margin-left: 8px;
          }

          .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 12px;
            border-bottom: 2.5px solid #0f172a;
            margin-bottom: 14px;
            gap: 16px;
          }

          .brand-box {
            display: flex;
            align-items: center;
            gap: 14px;
          }

          .brand-logo {
            height: 46px;
            max-width: 170px;
            object-fit: contain;
          }

          .brand-info h1 {
            margin: 0;
            font-size: 14px;
            font-weight: 800;
            color: #0f172a;
            letter-spacing: 0.3px;
            text-transform: uppercase;
          }

          .brand-info .system-name {
            font-size: 10px;
            color: #475569;
            margin-top: 2px;
            font-weight: 600;
          }

          .meta-box {
            text-align: right;
            font-size: 9.5px;
            color: #475569;
            line-height: 1.45;
          }

          .badge-report {
            display: inline-block;
            background: #0f172a;
            color: #ffffff;
            padding: 2px 8px;
            border-radius: 3px;
            font-size: 8.5px;
            font-weight: 800;
            letter-spacing: 0.6px;
            margin-bottom: 4px;
            text-transform: uppercase;
          }

          .report-heading {
            font-size: 13px;
            font-weight: 800;
            color: #0f172a;
            margin-bottom: 3px;
          }

          .accent-bar {
            height: 3px;
            background: linear-gradient(90deg, #2563eb 0%, #38bdf8 60%, transparent 100%);
            margin-top: -14px;
            margin-bottom: 14px;
          }

          table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 6px;
          }

          thead {
            display: table-header-group;
          }

          th {
            background-color: #1e293b;
            color: #f8fafc;
            font-weight: 700;
            text-align: left;
            padding: 7px 6px;
            border-top: 1px solid #0f172a;
            border-bottom: 1.5px solid #0f172a;
            font-size: 9px;
            text-transform: uppercase;
            letter-spacing: 0.4px;
          }

          td {
            padding: 5.5px 6px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 9px;
            color: #1e293b;
            vertical-align: middle;
          }

          tr:nth-child(even) {
            background-color: #f8fafc;
          }

          .cell-code {
            font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
            font-weight: 700;
            color: #1d4ed8;
          }

          .badge-status {
            display: inline-block;
            padding: 1.5px 6px;
            border-radius: 9999px;
            font-size: 8px;
            font-weight: 700;
            text-transform: capitalize;
            text-align: center;
          }

          .badge-active {
            background: #dcfce7;
            color: #15803d;
            border: 1px solid #86efac;
          }

          .badge-pending {
            background: #fef3c7;
            color: #b45309;
            border: 1px solid #fde68a;
          }

          .badge-neutral {
            background: #f1f5f9;
            color: #475569;
            border: 1px solid #cbd5e1;
          }

          .footer {
            margin-top: 18px;
            padding-top: 10px;
            border-top: 1px solid #cbd5e1;
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 8.5px;
            color: #64748b;
          }

          @media print {
            .no-print-bar {
              display: none !important;
            }
            body {
              padding: 0;
              margin: 0;
            }
            @page {
              size: landscape;
              margin: 8mm 10mm;
            }
            tr {
              page-break-inside: avoid;
            }
          }
        </style>
      </head>
      <body>
        <div class="no-print-bar">
          <div>
            <strong>Vista Previa de Impresión Oficial</strong> · ${title} (${rows.length} registros)
          </div>
          <div>
            <button class="btn-print" onclick="window.print()">🖨️ Imprimir / Guardar como PDF</button>
            <button class="btn-close" onclick="window.close()">Cerrar</button>
          </div>
        </div>

        <div class="header">
          <div class="brand-box">
            <img src="${ASSIST_LOGO_BASE64}" class="brand-logo" alt="ASSIST Consultores de Sistemas" />
            <div class="brand-info">
              <h1>${ASSIST_COMPANY_NAME}</h1>
              <div class="system-name">${subtitle}</div>
            </div>
          </div>
          <div class="meta-box">
            <div class="badge-report">REPORTE OFICIAL</div>
            <div class="report-heading">${title}</div>
            <div><strong>Emisión:</strong> ${timestamp}</div>
            <div><strong>Total Registros:</strong> ${rows.length} | <strong>Uso:</strong> Exclusivo Interno</div>
          </div>
        </div>
        <div class="accent-bar"></div>

        <table>
          <thead>
            <tr>
              ${headers.map((h) => `<th>${h}</th>`).join("")}
            </tr>
          </thead>
          <tbody>
            ${rows
              .map(
                (row) => {
                  return `<tr>${row
                    .map((cell, idx) => {
                      const isFirst = idx === 0;
                      const isLast = idx === headers.length - 1;
                      const strVal = cell !== null && cell !== undefined && String(cell).trim() !== "" ? String(cell) : "—";
                      
                      let content = strVal;
                      if (isLast && strVal !== "—") {
                        const lower = strVal.toLowerCase();
                        const badgeClass =
                          lower.includes("activo") || lower.includes("firmad") || lower.includes("finaliz")
                            ? "badge-active"
                            : lower.includes("pendient") || lower.includes("manten") || lower.includes("asigna")
                            ? "badge-pending"
                            : "badge-neutral";
                        content = `<span class="badge-status ${badgeClass}">${strVal}</span>`;
                      }

                      return `<td class="${isFirst ? "cell-code" : ""}">${content}</td>`;
                    })
                    .join("")}</tr>`;
                }
              )
              .join("")}
          </tbody>
        </table>

        <div class="footer">
          <div>Documento oficial generado por la Plataforma de Actas · Confidencial y de custodia interna institucional.</div>
          <div>${ASSIST_COMPANY_NAME} · Página 1</div>
        </div>
      </body>
    </html>
  `;

  printWindow.document.write(html);
  printWindow.document.close();
  printWindow.focus();
  setTimeout(() => {
    printWindow.print();
  }, 400);
}

