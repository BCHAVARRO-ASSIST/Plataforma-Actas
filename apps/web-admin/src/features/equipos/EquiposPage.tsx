import { useEffect, useMemo, useState } from "react";
import { EquipoFilters } from "./components/EquipoFilters";
import { EquipoTable } from "./components/EquipoTable";
import { EquipoFormModal } from "./components/EquipoFormModal";
import {
  getEquipos,
  createEquipo,
  updateEquipo,
  deactivateEquipo,
  activateEquipo,
  deleteEquipo,
  syncGoogleDriveInventory,
} from "./equipoService";
import type { Equipo, CreateEquipoRequest, UpdateEquipoRequest } from "./equipoService";
import { useAuth } from "@/features/auth/useAuth";
import { PERMISSIONS, hasPermission } from "@/features/auth/permissions";
import {
  EquipmentIcon,
  CheckIcon,
  MaintenanceIcon,
  PackageIcon,
  RefreshIcon,
  PlusIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";
import { Pagination } from "@/components/ui/Pagination";

const ITEMS_PER_PAGE = 12;

export function EquiposPage() {
  const { user } = useAuth();

  const [equipment, setEquipment] = useState<Equipo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [brand, setBrand] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEquipment, setEditingEquipment] = useState<Equipo | null>(null);
  const [saving, setSaving] = useState(false);
  const [syncingDrive, setSyncingDrive] = useState(false);

  async function handleSyncDrive() {
    try {
      setSyncingDrive(true);
      setError(null);
      const res = await syncGoogleDriveInventory();
      setSuccessMessage(res.message);
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al sincronizar con Google Drive.");
    } finally {
      setSyncingDrive(false);
    }
  }

  const canCreate = hasPermission(user, PERMISSIONS.EQUIPOS_REGISTRAR);
  const canEdit = hasPermission(user, PERMISSIONS.EQUIPOS_EDITAR);
  const canDeactivate = hasPermission(user, PERMISSIONS.EQUIPOS_DESACTIVAR);

  async function loadEquipment(silent = false) {
    try {
      if (!silent) {
        setLoading(true);
        setError(null);
      }
      const data = await getEquipos();
      setEquipment(data);
    } catch (err) {
      if (!silent) {
        setError(err instanceof Error ? err.message : "No fue posible cargar el inventario.");
      }
    } finally {
      if (!silent) {
        setLoading(false);
      }
    }
  }

  useEffect(() => {
    let mounted = true;
    loadEquipment(false);

    // Auto-refresco en segundo plano cada 60 segundos para reflejar sincronizaciones automáticas
    const interval = setInterval(() => {
      if (mounted) {
        loadEquipment(true);
      }
    }, 60000);

    return () => {
      mounted = false;
      clearInterval(interval);
    };
  }, []);

  const brands = useMemo(() => {
    return Array.from(
      new Set(
        equipment
          .map((item) => item.marca)
          .filter((value): value is string => Boolean(value))
      )
    ).sort();
  }, [equipment]);

  const filteredEquipment = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return equipment.filter((item) => {
      const matchesSearch =
        normalizedSearch === "" ||
        item.inventoryKey.toLowerCase().includes(normalizedSearch) ||
        item.nombreEquipo.toLowerCase().includes(normalizedSearch) ||
        (item.asignadoA ?? "").toLowerCase().includes(normalizedSearch) ||
        (item.marca ?? "").toLowerCase().includes(normalizedSearch) ||
        (item.departamento ?? "").toLowerCase().includes(normalizedSearch);

      const matchesStatus = status === "" || item.estatusEquipo === status;
      const matchesBrand = brand === "" || item.marca === brand;

      return matchesSearch && matchesStatus && matchesBrand;
    });
  }, [equipment, search, status, brand]);

  const totalPages = Math.max(1, Math.ceil(filteredEquipment.length / ITEMS_PER_PAGE));
  const paginatedEquipment = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    return filteredEquipment.slice(start, start + ITEMS_PER_PAGE);
  }, [filteredEquipment, currentPage]);

  const { activeCount, maintenanceCount, inactiveCount } = useMemo(() => {
    return equipment.reduce(
      (acc, i) => {
        if (i.estatusEquipo === "Activo" || i.estatusEquipo === "Asignado") acc.activeCount++;
        if (i.estatusEquipo === "Mantenimiento") acc.maintenanceCount++;
        if (i.estatusEquipo === "Inactivo") acc.inactiveCount++;
        return acc;
      },
      { activeCount: 0, maintenanceCount: 0, inactiveCount: 0 }
    );
  }, [equipment]);

  function clearFilters() {
    setSearch("");
    setStatus("");
    setBrand("");
    setCurrentPage(1);
  }

  function handleOpenCreate() {
    setEditingEquipment(null);
    setIsModalOpen(true);
  }

  function handleOpenEdit(item: Equipo) {
    setEditingEquipment(item);
    setIsModalOpen(true);
  }

  async function handleSaveEquipment(data: CreateEquipoRequest | UpdateEquipoRequest) {
    try {
      setSaving(true);
      if (editingEquipment) {
        await updateEquipo(editingEquipment.id, data as UpdateEquipoRequest);
        setSuccessMessage(`Equipo ${data.inventoryKey} actualizado correctamente.`);
      } else {
        await createEquipo(data as CreateEquipoRequest);
        setSuccessMessage(`Equipo ${data.inventoryKey} registrado correctamente.`);
      }
      setIsModalOpen(false);
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al guardar equipo.");
    } finally {
      setSaving(false);
    }
  }

  async function handleToggleStatus(item: Equipo) {
    const isActivo = item.estatusEquipo?.toLowerCase() === "activo" || item.estatusEquipo?.toLowerCase() === "asignado";
    const confirmMessage = isActivo
      ? `¿Estás seguro de que deseas desactivar el equipo ${item.inventoryKey}?`
      : `¿Estás seguro de que deseas activar el equipo ${item.inventoryKey}?`;

    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      if (isActivo) {
        await deactivateEquipo(item.id);
        setSuccessMessage(`Equipo ${item.inventoryKey} desactivado correctamente.`);
      } else {
        await activateEquipo(item.id);
        setSuccessMessage(`Equipo ${item.inventoryKey} activado correctamente.`);
      }
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cambiar estado del equipo.");
    }
  }

  async function handleDeleteEquipment(item: Equipo) {
    const confirmMessage = `¿Estás seguro de que deseas eliminar permanentemente el equipo inactivo ${item.inventoryKey} (${item.nombreEquipo})?\n\nEsta acción no se puede deshacer.`;
    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      await deleteEquipo(item.id);
      setSuccessMessage(`Equipo ${item.inventoryKey} eliminado definitivamente del inventario.`);
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al eliminar el equipo.");
    }
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando catálogo de hardware...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            CATÁLOGO MAESTRO DE HARDWARE
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Inventario de Equipos
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Control de especificaciones técnicas, custodia física, marca y estado operativo.
          </p>
        </div>

        <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={() => void loadEquipment(false)}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            title="Refrescar catálogo"
          >
            <RefreshIcon size={14} />
            <span>Actualizar</span>
          </button>

          {canCreate && (
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={handleSyncDrive}
              disabled={syncingDrive}
              style={{ display: "inline-flex", alignItems: "center", gap: "6px", color: "var(--color-primary-700)", borderColor: "var(--color-primary-300)", background: "var(--color-primary-50)" }}
              title="Sincronizar inventario desde Google Drive (Excel)"
            >
              <RefreshIcon size={14} style={syncingDrive ? { animation: "spin 0.8s linear infinite" } : undefined} />
              <span>{syncingDrive ? "Sincronizando Drive..." : "Sincronizar Drive"}</span>
            </button>
          )}

          {canCreate && (
            <button
              type="button"
              className="btn btn-primary btn-sm"
              onClick={handleOpenCreate}
              style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
            >
              <PlusIcon size={14} />
              <span>Registrar equipo</span>
            </button>
          )}
        </div>
      </div>

      {/* Quick Status KPI Strip */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase" }}>Total Registrados</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--text-primary)" }}>{equipment.length}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <EquipmentIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase" }}>Operativos / Asignados</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-success-700)" }}>{activeCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <CheckIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase" }}>En Mantenimiento</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-warning-700)" }}>{maintenanceCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <MaintenanceIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase" }}>Inactivos / Baja</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--text-muted)" }}>{inactiveCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-slate-100)", color: "var(--color-slate-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <PackageIcon size={18} />
          </div>
        </div>
      </div>

      <AutoDismissAlert
        type="success"
        message={successMessage}
        duration={4500}
        onDismiss={() => setSuccessMessage(null)}
      />

      <AutoDismissAlert
        type="error"
        message={error}
        duration={5000}
        onDismiss={() => setError(null)}
      />

      {/* Filters Toolbar */}
      <EquipoFilters
        search={search}
        status={status}
        brand={brand}
        brands={brands}
        onSearchChange={(v) => { setSearch(v); setCurrentPage(1); }}
        onStatusChange={(v) => { setStatus(v); setCurrentPage(1); }}
        onBrandChange={(v) => { setBrand(v); setCurrentPage(1); }}
        onClear={clearFilters}
      />

      {/* Data Table */}
      <EquipoTable
        equipment={paginatedEquipment}
        onEdit={handleOpenEdit}
        onToggleStatus={handleToggleStatus}
        onDelete={handleDeleteEquipment}
        canEdit={canEdit}
        canDeactivate={canDeactivate}
        canDelete={canDeactivate}
      />

      {/* Pagination Footer */}
      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        totalItems={filteredEquipment.length}
        currentCount={paginatedEquipment.length}
        itemName="equipos"
        onPageChange={setCurrentPage}
      />

      {/* Modal */}
      <EquipoFormModal
        isOpen={isModalOpen}
        equipmentToEdit={editingEquipment}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveEquipment}
        saving={saving}
      />
    </div>
  );
}

export const EquipmentPage = EquiposPage;
