interface EquipmentFiltersProps {
  search: string;
  status: string;
  brand: string;
  brands: string[];
  onSearchChange: (value: string) => void;
  onStatusChange: (value: string) => void;
  onBrandChange: (value: string) => void;
  onClear: () => void;
}

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
    <div className="inventory-filters">

      <div className="inventory-search">

        <span className="inventory-search-icon">
          ⌕
        </span>

        <input
          id="equipment-search"
          type="text"
          placeholder="Buscar por inventario, equipo, usuario..."
          value={search}
          onChange={(event) =>
            onSearchChange(event.target.value)
          }
        />

        {search && (
          <button
            type="button"
            className="inventory-search-clear"
            onClick={() => onSearchChange("")}
            aria-label="Limpiar búsqueda"
          >
            ×
          </button>
        )}

      </div>

      <div className="inventory-filter">

        <label htmlFor="equipment-status">
          Estado
        </label>

        <select
          id="equipment-status"
          value={status}
          onChange={(event) =>
            onStatusChange(event.target.value)
          }
        >
          <option value="">Todos los estados</option>
          <option value="Activo">Activo</option>
          <option value="Inactivo">Inactivo</option>
        </select>

      </div>

      <div className="inventory-filter">

        <label htmlFor="equipment-brand">
          Marca
        </label>

        <select
          id="equipment-brand"
          value={brand}
          onChange={(event) =>
            onBrandChange(event.target.value)
          }
        >
          <option value="">Todas las marcas</option>

          {brands.map((item) => (
            <option
              key={item}
              value={item}
            >
              {item}
            </option>
          ))}
        </select>

      </div>

      <button
        type="button"
        className="inventory-clear"
        onClick={onClear}
      >
        Restablecer
      </button>

    </div>
  );
}