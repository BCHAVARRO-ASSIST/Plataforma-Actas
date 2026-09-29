import { SearchIcon } from "@/components/ui/Icons";

export interface EquipmentFiltersProps {
  search: string;
  status: string;
  brand: string;
  brands: string[];
  onSearchChange: (value: string) => void;
  onStatusChange: (value: string) => void;
  onBrandChange: (value: string) => void;
  onClear: () => void;
}

export type EquipoFiltersProps = EquipmentFiltersProps;

export function EquipmentFilters({
  search,
  status,
  brand,
  brands,
  onSearchChange,
  onStatusChange,
  onBrandChange,
  onClear,
}: EquipmentFiltersProps) {
  return (
    <div className="toolbar-container" style={{ background: 'var(--bg-surface)', padding: '16px 20px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--border-default)' }}>
      <div style={{ display: 'flex', gap: '12px', alignItems: 'center', flex: 1, flexWrap: 'wrap' }}>
        <div className="search-input-wrapper">
          <span className="search-icon">
            <SearchIcon size={16} />
          </span>
          <input
            id="equipment-search"
            name="search"
            className="form-input"
            type="text"
            placeholder="Buscar por placa, modelo, responsable..."
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>

        <div style={{ width: '180px' }}>
          <select
            id="equipment-status"
            name="status"
            className="form-select"
            value={status}
            onChange={(e) => onStatusChange(e.target.value)}
          >
            <option value="">Todos los estados</option>
            <option value="Activo">Activo</option>
            <option value="Inactivo">Inactivo</option>
            <option value="Mantenimiento">Mantenimiento</option>
          </select>
        </div>

        <div style={{ width: '180px' }}>
          <select
            id="equipment-brand"
            name="brand"
            className="form-select"
            value={brand}
            onChange={(e) => onBrandChange(e.target.value)}
          >
            <option value="">Todas las marcas</option>
            {brands.map((b) => (
              <option key={b} value={b}>
                {b}
              </option>
            ))}
          </select>
        </div>

        {(search || status || brand) && (
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={onClear}
          >
            Limpiar filtros
          </button>
        )}
      </div>
    </div>
  );
}

export const EquipoFilters = EquipmentFilters;