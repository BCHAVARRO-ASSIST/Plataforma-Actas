import type { ReportFilter, ReportTab } from "../features/reportes/reportesService";
import { SearchIcon, FilterIcon, CalendarIcon, RefreshIcon, CloseIcon } from "./Icons";

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

  function setDatePreset(days: number | "month" | "year") {
    const today = new Date();
    const endStr = today.toISOString().split("T")[0];
    let startStr = "";

    if (days === "month") {
      const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
      startStr = firstDay.toISOString().split("T")[0];
    } else if (days === "year") {
      const firstDay = new Date(today.getFullYear(), 0, 1);
      startStr = firstDay.toISOString().split("T")[0];
    } else if (days === 0) {
      startStr = endStr;
    } else {
      const past = new Date();
      past.setDate(today.getDate() - days);
      startStr = past.toISOString().split("T")[0];
    }

    onFilterChange({
      ...filters,
      fechaInicio: startStr,
      fechaFin: endStr,
    });
  }

  const activeFiltersCount = Object.values(filters).filter(Boolean).length;
  const hasActiveFilters = activeFiltersCount > 0;

  return (
    <div
      style={{
        background: "var(--bg-surface)",
        border: "1px solid var(--border-default)",
        borderRadius: "var(--radius-lg)",
        padding: "16px 20px",
        marginBottom: "22px",
        boxShadow: "var(--shadow-sm)",
      }}
    >
      <form
        onSubmit={(e) => {
          e.preventDefault();
          onApply();
        }}
      >
        {/* Cabecera de Filtros con Accesos Rápidos */}
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: "14px",
            flexWrap: "wrap",
            gap: "10px",
            paddingBottom: "12px",
            borderBottom: "1px solid var(--border-subtle)",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <div
              style={{
                width: "28px",
                height: "28px",
                borderRadius: "var(--radius-sm)",
                background: "var(--color-primary-50)",
                color: "var(--color-primary-600)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <FilterIcon size={14} />
            </div>
            <span style={{ fontSize: "13px", fontWeight: 700, color: "var(--text-primary)" }}>
              Filtros de Búsqueda
            </span>
            {hasActiveFilters && (
              <span
                style={{
                  fontSize: "11px",
                  fontWeight: 700,
                  padding: "2px 8px",
                  borderRadius: "var(--radius-pill)",
                  background: "var(--color-primary-100)",
                  color: "var(--color-primary-800)",
                }}
              >
                {activeFiltersCount} {activeFiltersCount === 1 ? "activo" : "activos"}
              </span>
            )}
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: "8px", flexWrap: "wrap" }}>
            {/* Presets de fecha solo para reportes con fechas */}
            {tab !== "equipos" && (
              <div style={{ display: "flex", alignItems: "center", gap: "4px" }}>
                <span style={{ fontSize: "11.5px", color: "var(--text-muted)", marginRight: "2px" }}>
                  Rápido:
                </span>
                <button
                  type="button"
                  onClick={() => setDatePreset(0)}
                  className="btn btn-ghost btn-sm"
                  style={{ fontSize: "11px", padding: "3px 8px", height: "auto" }}
                >
                  Hoy
                </button>
                <button
                  type="button"
                  onClick={() => setDatePreset(7)}
                  className="btn btn-ghost btn-sm"
                  style={{ fontSize: "11px", padding: "3px 8px", height: "auto" }}
                >
                  7 días
                </button>
                <button
                  type="button"
                  onClick={() => setDatePreset(30)}
                  className="btn btn-ghost btn-sm"
                  style={{ fontSize: "11px", padding: "3px 8px", height: "auto" }}
                >
                  30 días
                </button>
                <button
                  type="button"
                  onClick={() => setDatePreset("month")}
                  className="btn btn-ghost btn-sm"
                  style={{ fontSize: "11px", padding: "3px 8px", height: "auto" }}
                >
                  Este mes
                </button>
              </div>
            )}

            {hasActiveFilters && (
              <button
                type="button"
                onClick={onClear}
                disabled={loading}
                className="btn btn-ghost btn-sm"
                style={{
                  color: "var(--color-danger-600)",
                  fontSize: "12px",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: "4px",
                  padding: "4px 10px",
                }}
                title="Restablecer todos los filtros"
              >
                <CloseIcon size={12} />
                <span>Limpiar filtros</span>
              </button>
            )}
          </div>
        </div>

        {/* Campos de Filtros Adaptativos */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns:
              tab === "equipos"
                ? "minmax(240px, 2fr) minmax(180px, 1fr) auto"
                : tab === "actas"
                ? "minmax(220px, 1.8fr) minmax(140px, 1fr) minmax(140px, 1fr) minmax(135px, 0.9fr) minmax(135px, 0.9fr) auto"
                : "minmax(220px, 2fr) minmax(150px, 1fr) minmax(135px, 0.9fr) minmax(135px, 0.9fr) auto",
            gap: "12px",
            alignItems: "flex-end",
          }}
        >
          {/* 1. Búsqueda rápida */}
          <div>
            <label
              className="form-label"
              htmlFor="filterSearch"
              style={{ fontSize: "12px", display: "flex", alignItems: "center", gap: "5px", marginBottom: "5px" }}
            >
              <SearchIcon size={13} style={{ color: "var(--text-muted)" }} />
              <span>Búsqueda rápida</span>
            </label>
            <input
              id="filterSearch"
              name="search"
              type="text"
              className="form-input"
              placeholder={
                tab === "equipos"
                  ? "Placa, modelo, asignado a..."
                  : "N° acta, equipo, destinatario, mensajero..."
              }
              value={filters.search || ""}
              onChange={(e) => handleChange("search", e.target.value)}
              style={{ height: "38px", fontSize: "13px" }}
            />
          </div>

          {/* 2. Tipo de Acta (Solo en pestaña Todas las Actas) */}
          {tab === "actas" && (
            <div>
              <label
                className="form-label"
                htmlFor="filterTipoActa"
                style={{ fontSize: "12px", marginBottom: "5px" }}
              >
                Tipo de Acta
              </label>
              <select
                id="filterTipoActa"
                name="tipoActa"
                className="form-select"
                value={filters.tipoActa || ""}
                onChange={(e) => handleChange("tipoActa", e.target.value)}
                style={{ height: "38px", fontSize: "13px" }}
              >
                <option value="">Todos los tipos</option>
                <option value="Entrega">Entrega</option>
                <option value="Devolución">Devolución</option>
              </select>
            </div>
          )}

          {/* 3. Estado */}
          <div>
            <label
              className="form-label"
              htmlFor="filterEstado"
              style={{ fontSize: "12px", display: "flex", alignItems: "center", gap: "5px", marginBottom: "5px" }}
            >
              <FilterIcon size={13} style={{ color: "var(--text-muted)" }} />
              <span>Estado</span>
            </label>
            <select
              id="filterEstado"
              name="estado"
              className="form-select"
              value={filters.estado || ""}
              onChange={(e) => handleChange("estado", e.target.value)}
              style={{ height: "38px", fontSize: "13px" }}
            >
              <option value="">Todos los estados</option>
              {tab === "equipos" ? (
                <>
                  <option value="Activo">Activo (Disponible)</option>
                  <option value="Asignado">Asignado (En uso)</option>
                  <option value="Mantenimiento">En Mantenimiento</option>
                  <option value="Inactivo">Inactivo / Baja</option>
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

          {/* 4. Fecha Inicial */}
          {tab !== "equipos" && (
            <div>
              <label
                className="form-label"
                htmlFor="filterFechaInicio"
                style={{ fontSize: "12px", display: "flex", alignItems: "center", gap: "5px", marginBottom: "5px" }}
              >
                <CalendarIcon size={13} style={{ color: "var(--text-muted)" }} />
                <span>Fecha Inicial</span>
              </label>
              <input
                id="filterFechaInicio"
                name="fechaInicio"
                type="date"
                className="form-input"
                value={filters.fechaInicio || ""}
                onChange={(e) => handleChange("fechaInicio", e.target.value)}
                style={{ height: "38px", fontSize: "12.5px" }}
              />
            </div>
          )}

          {/* 5. Fecha Final */}
          {tab !== "equipos" && (
            <div>
              <label
                className="form-label"
                htmlFor="filterFechaFin"
                style={{ fontSize: "12px", display: "flex", alignItems: "center", gap: "5px", marginBottom: "5px" }}
              >
                <CalendarIcon size={13} style={{ color: "var(--text-muted)" }} />
                <span>Fecha Final</span>
              </label>
              <input
                id="filterFechaFin"
                name="fechaFin"
                type="date"
                className="form-input"
                value={filters.fechaFin || ""}
                onChange={(e) => handleChange("fechaFin", e.target.value)}
                style={{ height: "38px", fontSize: "12.5px" }}
              />
            </div>
          )}

          {/* Botón de Aplicar / Filtrar */}
          <div style={{ display: "flex", gap: "8px" }}>
            <button
              type="submit"
              disabled={loading}
              className="btn btn-primary"
              style={{
                height: "38px",
                display: "inline-flex",
                alignItems: "center",
                gap: "6px",
                padding: "0 18px",
                fontWeight: 600,
                whiteSpace: "nowrap",
              }}
            >
              <RefreshIcon size={14} className={loading ? "spin-animation" : ""} />
              <span>{loading ? "Filtrando..." : "Filtrar"}</span>
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
