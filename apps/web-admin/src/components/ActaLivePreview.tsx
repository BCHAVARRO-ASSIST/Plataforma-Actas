import type { Equipment } from "../features/equipment/equipmentService";
import type { Usuario } from "../features/usuarios/usuarioService";
import { ActaIcon, CheckIcon } from "./Icons";

interface ActaLivePreviewProps {
  actaType: "Entrega" | "Devolución";
  actaOrigenId?: string;
  selectedEquipment?: Equipment;
  selectedDestinatario?: Usuario;
  selectedMensajero?: Usuario;
  isValid: boolean;
  saving: boolean;
  onCancel: () => void;
}

export function ActaLivePreview({
  actaType,
  actaOrigenId,
  selectedEquipment,
  selectedDestinatario,
  selectedMensajero,
  isValid,
  saving,
  onCancel,
}: ActaLivePreviewProps) {
  const tieneEquipo = Boolean(selectedEquipment);
  const tieneDestinatario = Boolean(selectedDestinatario);
  const tieneOrigen = actaType === "Entrega" || Boolean(actaOrigenId);

  return (
    <aside style={{ position: "sticky", top: "20px", display: "flex", flexDirection: "column", gap: "16px" }}>
      {/* Resumen Principal */}
      <div
        className="card"
        style={{
          padding: "20px",
          border: "1px solid var(--color-primary-200)",
          background: "linear-gradient(180deg, var(--color-primary-50) 0%, var(--bg-surface) 35%)",
          boxShadow: "var(--shadow-md)",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "16px" }}>
          <div
            style={{
              width: "36px",
              height: "36px",
              borderRadius: "var(--radius-md)",
              background: "var(--color-primary-600)",
              color: "#ffffff",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <ActaIcon size={18} />
          </div>
          <div>
            <span style={{ fontSize: "10px", fontWeight: 800, color: "var(--color-primary-700)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
              TI-FOR-ACTA-001
            </span>
            <h3 style={{ margin: 0, fontSize: "15px", fontWeight: 800, color: "var(--text-primary)" }}>
              Resumen del Trámite
            </h3>
          </div>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: "10px", fontSize: "12.5px" }}>
          {/* Tipo de Acta */}
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", paddingBottom: "8px", borderBottom: "1px solid var(--border-subtle)" }}>
            <span style={{ color: "var(--text-muted)", fontWeight: 500 }}>Tipo de Acta:</span>
            <span
              style={{
                fontWeight: 700,
                color: "var(--color-primary-700)",
                background: "var(--color-primary-100)",
                padding: "2px 8px",
                borderRadius: "var(--radius-pill)",
                fontSize: "11.5px",
              }}
            >
              {actaType}
            </span>
          </div>

          {/* Equipo */}
          <div style={{ display: "flex", flexDirection: "column", gap: "2px", paddingBottom: "8px", borderBottom: "1px solid var(--border-subtle)" }}>
            <span style={{ color: "var(--text-muted)", fontWeight: 500 }}>Equipo Tecnológico:</span>
            <strong style={{ color: selectedEquipment ? "var(--text-primary)" : "var(--color-danger-600)" }}>
              {selectedEquipment
                ? `${selectedEquipment.inventoryKey} — ${selectedEquipment.nombreEquipo}`
                : "Sin seleccionar *"}
            </strong>
            {selectedEquipment && (
              <span style={{ fontSize: "11px", color: "var(--text-muted)" }}>
                {selectedEquipment.marca ?? "Marca general"} · {selectedEquipment.estatusEquipo ?? "Activo"}
              </span>
            )}
          </div>

          {/* Destinatario */}
          <div style={{ display: "flex", flexDirection: "column", gap: "2px", paddingBottom: "8px", borderBottom: "1px solid var(--border-subtle)" }}>
            <span style={{ color: "var(--text-muted)", fontWeight: 500 }}>Destinatario / Consultor:</span>
            <strong style={{ color: selectedDestinatario ? "var(--text-primary)" : "var(--color-danger-600)" }}>
              {selectedDestinatario
                ? `${selectedDestinatario.nombre} ${selectedDestinatario.apellido}`
                : "Sin seleccionar *"}
            </strong>
            {selectedDestinatario && (
              <span style={{ fontSize: "11px", color: "var(--text-muted)" }}>
                {selectedDestinatario.correo} ({selectedDestinatario.usuarioRed})
              </span>
            )}
          </div>

          {/* Mensajero */}
          <div style={{ display: "flex", flexDirection: "column", gap: "2px", paddingBottom: "8px", borderBottom: "1px solid var(--border-subtle)" }}>
            <span style={{ color: "var(--text-muted)", fontWeight: 500 }}>Mensajero / Traslado:</span>
            <strong style={{ color: "var(--text-primary)" }}>
              {selectedMensajero
                ? `${selectedMensajero.nombre} ${selectedMensajero.apellido}`
                : "Entrega directa en oficina"}
            </strong>
          </div>

          {/* Estado Inicial */}
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", paddingTop: "2px" }}>
            <span style={{ color: "var(--text-muted)", fontWeight: 500 }}>Estado Inicial:</span>
            <span className="badge badge-warning">
              <span className="badge-dot" />
              Borrador
            </span>
          </div>
        </div>

        {/* Checklist de Validación Previa */}
        <div
          style={{
            marginTop: "16px",
            background: "var(--color-slate-50)",
            border: "1px solid var(--border-default)",
            borderRadius: "var(--radius-md)",
            padding: "10px 12px",
            fontSize: "12px",
            display: "flex",
            flexDirection: "column",
            gap: "5px",
          }}
        >
          <span style={{ fontWeight: 700, color: "var(--text-primary)", marginBottom: "2px" }}>
            Requisitos para emisión:
          </span>
          <span style={{ color: tieneEquipo ? "var(--color-success-700)" : "var(--color-danger-600)", fontWeight: 600, display: "inline-flex", alignItems: "center", gap: "5px" }}>
            {tieneEquipo ? <CheckIcon size={13} /> : <span style={{ display: "inline-block", width: "5px", height: "5px", borderRadius: "50%", background: "currentColor" }} />}
            <span>Equipo asignado</span>
          </span>
          <span style={{ color: tieneDestinatario ? "var(--color-success-700)" : "var(--color-danger-600)", fontWeight: 600, display: "inline-flex", alignItems: "center", gap: "5px" }}>
            {tieneDestinatario ? <CheckIcon size={13} /> : <span style={{ display: "inline-block", width: "5px", height: "5px", borderRadius: "50%", background: "currentColor" }} />}
            <span>Destinatario seleccionado</span>
          </span>
          {actaType === "Devolución" && (
            <span style={{ color: tieneOrigen ? "var(--color-success-700)" : "var(--color-danger-600)", fontWeight: 600, display: "inline-flex", alignItems: "center", gap: "5px" }}>
              {tieneOrigen ? <CheckIcon size={13} /> : <span style={{ display: "inline-block", width: "5px", height: "5px", borderRadius: "50%", background: "currentColor" }} />}
              <span>Acta de entrega vinculada</span>
            </span>
          )}
        </div>

        {/* Botones de Acción dentro de la Barra Lateral */}
        <div style={{ display: "flex", flexDirection: "column", gap: "8px", marginTop: "16px" }}>
          <button
            type="submit"
            className="btn btn-primary"
            disabled={saving || !isValid}
            style={{
              width: "100%",
              padding: "11px 16px",
              fontWeight: 700,
              fontSize: "13.5px",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: "8px",
              boxShadow: "0 2px 8px rgba(99, 102, 241, 0.25)",
            }}
          >
            <CheckIcon size={16} />
            <span>{saving ? "Guardando..." : "Guardar Borrador"}</span>
          </button>

          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={onCancel}
            disabled={saving}
            style={{ width: "100%", color: "var(--text-secondary)" }}
          >
            Cancelar trámite
          </button>
        </div>
      </div>
    </aside>
  );
}
