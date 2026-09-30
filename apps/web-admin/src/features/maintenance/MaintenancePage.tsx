import { useEffect, useMemo, useState } from "react";
import {
  getMaintenanceRecords,
  updateEquipmentMaintenance,
  type MaintenanceRecord,
} from "@/features/equipment/equipmentService";
import {
  MaintenanceIcon,
  CheckIcon,
  ClockIcon,
  RefreshIcon,
  SearchIcon,
  EditIcon,
  CloseIcon,
  EquipmentIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";
import { Modal } from "@/components/ui/Modal";
import { formatDate } from "@/shared/utils/date";

const ITEMS_PER_PAGE = 15;

export function MaintenancePage() {
  const [records, setRecords] = useState<MaintenanceRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState("");
  const [tipoFilter, setTipoFilter] = useState("");
  const [realizadoFilter, setRealizadoFilter] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  // Modal
  const [selectedItem, setSelectedItem] = useState<MaintenanceRecord | null>(null);
  const [modalTipo, setModalTipo] = useState("Preventivo");
  const [modalFecha, setModalFecha] = useState(new Date().toISOString().split("T")[0]);
  const [modalRealizado, setModalRealizado] = useState(true);
  const [saving, setSaving] = useState(false);

  async function loadData() {
    try {
      setLoading(true);
      setError(null);
      const data = await getMaintenanceRecords();
      setRecords(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los registros de mantenimiento.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, []);

  const stats = useMemo(() => {
    const result = records.reduce(
      (acc, r) => {
        const tipo = r.tipoMantenimiento?.toLowerCase();
        if (tipo === "preventivo") acc.preventivos++;
        if (tipo === "correctivo") acc.correctivos++;
        if (r.mantenimientoRealizado) acc.concluidos++;
        return acc;
      },
      { preventivos: 0, correctivos: 0, concluidos: 0 }
    );

    const total = records.length;
    const pendientes = total - result.concluidos;

    return { total, preventivos: result.preventivos, correctivos: result.correctivos, concluidos: result.concluidos, pendientes };
  }, [records]);

  const filteredRecords = useMemo(() => {
    const term = search.trim().toLowerCase();
    return records.filter((r) => {
      const matchesSearch =
        term === "" ||
        r.inventoryKey.toLowerCase().includes(term) ||
        r.nombreEquipo.toLowerCase().includes(term) ||
        (r.marca ?? "").toLowerCase().includes(term) ||
        (r.asignadoA ?? "").toLowerCase().includes(term) ||
        (r.departamento ?? "").toLowerCase().includes(term);

      const matchesTipo =
        tipoFilter === "" || (r.tipoMantenimiento ?? "").toLowerCase() === tipoFilter.toLowerCase();

      const matchesRealizado =
        realizadoFilter === "" ||
        (realizadoFilter === "si" ? r.mantenimientoRealizado : !r.mantenimientoRealizado);

      return matchesSearch && matchesTipo && matchesRealizado;
    });
  }, [records, search, tipoFilter, realizadoFilter]);

  const totalPages = Math.max(1, Math.ceil(filteredRecords.length / ITEMS_PER_PAGE));
  const paginatedRecords = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    return filteredRecords.slice(start, start + ITEMS_PER_PAGE);
  }, [filteredRecords, currentPage]);

  function handleOpenModal(item: MaintenanceRecord) {
    setSelectedItem(item);
    setModalTipo(item.tipoMantenimiento || "Preventivo");
    setModalFecha(item.fechaMantenimiento || new Date().toISOString().split("T")[0]);
    setModalRealizado(item.mantenimientoRealizado);
  }

  async function handleSaveModal(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedItem) return;

    try {
      setSaving(true);
      setError(null);
      await updateEquipmentMaintenance(selectedItem.id, {
        tipoMantenimiento: modalTipo,
        fechaMantenimiento: modalFecha || null,
        mantenimientoRealizado: modalRealizado,
      });

      setSuccessMessage(`Mantenimiento actualizado para el equipo ${selectedItem.inventoryKey}.`);
      setSelectedItem(null);
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al guardar el mantenimiento.");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando módulo de mantenimientos...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            SOPORTE TÉCNICO Y CONTROL DE HARDWARE
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Control de Mantenimientos
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Programa, registra y supervisa el estado de mantenimientos preventivos y correctivos.
          </p>
        </div>

        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={loadData}
          style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
        >
          <RefreshIcon size={14} />
          <span>Actualizar</span>
        </button>
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

      {/* 4 KPI Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase" }}>Total Equipos</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--text-primary)" }}>{stats.total}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <EquipmentIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-primary-700)", fontWeight: 700, textTransform: "uppercase" }}>Preventivos</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-primary-700)" }}>{stats.preventivos}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <MaintenanceIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase" }}>Correctivos</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-warning-700)" }}>{stats.correctivos}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <ClockIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase" }}>Concluidos</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-success-700)" }}>{stats.concluidos}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <CheckIcon size={18} />
          </div>
        </div>
      </div>

      {/* Toolbar */}
      <div className="toolbar-container" style={{ background: 'var(--bg-surface)', padding: '16px 20px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--border-default)' }}>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center', flex: 1, flexWrap: 'wrap' }}>
          <div className="search-input-wrapper">
            <span className="search-icon">
              <SearchIcon size={16} />
            </span>
            <input
              id="mantenimientos-search"
              name="search"
              type="text"
              className="form-input"
              placeholder="Buscar por placa, nombre, marca o responsable..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setCurrentPage(1); }}
            />
          </div>

          <div style={{ width: '180px' }}>
            <select
              id="mantenimientos-tipo-filter"
              name="tipoFilter"
              className="form-select"
              value={tipoFilter}
              onChange={(e) => { setTipoFilter(e.target.value); setCurrentPage(1); }}
            >
              <option value="">Todos los tipos</option>
              <option value="Preventivo">Preventivo</option>
              <option value="Correctivo">Correctivo</option>
            </select>
          </div>

          <div style={{ width: '180px' }}>
            <select
              id="mantenimientos-realizado-filter"
              name="realizadoFilter"
              className="form-select"
              value={realizadoFilter}
              onChange={(e) => { setRealizadoFilter(e.target.value); setCurrentPage(1); }}
            >
              <option value="">Todos los estados</option>
              <option value="si">Concluido / Al día</option>
              <option value="no">Pendiente</option>
            </select>
          </div>
        </div>

        {(search || tipoFilter || realizadoFilter) && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => { setSearch(""); setTipoFilter(""); setRealizadoFilter(""); setCurrentPage(1); }}
          >
            Restablecer
          </button>
        )}
      </div>

      {/* Table */}
      <div className="table-wrapper">
        <table className="data-table">
          <thead>
            <tr>
              <th>Placa / Clave</th>
              <th>Equipo</th>
              <th>Responsable</th>
              <th>Tipo Mantenimiento</th>
              <th>Fecha Programada</th>
              <th>Estado</th>
              <th style={{ textAlign: "center" }}>Acción</th>
            </tr>
          </thead>
          <tbody>
            {paginatedRecords.length === 0 ? (
              <tr>
                <td colSpan={7} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
                  No se encontraron mantenimientos coincidentes.
                </td>
              </tr>
            ) : (
              paginatedRecords.map((r) => (
                <tr key={r.id}>
                  <td className="primary-cell">
                    <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--color-primary-700)', fontWeight: 700 }}>
                      {r.inventoryKey}
                    </span>
                  </td>
                  <td>
                    <strong style={{ color: "var(--text-primary)" }}>{r.nombreEquipo}</strong>
                    <span style={{ fontSize: "11px", color: "var(--text-muted)", display: "block" }}>{r.marca ?? "Marca no especificada"}</span>
                  </td>
                  <td>{r.asignadoA ?? "Sin asignar"}</td>
                  <td>
                    <span className={r.tipoMantenimiento?.toLowerCase() === "preventivo" ? "badge badge-info" : "badge badge-warning"}>
                      {r.tipoMantenimiento || "No asignado"}
                    </span>
                  </td>
                  <td style={{ fontSize: "12.5px" }}>
                    {formatDate(r.fechaMantenimiento)}
                  </td>
                  <td>
                    <span className={r.mantenimientoRealizado ? "badge badge-success" : "badge badge-neutral"}>
                      <span className="badge-dot" />
                      {r.mantenimientoRealizado ? "Concluido" : "Pendiente"}
                    </span>
                  </td>
                  <td>
                    <div style={{ display: "flex", justifyContent: "center" }}>
                      <button
                        type="button"
                        className="btn btn-secondary btn-sm"
                        onClick={() => handleOpenModal(r)}
                        style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}
                      >
                        <EditIcon size={13} />
                        <span>Gestionar</span>
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: "16px" }}>
          <span style={{ fontSize: "12.5px", color: "var(--text-muted)" }}>
            Mostrando {paginatedRecords.length} de {filteredRecords.length} registros
          </span>
          <div style={{ display: "flex", gap: "6px" }}>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              disabled={currentPage === 1}
            >
              ← Anterior
            </button>
            <span style={{ padding: "6px 12px", fontSize: "12.5px", fontWeight: 600 }}>
              Página {currentPage} de {totalPages}
            </span>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
            >
              Siguiente →
            </button>
          </div>
        </div>
      )}

      {/* Modal */}
      {selectedItem && (
        <Modal isOpen={!!selectedItem} onClose={() => setSelectedItem(null)}>
          <div className="modal-header">
            <div>
              <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>SOPORTE TÉCNICO</span>
              <h2>Gestionar Mantenimiento: {selectedItem.inventoryKey}</h2>
            </div>
            <button type="button" className="modal-close" onClick={() => setSelectedItem(null)}>
              <CloseIcon size={16} />
            </button>
          </div>

          <form onSubmit={handleSaveModal}>
            <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "14px" }}>
              <div className="form-group">
                <label className="form-label" htmlFor="modalTipo">Tipo de Mantenimiento</label>
                <select
                  id="modalTipo"
                  name="modalTipo"
                  className="form-select"
                  value={modalTipo}
                  onChange={(e) => setModalTipo(e.target.value)}
                >
                  <option value="Preventivo">Preventivo (Limpieza, pasta térmica, diagnóstico)</option>
                  <option value="Correctivo">Correctivo (Reparación, cambio de componente)</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="modalFecha">Fecha de Mantenimiento</label>
                <input
                  id="modalFecha"
                  name="modalFecha"
                  type="date"
                  className="form-input"
                  value={modalFecha}
                  onChange={(e) => setModalFecha(e.target.value)}
                  min="2010-01-01"
                  max={new Date(Date.now() + 2 * 365 * 24 * 60 * 60 * 1000).toISOString().split("T")[0]}
                  required
                />
              </div>

              <div className="form-group" style={{ display: "flex", alignItems: "center", gap: "10px", marginTop: "6px" }}>
                <input
                  type="checkbox"
                  id="modal-realizado"
                  name="modalRealizado"
                  checked={modalRealizado}
                  onChange={(e) => setModalRealizado(e.target.checked)}
                  style={{ width: "16px", height: "16px", accentColor: "var(--color-primary-600)" }}
                />
                <label htmlFor="modal-realizado" className="form-label" style={{ margin: 0, cursor: "pointer" }}>
                  Mantenimiento realizado y verificado
                </label>
              </div>
            </div>

            <div className="modal-footer">
              <button type="button" className="btn btn-ghost" onClick={() => setSelectedItem(null)}>
                Cancelar
              </button>
              <button type="submit" className="btn btn-primary" disabled={saving || !modalFecha}>
                {saving ? "Guardando..." : "Guardar Mantenimiento"}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}

// Retrocompatibilidad
export { MaintenancePage as MantenimientosPage };
