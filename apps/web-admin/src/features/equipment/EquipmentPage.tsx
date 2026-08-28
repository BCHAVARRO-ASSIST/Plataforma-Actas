import { useEffect, useMemo, useState } from "react";
import { EquipmentFilters } from "./components/EquipmentFilters";
import { EquipmentTable } from "./components/EquipmentTable";
import { EquipmentFormModal } from "./components/EquipmentFormModal";
import {
  getEquipment,
  createEquipment,
  updateEquipment,
  deactivateEquipment,
  activateEquipment,
  deleteEquipment,
} from "./equipmentService";
import type { Equipment, CreateEquipmentRequest, UpdateEquipmentRequest } from "./equipmentService";
import { useAuth } from "../auth/AuthContext";
import { PERMISSIONS, hasPermission } from "../auth/permissions";

const ITEMS_PER_PAGE = 15;

export function EquipmentPage() {
  const { token } = useAuth();

  const [equipment, setEquipment] = useState<Equipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [brand, setBrand] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEquipment, setEditingEquipment] = useState<Equipment | null>(null);
  const [saving, setSaving] = useState(false);

  const canCreate = hasPermission(token, PERMISSIONS.EQUIPOS_REGISTRAR);
  const canEdit = hasPermission(token, PERMISSIONS.EQUIPOS_EDITAR);
  const canDeactivate = hasPermission(token, PERMISSIONS.EQUIPOS_DESACTIVAR);

  async function loadEquipment() {
    try {
      setLoading(true);
      setError(null);
      const data = await getEquipment();
      setEquipment(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "No fue posible cargar el inventario."
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let mounted = true;
    getEquipment()
      .then((data) => {
        if (mounted) {
          setEquipment(data);
          setError(null);
          setLoading(false);
        }
      })
      .catch((err) => {
        if (mounted) {
          setError(
            err instanceof Error
              ? err.message
              : "No fue posible cargar el inventario."
          );
          setLoading(false);
        }
      });

    return () => {
      mounted = false;
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

      const matchesStatus =
        status === "" || item.estatusEquipo === status;

      const matchesBrand =
        brand === "" || item.marca === brand;

      return matchesSearch && matchesStatus && matchesBrand;
    });
  }, [equipment, search, status, brand]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredEquipment.length / ITEMS_PER_PAGE)
  );

  const paginatedEquipment = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    return filteredEquipment.slice(start, start + ITEMS_PER_PAGE);
  }, [filteredEquipment, currentPage]);

  const activeEquipment = equipment.filter(
    (item) => item.estatusEquipo === "Activo"
  ).length;

  const inactiveEquipment = equipment.filter(
    (item) => item.estatusEquipo === "Inactivo"
  ).length;

  function clearFilters() {
    setSearch("");
    setStatus("");
    setBrand("");
    setCurrentPage(1);
  }

  function handleSearchChange(value: string) {
    setSearch(value);
    setCurrentPage(1);
  }

  function handleStatusChange(value: string) {
    setStatus(value);
    setCurrentPage(1);
  }

  function handleBrandChange(value: string) {
    setBrand(value);
    setCurrentPage(1);
  }

  function goToPage(page: number) {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  }

  function handleOpenCreate() {
    setEditingEquipment(null);
    setIsModalOpen(true);
  }

  function handleOpenEdit(item: Equipment) {
    setEditingEquipment(item);
    setIsModalOpen(true);
  }

  async function handleSaveEquipment(data: CreateEquipmentRequest | UpdateEquipmentRequest) {
    try {
      setSaving(true);
      if (editingEquipment) {
        await updateEquipment(editingEquipment.id, data as UpdateEquipmentRequest);
        setSuccessMessage(`Equipo ${data.inventoryKey} actualizado correctamente.`);
      } else {
        await createEquipment(data as CreateEquipmentRequest);
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

  async function handleToggleStatus(item: Equipment) {
    const isActivo = item.estatusEquipo?.toLowerCase() === "activo" || item.estatusEquipo?.toLowerCase() === "asignado";
    const confirmMessage = isActivo
      ? `Â¿EstÃ¡s seguro de que deseas desactivar el equipo ${item.inventoryKey}?`
      : `Â¿EstÃ¡s seguro de que deseas activar el equipo ${item.inventoryKey}?`;

    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      if (isActivo) {
        await deactivateEquipment(item.id);
        setSuccessMessage(`Equipo ${item.inventoryKey} desactivado correctamente.`);
      } else {
        await activateEquipment(item.id);
        setSuccessMessage(`Equipo ${item.inventoryKey} activado correctamente.`);
      }
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cambiar estado del equipo.");
    }
  }

  async function handleDeleteEquipment(item: Equipment) {
    const confirmMessage = `Â¿EstÃ¡s seguro de que deseas eliminar permanentemente el equipo inactivo ${item.inventoryKey} (${item.nombreEquipo})?\n\nEsta acciÃ³n eliminarÃ¡ el registro de la base de datos.`;
    if (!window.confirm(confirmMessage)) return;

    try {
      setError(null);
      await deleteEquipment(item.id);
      setSuccessMessage(`Equipo ${item.inventoryKey} eliminado definitivamente del inventario.`);
      await loadEquipment();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al eliminar el equipo.");
    }
  }

  if (loading) {
    return (
      <main className="inventory-page">
        <div className="inventory-state">
          <span className="inventory-loader" />
          <strong>Cargando inventario</strong>
          <small>Consultando equipos registrados...</small>
        </div>
      </main>
    );
  }

  const firstItem =
    filteredEquipment.length === 0
      ? 0
      : (currentPage - 1) * ITEMS_PER_PAGE + 1;

  const lastItem = Math.min(
    currentPage * ITEMS_PER_PAGE,
    filteredEquipment.length
  );

  return (
    <main className="inventory-page">
      {/* HEADER */}
      <header className="inventory-header">
        <div>
          <span className="inventory-kicker">CATÃLOGO MAESTRO DE HARDWARE</span>
          <h1>Inventario de Equipos TecnolÃ³gicos</h1>
          <p>
            Control de especificaciones tÃ©cnicas, custodia fÃ­sica, marca y estado operativo de los equipos de la organizaciÃ³n.
          </p>
        </div>

        <div className="inventory-header-actions">
          <span className="inventory-live">
            <span className="live-dot" />
            CatÃ¡logo sincronizado
          </span>

          {canCreate && (
            <button
              type="button"
              className="actas-primary"
              onClick={handleOpenCreate}
            >
              + Registrar equipo
            </button>
          )}
        </div>
      </header>

      {/* MENSAJE DE Ã‰XITO */}
      {successMessage && (
        <div className="usuarios-success" style={{ marginBottom: "20px" }}>
          <span>âœ“</span>
          {successMessage}
          <button type="button" onClick={() => setSuccessMessage(null)} aria-label="Cerrar">
            Ã—
          </button>
        </div>
      )}

      {/* ERROR */}
      {error && (
        <div className="actas-error" style={{ marginBottom: "20px" }}>
          {error}
        </div>
      )}

      {/* METRICS */}
      <section className="inventory-metrics">
        <article className="inventory-metric inventory-metric-dark">
          <div className="inventory-metric-top">
            <span>TOTAL EQUIPOS</span>
            <span>â–¦</span>
          </div>
          <strong>{equipment.length}</strong>
          <p>Activos totales registrados</p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>EQUIPOS ACTIVOS</span>
            <span className="live-dot" />
          </div>
          <strong>{activeEquipment}</strong>
          <p>
            {equipment.length > 0
              ? `${Math.round((activeEquipment / equipment.length) * 100)}% en operaciÃ³n`
              : "Sin datos"}
          </p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>INACTIVOS / BODEGA</span>
            <span>â—‹</span>
          </div>
          <strong>{inactiveEquipment}</strong>
          <p>Equipos disponibles o en baja</p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>FILTRADOS</span>
            <span>âŒ•</span>
          </div>
          <strong>{filteredEquipment.length}</strong>
          <p>Coincidencias en bÃºsqueda</p>
        </article>
      </section>

      {/* FILTROS */}
      <EquipmentFilters
        search={search}
        status={status}
        brand={brand}
        brands={brands}
        onSearchChange={handleSearchChange}
        onStatusChange={handleStatusChange}
        onBrandChange={handleBrandChange}
        onClear={clearFilters}
      />

      {/* RESULTADOS INFO */}
      <div className="inventory-results-info">
        <span>
          Mostrando <strong>{firstItem}</strong> â€“ <strong>{lastItem}</strong> de <strong>{filteredEquipment.length}</strong> equipos
        </span>

        {filteredEquipment.length > 0 && (
          <span className="inventory-page-info">
            PÃ¡gina {currentPage} de {totalPages}
          </span>
        )}
      </div>

      {/* TABLA */}
      {filteredEquipment.length === 0 ? (
        <div className="inventory-state">
          <strong>No se encontraron equipos</strong>
          <small>Prueba ajustando los filtros de bÃºsqueda</small>
        </div>
      ) : (
        <EquipmentTable
          equipment={paginatedEquipment}
          onEdit={handleOpenEdit}
          onToggleStatus={handleToggleStatus}
          onDelete={handleDeleteEquipment}
          canEdit={canEdit}
          canDeactivate={canDeactivate}
          canDelete={canDeactivate}
        />
      )}

      {/* PAGINACIÃ“N */}
      {totalPages > 1 && (
        <div className="pagination">
          <button
            type="button"
            onClick={() => goToPage(currentPage - 1)}
            disabled={currentPage === 1}
          >
            â† Anterior
          </button>

          <span className="pagination-info">
            {currentPage} / {totalPages}
          </span>

          <button
            type="button"
            onClick={() => goToPage(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            Siguiente â†’
          </button>
        </div>
      )}

      {/* MODAL REGISTRAR / EDITAR */}
      <EquipmentFormModal
        isOpen={isModalOpen}
        equipmentToEdit={editingEquipment}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveEquipment}
        saving={saving}
      />

    </main>
  );
}
