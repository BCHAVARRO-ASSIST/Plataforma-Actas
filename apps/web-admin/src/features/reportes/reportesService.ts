import { API_URL } from "../../shared/config/api";

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
  equipmentInventoryKey: string | null;
  equipmentNombre: string | null;
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
  equipmentInventoryKey: string;
  equipmentNombre: string;
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
  equipmentInventoryKey: string;
  equipmentNombre: string;
  responsableEntrega: string | null;
  estado: string;
  fechaCreacion: string;
  fechaFirma: string | null;
}

const TOKEN_KEY = "plataforma_actas_token";

function getAuthHeaders(): HeadersInit {
  const token = localStorage.getItem(TOKEN_KEY);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export async function getEquiposReport(filter: ReportFilter): Promise<EquiposReportItem[]> {
  const res = await fetch(`${API_URL}/reportes/equipos`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(filter),
  });
  if (!res.ok) throw new Error("No fue posible cargar el reporte de equipos.");
  return res.json();
}

export async function getActasReport(filter: ReportFilter): Promise<ActasReportItem[]> {
  const res = await fetch(`${API_URL}/reportes/actas`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(filter),
  });
  if (!res.ok) throw new Error("No fue posible cargar el reporte de actas.");
  return res.json();
}

export async function getEntregasReport(filter: ReportFilter): Promise<EntregasReportItem[]> {
  const res = await fetch(`${API_URL}/reportes/entregas`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(filter),
  });
  if (!res.ok) throw new Error("No fue posible cargar el reporte de entregas.");
  return res.json();
}

export async function getDevolucionesReport(filter: ReportFilter): Promise<DevolucionesReportItem[]> {
  const res = await fetch(`${API_URL}/reportes/devoluciones`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(filter),
  });
  if (!res.ok) throw new Error("No fue posible cargar el reporte de devoluciones.");
  return res.json();
}

export function exportToCsv(
  filename: string,
  headers: string[],
  rows: (string | number | boolean | null | undefined)[][]
) {
  const escapeCell = (val: string | number | boolean | null | undefined) => {
    if (val === null || val === undefined) return '""';
    const str = String(val).replace(/"/g, '""');
    return `"${str}"`;
  };

  const csvContent =
    "\uFEFF" +
    [
      headers.map(escapeCell).join(";"),
      ...rows.map((row) => row.map(escapeCell).join(";")),
    ].join("\r\n");

  const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.setAttribute("href", url);
  link.setAttribute("download", `${filename}_${new Date().toISOString().slice(0, 10)}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

export function exportToPdf(
  title: string,
  headers: string[],
  rows: (string | number | boolean | null | undefined)[][]
) {
  const printWindow = window.open("", "_blank");
  if (!printWindow) {
    alert("Por favor habilita las ventanas emergentes para generar el PDF.");
    return;
  }

  const html = `
    <!DOCTYPE html>
    <html>
      <head>
        <title>${title}</title>
        <style>
          body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 20px; color: #0f172a; font-size: 11px; }
          .header { border-bottom: 2px solid #0f172a; padding-bottom: 12px; margin-bottom: 16px; display: flex; justify-content: space-between; align-items: flex-end; }
          h1 { margin: 0; font-size: 18px; color: #0f172a; }
          .meta { font-size: 10px; color: #64748b; }
          table { width: 100%; border-collapse: collapse; margin-top: 10px; }
          th { background-color: #f1f5f9; color: #334155; font-weight: 700; text-align: left; padding: 8px 6px; border-bottom: 1px solid #cbd5e1; font-size: 10px; text-transform: uppercase; }
          td { padding: 6px; border-bottom: 1px solid #e2e8f0; font-size: 10px; }
          tr:nth-child(even) { background-color: #f8fafc; }
          @media print {
            body { margin: 0; }
            @page { size: landscape; margin: 10mm; }
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div>
            <h1>PLATAFORMA DE ACTAS · REPORTE OFICIAL</h1>
            <div class="meta" style="margin-top: 4px;">${title} · Total registros: ${rows.length}</div>
          </div>
          <div class="meta">
            Fecha de emisión: ${new Date().toLocaleString("es-CO")}<br />
            Sistema de Trazabilidad y Custodia
          </div>
        </div>
        <table>
          <thead>
            <tr>
              ${headers.map((h) => `<th>${h}</th>`).join("")}
            </tr>
          </thead>
          <tbody>
            ${rows
              .map(
                (row) =>
                  `<tr>${row
                    .map((cell) => `<td>${cell ?? "—"}</td>`)
                    .join("")}</tr>`
              )
              .join("")}
          </tbody>
        </table>
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
