import { useEffect, useMemo, useState } from "react";
import {
  getMaintenanceRecords,
  updateEquipmentMaintenance,
  type MaintenanceRecord,
} from "../equipment/equipmentService";

export function MantenimientosPage() {
  const [records, setRecords] = useState<MaintenanceRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Filters
  const [search, setSearch] = useState("");
  const [tipoFilter, setTipoFilter] = useState("");
  const [realizadoFilter, setRealizadoFilter] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const ITEMS_PER_PAGE = 15;

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
    let mounted = true;
    getMaintenanceRecords()
      .then((data: MaintenanceRecord[]) => {
        if (mounted) {
          setRecords(data);
          setLoading(false);
        }
      })
      .catch((err: unknown) => {
        if (mounted) {
          setError(err instanceof Error ? err.message : "Error al cargar los mantenimientos.");
          setLoading(false);
        }
      });

    return () => {
      mounted = false;
    };
  }, []);

  const stats = useMemo(() => {
    const total = records.length;
    const preventivos = records.filter((r) => r.tipoMantenimiento?.toLowerCase() === "preventivo").length;
    const correctivos = records.filter((r) => r.tipoMantenimiento?.toLowerCase() === "correctivo").length;
    const concluidos = records.filter((r) => r.mantenimientoRealizado).length;
    const pendientes = total - concluidos;

    return { total, preventivos, correctivos, concluidos, pendientes };
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
      <main className="inventory-page">
        <div className="inventory-state">
          <span className="inventory-loader" />
          <strong>Cargando módulo de mantenimientos</strong>
          <small>Consultando historial de soporte técnico...</small>
        </div>
      </main>
    );
  }

  return (
    <main className="inventory-page">
      {/* HEADER */}
      <header className="inventory-header">
        <div>
          <span className="inventory-kicker">SOPORTE TÉCNICO Y CONTROL DE HARDWARE</span>
          <h1>Control de Mantenimientos</h1>
          <p>
            Programa, registra y supervisa el estado de mantenimientos preventivos y correctivos de los equipos tecnológicos.
          </p>
        </div>

        <div className="inventory-header-actions">
          <button
            type="button"
            className="actas-reset"
            onClick={loadData}
            style={{ display: "flex", alignItems: "center", gap: "6px" }}
          >
            ↻ Actualizar
          </button>
        </div>
      </header>

      {/* MENSAJES */}
      {successMessage && (
        <div className="usuarios-success" style={{ marginBottom: "20px" }}>
          <span>✓</span>
          {successMessage}
          <button type="button" onClick={() => setSuccessMessage(null)} aria-label="Cerrar">
            ×
          </button>
        </div>
      )}

      {error && (
        <div className="actas-error" style={{ marginBottom: "20px" }}>
          {error}
        </div>
      )}

      {/* 4 TARJETAS KPI DE MANTENIMIENTO */}
      <section className="inventory-metrics">
        <article className="inventory-metric inventory-metric-dark">
          <div className="inventory-metric-top">
            <span>TOTAL EQUIPOS</span>
            <span>⚙</span>
          </div>
          <strong>{stats.total}</strong>
          <p>Equipos bajo supervisión técnica</p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>PREVENTIVOS</span>
            <span style={{ color: "#6366f1" }}>● </span>
          </div>
          <strong style={{ color: "#6366f1" }}>{stats.preventivos}</strong>
          <p>Revisiones periódicas registradas</p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>CORRECTIVOS</span>
            <span style={{ color: "#f59e0b" }}>● </span>
          </div>
          <strong style={{ color: "#f59e0b" }}>{stats.correctivos}</strong>
          <p>Reparaciones y soporte puntual</p>
        </article>

        <article className="inventory-metric">
          <div className="inventory-metric-top">
            <span>CONCLUIDOS</span>
            <span className="live-dot" />
          </div>
          <strong style={{ color: "#10b981" }}>{stats.concluidos}</strong>
          <p>
            {stats.total > 0
              ? `${Math.round((stats.concluidos / stats.total) * 100)}% al día`
              : "100% al día"}
          </p>
        </article>
      </section>

      {/* FILTROS */}
      <div
        style={{
          background: "#ffffff",
          borderRadius: "14px",
          padding: "16px 20px",
          border: "1px solid #e2e8f0",
          boxShadow: "0 1px 3px rgba(0,0,0,0.03)",
          marginBottom: "20px",
          display: "flex",
          flexWrap: "wrap",
          gap: "14px",
          alignItems: "center",
        }}
      >
        <div style={{ flex: "1 1 260px" }}>
          <input
            type="text"
            className="search-input"
            placeholder="Buscar por código, nombre, marca, responsable..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setCurrentPage(1);
            }}
            style={{ width: "100%", padding: "10px 14px", borderRadius: "8px", border: "1px solid #cbd5e1" }}
          />
        </div>

        <div style={{ width: "180px" }}>
          <select
            className="filter-select"
            value={tipoFilter}
            onChange={(e) => {
              setTipoFilter(e.target.value);
              setCurrentPage(1);
            }}
            style={{ width: "100%", padding: "10px 12px", borderRadius: "8px", border: "1px solid #cbd5e1" }}
          >
            <option value="">Todos los Tipos</option>
            <option value="Preventivo">Preventivo</option>
            <option value="Correctivo">Correctivo</option>
          </select>
        </div>

        <div style={{ width: "180px" }}>
          <select
            className="filter-select"
            value={realizadoFilter}
            onChange={(e) => {
              setRealizadoFilter(e.target.value);
              setCurrentPage(1);
            }}
            style={{ width: "100%", padding: "10px 12px", borderRadius: "8px", border: "1px solid #cbd5e1" }}
          >
            <option value="">Todos los Estados</option>
            <option value="si">Concluido / Al día</option>
            <option value="no">Pendiente</option>
          </select>
        </div>

        {(search || tipoFilter || realizadoFilter) && (
          <button
            type="button"
            className="actas-reset"
            onClick={() => {
              setSearch("");
              setTipoFilter("");
              setRealizadoFilter("");
              setCurrentPage(1);
            }}
            style={{ padding: "8px 14px" }}
          >
            Limpiar filtros
          </button>
        )}
      </div>

      {/* TABLA DE MANTENIMIENTOS */}
      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Inventario</th>
              <th>Equipo</th>
              <th>Marca</th>
              <th>Asignado a</th>
              <th>Departamento</th>
              <th>Tipo Mantenimiento</th>
              <th>Fecha Registro</th>
              <th>Estado</th>
              <th style={{ textAlign: "center" }}>Acción</th>
            </tr>
          </thead>
          <tbody>
            {paginatedRecords.length === 0 ? (
              <tr>
                <td colSpan={9} style={{ textAlign: "center", padding: "32px", color: "#64748b" }}>
                  No se encontraron registros de mantenimiento con los criterios seleccionados.
                </td>
              </tr>
            ) : (
              paginatedRecords.map((r) => {
                const isRealizado = r.mantenimientoRealizado;
                const isPreventivo = r.tipoMantenimiento?.toLowerCase() === "preventivo";

                return (
                  <tr key={r.id}>
                    <td>
                      <strong className="equipment-key">{r.inventoryKey}</strong>
                    </td>
                    <td>{r.nombreEquipo}</td>
                    <td>{r.marca ?? "—"}</td>
                    <td>{r.asignadoA ?? "—"}</td>
                    <td>{r.departamento ?? "—"}</td>
                    <td>
                      <span
                        style={{
                          display: "inline-block",
                          padding: "3px 8px",
                          borderRadius: "6px",
                          fontSize: "12px",
                          fontWeight: 700,
                          backgroundColor: isPreventivo ? "#eff6ff" : "#fef3c7",
                          color: isPreventivo ? "#2563eb" : "#d97706",
                        }}
                      >
                        {r.tipoMantenimiento ?? "Preventivo"}
                      </span>
                    </td>
                    <td>{r.fechaMantenimiento ?? "—"}</td>
                    <td>
                      <span
                        className={isRealizado ? "status-badge status-active" : "status-badge status-warning"}
                      >
                        {isRealizado ? "Concluido" : "Pendiente"}
                      </span>
                    </td>
                    <td style={{ textAlign: "center" }}>
                      <button
                        type="button"
                        onClick={() => handleOpenModal(r)}
                        className="usuario-edit-button"
                        style={{ padding: "5px 12px", fontSize: "12px", display: "inline-flex", alignItems: "center", gap: "4px" }}
                      >
                        ⚙ Registrar
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* PAGINACIÓN */}
      {totalPages > 1 && (
        <div className="pagination" style={{ marginTop: "20px" }}>
          <button
            type="button"
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={currentPage === 1}
          >
            ←  Anterior
          </button>
          <span className="pagination-info">
            {currentPage} / {totalPages}
          </span>
          <button
            type="button"
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
          >
            Siguiente →
          </button>
        </div>
      )}

      {/* MODAL DE GESTIÓN DE MANTENIMIENTO */}
      {selectedItem && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: "rgba(15, 23, 42, 0.6)",
            backdropFilter: "blur(4px)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1000,
            padding: "20px",
          }}
        >
          <div
            style={{
              background: "#ffffff",
              borderRadius: "16px",
              width: "100%",
              maxWidth: "500px",
              padding: "24px",
              boxShadow: "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)",
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
              <h3 style={{ margin: 0, fontSize: "18px", fontWeight: 700, color: "#0f172a" }}>
                Registrar Mantenimiento
              </h3>
              <button
                type="button"
                onClick={() => setSelectedItem(null)}
                style={{ background: "none", border: "none", fontSize: "18px", cursor: "pointer", color: "#64748b" }}
              >
                ✕
              </button>
            </div>

            <div style={{ background: "#f8fafc", padding: "12px 14px", borderRadius: "8px", marginBottom: "18px", fontSize: "13px" }}>
              <div><strong>Equipo:</strong> {selectedItem.nombreEquipo} ({selectedItem.inventoryKey})</div>
              <div><strong>Custodio:</strong> {selectedItem.asignadoA || "No asignado"}</div>
            </div>

            <form onSubmit={handleSaveModal}>
              <div style={{ marginBottom: "14px" }}>
                <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
                  Tipo de Mantenimiento
                </label>
                <select
                  value={modalTipo}
                  onChange={(e) => setModalTipo(e.target.value)}
                  style={{ width: "100%", padding: "10px", borderRadius: "8px", border: "1px solid #cbd5e1" }}
                  required
                >
                  <option value="Preventivo">Preventivo (Limpieza, revisión y diagnóstico)</option>
                  <option value="Correctivo">Correctivo (Reparación, cambio de partes)</option>
                </select>
              </div>

              <div style={{ marginBottom: "14px" }}>
                <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
                  Fecha de Mantenimiento
                </label>
                <input
                  type="date"
                  value={modalFecha}
                  onChange={(e) => setModalFecha(e.target.value)}
                  style={{ width: "100%", padding: "10px", borderRadius: "8px", border: "1px solid #cbd5e1" }}
                  required
                />
              </div>

              <div style={{ marginBottom: "22px", display: "flex", alignItems: "center", gap: "10px" }}>
                <input
                  type="checkbox"
                  id="modalRealizado"
                  checked={modalRealizado}
                  onChange={(e) => setModalRealizado(e.target.checked)}
                  style={{ width: "18px", height: "18px", cursor: "pointer" }}
                />
                <label htmlFor="modalRealizado" style={{ fontSize: "13px", fontWeight: 600, color: "#1e293b", cursor: "pointer" }}>
                  Mantenimiento concluido satisfactoriamente
                </label>
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px" }}>
                <button
                  type="button"
                  className="actas-reset"
                  onClick={() => setSelectedItem(null)}
                  disabled={saving}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="actas-primary"
                  disabled={saving}
                >
                  {saving ? "Guardando..." : "Guardar Registro"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </main>
  );
}

