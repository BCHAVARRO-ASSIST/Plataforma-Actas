import type { Equipment } from "../features/equipment/equipmentService";
import { UserIcon, TrashIcon, EditIcon } from "./Icons";

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
    <div className="table-wrapper">
      <table className="data-table">
        <thead>
          <tr>
            <th>Placa / Inventario</th>
            <th>Equipo</th>
            <th>Empresa</th>
            <th>Asignado a</th>
            <th>Marca</th>
            <th>Specs (CPU/RAM/Disco)</th>
            <th>Departamento</th>
            <th>Estado</th>
            {showActions && <th style={{ textAlign: "center" }}>Acciones</th>}
          </tr>
        </thead>

        <tbody>
          {equipment.length === 0 ? (
            <tr>
              <td colSpan={showActions ? 9 : 8} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
                No se encontraron equipos que coincidan con los filtros.
              </td>
            </tr>
          ) : (
            equipment.map((item) => {
              const isActivo = item.estatusEquipo?.toLowerCase() === "activo";
              const isAsignado = item.estatusEquipo?.toLowerCase() === "asignado";
              const isInactive = !isActivo && !isAsignado;

              const specs = [item.procesador, item.memoriaRam, item.discoDuro]
                .filter(Boolean)
                .join(" · ") || "—";

              return (
                <tr key={item.id}>
                  <td className="primary-cell">
                    <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--color-primary-700)', fontWeight: 700 }}>
                      {item.inventoryKey}
                    </span>
                  </td>

                  <td>
                    <strong style={{ color: "var(--text-primary)" }}>{item.nombreEquipo}</strong>
                  </td>

                  <td>{item.empresa ?? "—"}</td>
                  <td>
                    {item.asignadoA ? (
                      <span style={{ display: 'inline-flex', alignItems: 'center', gap: '5px' }}>
                        <UserIcon size={14} style={{ color: 'var(--text-muted)' }} />
                        {item.asignadoA}
                      </span>
                    ) : "—"}
                  </td>
                  <td>{item.marca ?? "—"}</td>
                  <td style={{ fontSize: "12px", color: "var(--text-muted)" }}>{specs}</td>
                  <td>{item.departamento ?? "—"}</td>
                  <td>
                    <span
                      className={
                        isActivo
                          ? "badge badge-success"
                          : isAsignado
                          ? "badge badge-info"
                          : "badge badge-neutral"
                      }
                    >
                      <span className="badge-dot" />
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
                            className="btn btn-secondary btn-sm"
                            style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}
                            title="Editar información del equipo"
                          >
                            <EditIcon size={13} />
                            <span>Editar</span>
                          </button>
                        )}

                        {canDeactivate && onToggleStatus && (
                          <button
                            type="button"
                            onClick={() => onToggleStatus(item)}
                            className={isActivo || isAsignado ? "btn btn-ghost btn-sm" : "btn btn-secondary btn-sm"}
                            style={{ color: isActivo || isAsignado ? "var(--color-warning-700)" : "var(--color-success-700)" }}
                            title={isActivo || isAsignado ? "Desactivar equipo" : "Activar equipo"}
                          >
                            {isActivo || isAsignado ? "Desactivar" : "Activar"}
                          </button>
                        )}

                        {isInactive && canDelete && onDelete && (
                          <button
                            type="button"
                            onClick={() => onDelete(item)}
                            className="btn btn-danger btn-sm"
                            style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}
                            title="Eliminar permanentemente equipo inactivo"
                          >
                            <TrashIcon size={13} />
                            <span>Eliminar</span>
                          </button>
                        )}
                      </div>
                    </td>
                  )}
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}