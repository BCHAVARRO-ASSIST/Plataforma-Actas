import type { Equipment } from "../types/equipment";

interface EquipmentTableProps {
  equipment: Equipment[];
  onEdit?: (item: Equipment) => void;
  onToggleStatus?: (item: Equipment) => void;
  onDelete?: (item: Equipment) => void;
  canEdit?: boolean;
  canDeactivate?: boolean;
  canDelete?: boolean;
}

export function EquipmentTable({
  equipment,
  onEdit,
  onToggleStatus,
  onDelete,
  canEdit = false,
  canDeactivate = false,
  canDelete = false,
}: EquipmentTableProps) {
  const showActions = canEdit || canDeactivate || canDelete;

  return (
    <div className="table-container">
      <table>
        <thead>
          <tr>
            <th>Inventario</th>
            <th>Equipo</th>
            <th>Empresa</th>
            <th>Asignado a</th>
            <th>Marca</th>
            <th>Procesador</th>
            <th>RAM</th>
            <th>Disco</th>
            <th>Departamento</th>
            <th>Estado</th>
            {showActions && <th style={{ textAlign: "center" }}>Acciones</th>}
          </tr>
        </thead>

        <tbody>
          {equipment.map((item) => {
            const isActivo = item.estatusEquipo?.toLowerCase() === "activo";
            const isAsignado = item.estatusEquipo?.toLowerCase() === "asignado";
            const isInactive = !isActivo && !isAsignado;

            return (
              <tr key={item.id}>
                <td>
                  <strong className="equipment-key">{item.inventoryKey}</strong>
                </td>

                <td>{item.nombreEquipo}</td>
                <td>{item.empresa ?? "—"}</td>
                <td>{item.asignadoA ?? "—"}</td>
                <td>{item.marca ?? "—"}</td>
                <td>{item.procesador ?? "—"}</td>
                <td>{item.memoriaRam ?? "—"}</td>
                <td>{item.discoDuro ?? "—"}</td>
                <td>{item.departamento ?? "—"}</td>
                <td>
                  <span
                    className={
                      isActivo
                        ? "status-badge status-active"
                        : isAsignado
                        ? "status-badge status-warning"
                        : "status-badge status-inactive"
                    }
                  >
                    {item.estatusEquipo ?? "Sin estado"}
                  </span>
                </td>

                {showActions && (
                  <td>
                    <div style={{ display: "flex", gap: "6px", justifyContent: "center", alignItems: "center" }}>
                      {canEdit && onEdit && (
                        <button
                          type="button"
                          onClick={() => onEdit(item)}
                          className="usuario-edit-button"
                          style={{ padding: "4px 10px", fontSize: "12px" }}
                          title="Editar información del equipo"
                        >
                          Editar
                        </button>
                      )}

                      {canDeactivate && onToggleStatus && (
                        <button
                          type="button"
                          onClick={() => onToggleStatus(item)}
                          className={isActivo || isAsignado ? "usuario-delete-button" : "usuario-activate-button"}
                          style={{ padding: "4px 10px", fontSize: "12px" }}
                          title={isActivo || isAsignado ? "Desactivar equipo" : "Activar equipo"}
                        >
                          {isActivo || isAsignado ? "Desactivar" : "Activar"}
                        </button>
                      )}

                      {isInactive && canDelete && onDelete && (
                        <button
                          type="button"
                          onClick={() => onDelete(item)}
                          style={{
                            padding: "4px 10px",
                            fontSize: "12px",
                            fontWeight: 600,
                            borderRadius: "6px",
                            background: "#fee2e2",
                            color: "#b91c1c",
                            border: "1px solid #fecaca",
                            cursor: "pointer",
                            transition: "all 0.15s ease",
                          }}
                          onMouseEnter={(e) => {
                            e.currentTarget.style.background = "#ef4444";
                            e.currentTarget.style.color = "#ffffff";
                          }}
                          onMouseLeave={(e) => {
                            e.currentTarget.style.background = "#fee2e2";
                            e.currentTarget.style.color = "#b91c1c";
                          }}
                          title="Eliminar permanentemente equipo inactivo"
                        >
                          🗑 Eliminar
                        </button>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}