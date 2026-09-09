import type { Acta } from "../features/actas/actaService";
import { ActaIcon, MessengerIcon, PackageIcon } from "./Icons";

interface ActaTypeSelectorProps {
  actaType: "Entrega" | "Devolución";
  onTypeChange: (type: "Entrega" | "Devolución") => void;
  actaOrigenId: string;
  onActaOrigenChange: (id: string) => void;
  actasEntrega: Acta[];
  disabled?: boolean;
}

export function ActaTypeSelector({
  actaType,
  onTypeChange,
  actaOrigenId,
  onActaOrigenChange,
  actasEntrega,
  disabled = false,
}: ActaTypeSelectorProps) {
  return (
    <div className="card" style={{ padding: "22px" }}>
      <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "18px" }}>
        <div
          style={{
            width: "28px",
            height: "28px",
            borderRadius: "50%",
            background: "var(--color-primary-100)",
            color: "var(--color-primary-700)",
            fontWeight: 800,
            fontSize: "12px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          1
        </div>
        <div>
          <h2 style={{ margin: 0, fontSize: "16px", fontWeight: 700, color: "var(--text-primary)" }}>
            Tipo de Trámite Documental
          </h2>
          <p style={{ margin: 0, fontSize: "12px", color: "var(--text-muted)" }}>
            Selecciona la operación que vas a registrar en el sistema.
          </p>
        </div>
      </div>

      {/* Selector Visual Segmentado */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gap: "12px",
          marginBottom: "16px",
        }}
      >
        <div
          onClick={() => !disabled && onTypeChange("Entrega")}
          style={{
            border: actaType === "Entrega" ? "2px solid var(--color-primary-600)" : "1px solid var(--border-default)",
            borderRadius: "var(--radius-md)",
            padding: "14px 16px",
            background: actaType === "Entrega" ? "var(--color-primary-50)" : "var(--bg-surface)",
            cursor: disabled ? "not-allowed" : "pointer",
            transition: "all var(--transition-fast)",
            display: "flex",
            alignItems: "center",
            gap: "12px",
          }}
        >
          <div
            style={{
              width: "36px",
              height: "36px",
              borderRadius: "var(--radius-md)",
              background: actaType === "Entrega" ? "var(--color-primary-600)" : "var(--color-slate-100)",
              color: actaType === "Entrega" ? "#ffffff" : "var(--text-muted)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <PackageIcon size={18} />
          </div>
          <div>
            <strong style={{ fontSize: "14px", color: actaType === "Entrega" ? "var(--color-primary-900)" : "var(--text-primary)" }}>
              Acta de Entrega
            </strong>
            <span style={{ fontSize: "12px", color: "var(--text-muted)", display: "block" }}>
              Asignación inicial o recambio de equipo.
            </span>
          </div>
        </div>

        <div
          onClick={() => !disabled && onTypeChange("Devolución")}
          style={{
            border: actaType === "Devolución" ? "2px solid var(--color-primary-600)" : "1px solid var(--border-default)",
            borderRadius: "var(--radius-md)",
            padding: "14px 16px",
            background: actaType === "Devolución" ? "var(--color-primary-50)" : "var(--bg-surface)",
            cursor: disabled ? "not-allowed" : "pointer",
            transition: "all var(--transition-fast)",
            display: "flex",
            alignItems: "center",
            gap: "12px",
          }}
        >
          <div
            style={{
              width: "36px",
              height: "36px",
              borderRadius: "var(--radius-md)",
              background: actaType === "Devolución" ? "var(--color-primary-600)" : "var(--color-slate-100)",
              color: actaType === "Devolución" ? "#ffffff" : "var(--text-muted)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <MessengerIcon size={18} />
          </div>
          <div>
            <strong style={{ fontSize: "14px", color: actaType === "Devolución" ? "var(--color-primary-900)" : "var(--text-primary)" }}>
              Acta de Devolución
            </strong>
            <span style={{ fontSize: "12px", color: "var(--text-muted)", display: "block" }}>
              Reingreso de equipo vinculado a entrega previa.
            </span>
          </div>
        </div>
      </div>

      {/* Si es Devolución, selector de acta previa */}
      {actaType === "Devolución" && (
        <div
          style={{
            background: "var(--color-slate-50)",
            border: "1px solid var(--border-default)",
            borderRadius: "var(--radius-md)",
            padding: "14px 16px",
            marginTop: "12px",
          }}
        >
          <label
            className="form-label"
            htmlFor="acta-origen"
            style={{ display: "flex", alignItems: "center", gap: "6px", fontWeight: 700 }}
          >
            <ActaIcon size={14} style={{ color: "var(--color-primary-600)" }} />
            <span>Acta de Entrega Original (Requerida) *</span>
          </label>
          <select
            id="acta-origen"
            name="actaOrigenId"
            className="form-select"
            value={actaOrigenId}
            onChange={(e) => onActaOrigenChange(e.target.value)}
            disabled={disabled}
            required
            style={{ background: "#ffffff" }}
          >
            <option value="">
              {actasEntrega.length === 0
                ? "No hay actas de entrega firmadas disponibles"
                : "Seleccione el acta de entrega original..."}
            </option>
            {actasEntrega.map((a: Acta) => (
              <option key={a.id} value={a.id}>
                {a.numeroActa} — {a.inventoryKey ?? a.equipmentId} ({a.nombreEquipo ?? "Equipo"})
              </option>
            ))}
          </select>
          <span style={{ fontSize: "11.5px", color: "var(--text-muted)", marginTop: "4px", display: "block" }}>
            El equipo y el responsable se vincularán automáticamente del acta original.
          </span>
        </div>
      )}
    </div>
  );
}
