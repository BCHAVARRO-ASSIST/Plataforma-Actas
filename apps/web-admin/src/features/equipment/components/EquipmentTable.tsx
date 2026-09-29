import type { Equipment } from "../equipmentService";
import { UserIcon, TrashIcon, EditIcon } from "@/components/ui/Icons";
import { StatusBadge } from "@/components/ui/StatusBadge";

export interface EquipmentTableProps {
  equipment: Equipment[];
  onEdit?: (item: Equipment) => void;
  onToggleStatus?: (item: Equipment) => void;
  onDelete?: (item: Equipment) => void;
  canEdit?: boolean;
  canDeactivate?: boolean;
  canDelete?: boolean;
}

export type EquipoTableProps = EquipmentTableProps;

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
            <th>Equipo / Placa</th>
            <th>Asignado a</th>
            <th>Marca</th>
            <th>Especificaciones Técnicas</th>
            <th>Departamento</th>
            <th>Estado</th>
            {showActions && <th style={{ textAlign: "center" }}>Acciones</th>}
          </tr>
        </thead>

        <tbody>
          {equipment.length === 0 ? (
            <tr>
              <td colSpan={showActions ? 7 : 6} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
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

              const tipoDescripcion = item.nombreEquipo && item.nombreEquipo !== item.inventoryKey
                ? item.nombreEquipo
                : "Equipo Corporativo";

              return (
                <tr key={item.id}>
                  <td>
                    <div style={{ display: "flex", flexDirection: "column" }}>
                      <strong style={{ color: "var(--text-primary)", fontFamily: "monospace" }}>
                        {item.inventoryKey}
                      </strong>
                      <span style={{ fontSize: "12px", color: "var(--text-muted)" }}>
                        {tipoDescripcion}
                      </span>
                    </div>
                  </td>

                  <td>
                    <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                      <UserIcon size={14} style={{ color: "var(--text-muted)" }} />
                      <span>{item.asignadoA || "Sin asignar"}</span>
                    </div>
                  </td>

                  <td>{item.marca || "—"}</td>

                  <td style={{ color: "var(--text-secondary)", fontSize: "13px" }}>
                    {specs}
                  </td>

                  <td>{item.departamento || "—"}</td>

                  <td>
                    <StatusBadge status={item.estatusEquipo || "Inactivo"} />
                  </td>

                  {showActions && (
                    <td style={{ textAlign: "center" }}>
                      <div style={{ display: "inline-flex", gap: "6px", alignItems: "center" }}>
                        {canEdit && onEdit && (
                          <button
                            type="button"
                            onClick={() => onEdit(item)}
                            className="btn btn-secondary btn-sm"
                            style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}
                            title="Editar equipo"
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

export const EquipoTable = EquipmentTable;