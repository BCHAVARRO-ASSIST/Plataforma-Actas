import { useEffect, useMemo, useState } from "react";
import {
  getActaById,
  getActas,
} from "./actaService";
import type {
  Acta,
  ActaDetail,
} from "./actaService";

import { ActaDetail as ActaDetailComponent } from "../../components/ActaDetail";
import { ActaTable } from "../../components/ActaTable";
import { NewActaForm } from "../../components/NewActaForm";
import {
  ActaIcon,
  ClockIcon,
  SignatureIcon,
  MessengerIcon,
  RefreshIcon,
  PlusIcon,
  SearchIcon,
} from "../../components/Icons";
import { AutoDismissAlert } from "../../components/AutoDismissAlert";

export function ActaPage() {
  const [actas, setActas] = useState<Acta[]>([]);
  const [selectedActa, setSelectedActa] = useState<ActaDetail | null>(null);
  const [creating, setCreating] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  useEffect(() => {
    loadActas();
  }, []);

  async function loadActas() {
    try {
      setLoading(true);
      setError(null);
      const data = await getActas();
      setActas(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar las actas.");
    } finally {
      setLoading(false);
    }
  }

  async function handleSelect(id: string) {
    try {
      setError(null);
      const detail = await getActaById(id);
      setSelectedActa(detail);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el detalle.");
    }
  }

  async function handleRefreshDetail() {
    if (selectedActa) {
      await handleSelect(selectedActa.id);
    }
    await loadActas();
  }

  function handleCreated() {
    setCreating(false);
    loadActas();
  }

  const filteredActas = useMemo(() => {
    const value = search.trim().toLowerCase();

    return actas.filter((acta) => {
      const matchesSearch =
        value === "" ||
        (acta.numeroActa ?? "").toLowerCase().includes(value) ||
        (acta.nombreEquipo ?? "").toLowerCase().includes(value) ||
        (acta.inventoryKey ?? "").toLowerCase().includes(value) ||
        acta.id.toLowerCase().includes(value);

      const matchesStatus =
        statusFilter === "" || acta.estado.toLowerCase() === statusFilter.toLowerCase();

      return matchesSearch && matchesStatus;
    });
  }, [actas, search, statusFilter]);

  const { draftCount, signedCount, deliveredCount } = useMemo(() => {
    return actas.reduce(
      (acc, a) => {
        const estado = a.estado.toLowerCase();
        if (estado === "borrador") acc.draftCount++;
        if (estado === "firmada" || estado === "finalizada") acc.signedCount++;
        if (a.actaEntregada) acc.deliveredCount++;
        return acc;
      },
      { draftCount: 0, signedCount: 0, deliveredCount: 0 }
    );
  }, [actas]);

  if (selectedActa) {
    return (
      <ActaDetailComponent
        acta={selectedActa}
        onBack={() => { setSelectedActa(null); loadActas(); }}
        onRefresh={handleRefreshDetail}
      />
    );
  }

  if (creating) {
    return (
      <NewActaForm
        onCancel={() => setCreating(false)}
        onCreated={handleCreated}
      />
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            GESTIÓN DOCUMENTAL LEGAL
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Actas de Entrega y Devolución
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Generación, firma digital manuscrita y custodia de actas de activos tecnológicos.
          </p>
        </div>

        <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={loadActas}
            disabled={loading}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
          >
            <RefreshIcon size={14} />
            <span>Actualizar</span>
          </button>

          <button
            type="button"
            className="btn btn-primary btn-sm"
            onClick={() => setCreating(true)}
            style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
          >
            <PlusIcon size={14} />
            <span>Nueva acta</span>
          </button>
        </div>
      </div>

      {/* KPI Cards Strip */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "12px", marginBottom: "20px" }}>
        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 700, textTransform: "uppercase" }}>Total Emitidas</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--text-primary)" }}>{actas.length}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <ActaIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-warning-700)", fontWeight: 700, textTransform: "uppercase" }}>Borradores</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-warning-700)" }}>{draftCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-warning-50)", color: "var(--color-warning-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <ClockIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-success-700)", fontWeight: 700, textTransform: "uppercase" }}>Firmadas</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-success-700)" }}>{signedCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <SignatureIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "14px 18px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <div>
            <span style={{ fontSize: "11px", color: "var(--color-info-700)", fontWeight: 700, textTransform: "uppercase" }}>Entregadas</span>
            <div style={{ fontSize: "22px", fontWeight: 800, color: "var(--color-info-700)" }}>{deliveredCount}</div>
          </div>
          <div style={{ width: "36px", height: "36px", borderRadius: "var(--radius-md)", background: "var(--color-info-50)", color: "var(--color-info-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <MessengerIcon size={18} />
          </div>
        </div>
      </div>

      <AutoDismissAlert
        type="error"
        message={error}
        duration={5000}
        onDismiss={() => setError(null)}
      />

      {/* Toolbar */}
      <div className="toolbar-container" style={{ background: 'var(--bg-surface)', padding: '16px 20px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--border-default)' }}>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center', flex: 1, flexWrap: 'wrap' }}>
          <div className="search-input-wrapper">
            <span className="search-icon">
              <SearchIcon size={16} />
            </span>
            <input
              id="actas-search"
              name="search"
              className="form-input"
              type="text"
              placeholder="Buscar por número de acta, equipo, placa..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>

          <div style={{ width: '180px' }}>
            <select
              id="actas-status-filter"
              name="statusFilter"
              className="form-select"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="">Todos los estados</option>
              <option value="Borrador">Borrador</option>
              <option value="Firmada">Firmada</option>
              <option value="Finalizada">Finalizada</option>
              <option value="Anulada">Anulada</option>
            </select>
          </div>
        </div>

        {(search || statusFilter) && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => { setSearch(""); setStatusFilter(""); }}
          >
            Restablecer
          </button>
        )}
      </div>

      {/* Table */}
      {loading ? (
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '200px', gap: '12px' }}>
          <div style={{ width: '28px', height: '28px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
          <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando registros de actas...</span>
        </div>
      ) : (
        <ActaTable
          actas={filteredActas}
          onSelect={handleSelect}
        />
      )}
    </div>
  );
}
