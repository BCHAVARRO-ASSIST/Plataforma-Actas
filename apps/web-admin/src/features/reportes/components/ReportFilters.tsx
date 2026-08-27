import type { ReportFilter, ReportTab } from "../types/reportes";

interface ReportFiltersProps {
  tab: ReportTab;
  filters: ReportFilter;
  onFilterChange: (newFilters: ReportFilter) => void;
  onApply: () => void;
  onClear: () => void;
  loading: boolean;
}

export function ReportFilters({
  tab,
  filters,
  onFilterChange,
  onApply,
  onClear,
  loading,
}: ReportFiltersProps) {
  function handleChange(key: keyof ReportFilter, value: string) {
    onFilterChange({
      ...filters,
      [key]: value ? value : null,
    });
  }

  return (
    <div className="dashboard-block" style={{ padding: "16px 20px", marginBottom: "20px" }}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          onApply();
        }}
      >
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "12px", alignItems: "flex-end" }}>
          {/* BÚSQUEDA TEXTUAL */}
          <div>
            <label style={{ display: "block", fontSize: "11px", fontWeight: 700, color: "#475569", marginBottom: "4px" }}>
              Búsqueda rápida
            </label>
            <input
              type="text"
              placeholder="Número acta, clave, nombre..."
              value={filters.search || ""}
              onChange={(e) => handleChange("search", e.target.value)}
              style={{ width: "100%", padding: "7px 10px", border: "1px solid #cbd5e1", borderRadius: "6px", fontSize: "13px" }}
            />
          </div>

          {/* FECHA INICIAL */}
          {tab !== "equipos" && (
            <div>
              <label style={{ display: "block", fontSize: "11px", fontWeight: 700, color: "#475569", marginBottom: "4px" }}>
                Fecha Inicial
              </label>
              <input
                type="date"
                value={filters.fechaInicio || ""}
                onChange={(e) => handleChange("fechaInicio", e.target.value)}
                style={{ width: "100%", padding: "7px 10px", border: "1px solid #cbd5e1", borderRadius: "6px", fontSize: "13px" }}
              />
            </div>
          )}

          {/* FECHA FINAL */}
          {tab !== "equipos" && (
            <div>
              <label style={{ display: "block", fontSize: "11px", fontWeight: 700, color: "#475569", marginBottom: "4px" }}>
                Fecha Final
              </label>
              <input
                type="date"
                value={filters.fechaFin || ""}
                onChange={(e) => handleChange("fechaFin", e.target.value)}
                style={{ width: "100%", padding: "7px 10px", border: "1px solid #cbd5e1", borderRadius: "6px", fontSize: "13px" }}
              />
            </div>
          )}

          {/* ESTADO */}
          <div>
            <label style={{ display: "block", fontSize: "11px", fontWeight: 700, color: "#475569", marginBottom: "4px" }}>
              Estado
            </label>
            <select
              value={filters.estado || ""}
              onChange={(e) => handleChange("estado", e.target.value)}
              style={{ width: "100%", padding: "7px 10px", border: "1px solid #cbd5e1", borderRadius: "6px", fontSize: "13px", background: "#ffffff" }}
            >
              <option value="">Todos los estados</option>
              {tab === "equipos" ? (
                <>
                  <option value="Activo">Activo</option>
                  <option value="Inactivo">Inactivo</option>
                  <option value="Asignado">Asignado</option>
                </>
              ) : (
                <>
                  <option value="Borrador">Borrador</option>
                  <option value="Firmada">Firmada</option>
                  <option value="Finalizada">Finalizada</option>
                  <option value="Anulada">Anulada</option>
                </>
              )}
            </select>
          </div>

          {/* TIPO DE ACTA */}
          {tab === "actas" && (
            <div>
              <label style={{ display: "block", fontSize: "11px", fontWeight: 700, color: "#475569", marginBottom: "4px" }}>
                Tipo de Acta
              </label>
              <select
                value={filters.tipoActa || ""}
                onChange={(e) => handleChange("tipoActa", e.target.value)}
                style={{ width: "100%", padding: "7px 10px", border: "1px solid #cbd5e1", borderRadius: "6px", fontSize: "13px", background: "#ffffff" }}
              >
                <option value="">Todos los tipos</option>
                <option value="Entrega">Entrega</option>
                <option value="Devolución">Devolución</option>
              </select>
            </div>
          )}

          {/* BOTONES DE ACCIÓN */}
          <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
            <button
              type="button"
              onClick={onClear}
              disabled={loading}
              style={{
                padding: "8px 14px",
                borderRadius: "6px",
                border: "1px solid #cbd5e1",
                background: "#ffffff",
                color: "#475569",
                fontSize: "13px",
                fontWeight: 600,
                cursor: "pointer",
              }}
            >
              Limpiar
            </button>

            <button
              type="submit"
              disabled={loading}
              className="actas-primary"
              style={{ minHeight: "36px", padding: "8px 16px", fontSize: "13px" }}
            >
              {loading ? "Filtrando..." : "Aplicar Filtros"}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
