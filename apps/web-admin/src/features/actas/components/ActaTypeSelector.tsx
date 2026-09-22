import type { Acta } from "@/features/actas/actaService";
import { ActaIcon, MessengerIcon, PackageIcon } from "@/components/ui/Icons";

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
        <div className="form-step-badge">
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
      <div className="tramite-selector-grid">
        <div
          onClick={() => !disabled && onTypeChange("Entrega")}
          className={`tramite-card ${actaType === "Entrega" ? "active" : ""} ${disabled ? "disabled" : ""}`}
        >
          <div className="tramite-icon-box">
            <PackageIcon size={18} />
          </div>
          <div className="tramite-info">
            <strong className="tramite-title">
              Acta de Entrega
            </strong>
            <span className="tramite-desc">
              Asignación inicial o recambio de equipo.
            </span>
          </div>
        </div>

        <div
          onClick={() => !disabled && onTypeChange("Devolución")}
          className={`tramite-card ${actaType === "Devolución" ? "active" : ""} ${disabled ? "disabled" : ""}`}
        >
          <div className="tramite-icon-box">
            <MessengerIcon size={18} />
          </div>
          <div className="tramite-info">
            <strong className="tramite-title">
              Acta de Devolución
            </strong>
            <span className="tramite-desc">
              Reingreso de equipo vinculado a entrega previa.
            </span>
          </div>
        </div>
      </div>

      {/* Si es Devolución, selector de acta previa */}
      {actaType === "Devolución" && (
        <div className="acta-devolucion-origen-card">
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
          >
            <option value="">
              {actasEntrega.length === 0
                ? "No hay actas de entrega firmadas disponibles"
                : "Seleccione el acta de entrega original..."}
            </option>
            {actasEntrega.map((a: Acta) => (
              <option key={a.id} value={a.id}>
                {a.numeroActa} — {a.inventoryKey ?? a.equipmentId} ({a.nombreEquipo ?? "Equipo"}) · {a.estado}
              </option>
            ))}
          </select>
          <span className="acta-devolucion-helper">
            El equipo y el responsable se vincularán automáticamente del acta original.
          </span>
        </div>
      )}
    </div>
  );
}
