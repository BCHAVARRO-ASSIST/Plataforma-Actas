import { useEffect, useMemo, useState } from "react";
import {
  getActaById,
  getActas,
} from "./actaService";

import type {
  Acta,
  ActaDetail,
} from "./actaService";

import { ActaDetail as ActaDetailComponent } from "./components/ActaDetail";
import { ActaTable } from "./components/ActaTable";
import { NewActaForm } from "./components/NewActaForm";

export function ActaPage() {
  const [actas, setActas] = useState<Acta[]>([]);
  const [selectedActa, setSelectedActa] =
    useState<ActaDetail | null>(null);

  const [creating, setCreating] = useState(false);
  const [loading, setLoading] = useState(true);

  const [error, setError] =
    useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] =
    useState("");

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
      setError(
        err instanceof Error
          ? err.message
          : "Error al cargar las actas."
      );
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
      setError(
        err instanceof Error
          ? err.message
          : "Error al cargar el detalle."
      );
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
        acta.id.toLowerCase().includes(value) ||
        acta.equipmentId.toLowerCase().includes(value) ||
        String(acta.usuarioId).includes(value);

      const matchesStatus =
        statusFilter === "" ||
        acta.estado === statusFilter;

      return matchesSearch && matchesStatus;
    });
  }, [actas, search, statusFilter]);

  const draftCount = actas.filter(
    (acta) => acta.estado === "Borrador"
  ).length;

  const deliveredCount = actas.filter(
    (acta) => acta.estado === "Firmada" || acta.actaEntregada
  ).length;

  const finalizedCount = actas.filter(
    (acta) => acta.estado === "Finalizada"
  ).length;

  if (loading) {
    return (
      <main className="actas-page">
        <div className="actas-state">
          <span className="actas-loader" />
          <strong>Cargando actas</strong>
          <small>
            Consultando el registro documental...
          </small>
        </div>
      </main>
    );
  }

  if (selectedActa) {
    return (
      <ActaDetailComponent
        acta={selectedActa}
        onBack={() => {
          setSelectedActa(null);
          loadActas();
        }}
        onRefresh={handleRefreshDetail}
      />
    );
  }

  if (creating) {
    return (
      <main className="actas-page">
        <NewActaForm
          onCreated={handleCreated}
          onCancel={() => setCreating(false)}
        />
      </main>
    );
  }

  return (
    <main className="actas-page">

      {/* HEADER */}

      <header className="actas-header">

        <div>
          <span className="actas-kicker">
            GESTIÓN DOCUMENTAL Y TRAZABILIDAD
          </span>

          <h1>Control de Actas de Entrega y Devolución</h1>

          <p>
            Emisión, supervisión y custodia digital de actas con firma manuscrita certificada y sellado criptográfico.
          </p>
        </div>

        <div className="actas-header-actions">

          <span className="actas-live">
            <span className="live-dot" />
            Flujo documental activo
          </span>

          <button
            type="button"
            className="actas-primary"
            onClick={() => setCreating(true)}
          >
            + Crear nueva acta
          </button>

        </div>

      </header>



      {/* METRICS */}

      <section className="actas-metrics">

        <article className="actas-metric actas-metric-dark">

          <div className="actas-metric-top">
            <span>TOTAL DE ACTAS</span>
            <span>â–¤</span>
          </div>

          <strong>{actas.length}</strong>

          <p>Documentos totales en base de datos</p>

        </article>

        <article className="actas-metric">

          <div className="actas-metric-top">
            <span>BORRADORES</span>
            <span className="live-dot" />
          </div>

          <strong>{draftCount}</strong>

          <p>Pendientes de entrega y firma</p>

        </article>

        <article className="actas-metric">

          <div className="actas-metric-top">
            <span>FIRMADA / ENTREGADA</span>
            <span>✓</span>
          </div>

          <strong>{deliveredCount}</strong>

          <p>Con firma certificada SHA-256</p>

        </article>

        <article className="actas-metric">

          <div className="actas-metric-top">
            <span>FINALIZADAS</span>
            <span>â—‰</span>
          </div>

          <strong>{finalizedCount}</strong>

          <p>Ciclos cerrados y archivados</p>

        </article>

      </section>


      {/* ERROR */}

      {error && (
        <div className="actas-error">
          {error}
        </div>
      )}

      {/* TABLE PANEL */}

      <section className="actas-panel">

        <div className="actas-panel-header">

          <div>
            <span>
              Registro documental
            </span>

            <h2>Actas registradas</h2>

            <p>
              Consulta el estado y la información
              de cada documento.
            </p>
          </div>

          <div className="actas-result-count">
            {filteredActas.length} resultados
          </div>

        </div>

        <div className="actas-filters">

          <div className="actas-search">

            <span>âŒ•</span>

            <input
              type="text"
              placeholder="Buscar por ID, equipo o usuario..."
              value={search}
              onChange={(event) =>
                setSearch(event.target.value)
              }
            />

            {search && (
              <button
                type="button"
                onClick={() => setSearch("")}
              >
                ×
              </button>
            )}

          </div>

          <select
            value={statusFilter}
            onChange={(event) =>
              setStatusFilter(event.target.value)
            }
          >
            <option value="">
              Todos los estados
            </option>

            <option value="Borrador">
              Borrador
            </option>

            <option value="Firmada">
              Firmada
            </option>

            <option value="Finalizada">
              Finalizada
            </option>

            <option value="Anulada">
              Anulada
            </option>

          </select>

          <button
            type="button"
            className="actas-reset"
            onClick={() => {
              setSearch("");
              setStatusFilter("");
            }}
          >
            Restablecer
          </button>

        </div>

        {filteredActas.length === 0 ? (
          <div className="actas-empty">
            <div>â–¤</div>

            <strong>
              No encontramos actas
            </strong>

            <span>
              Cambia los filtros o crea una
              nueva acta.
            </span>
          </div>
        ) : (
          <ActaTable
            actas={filteredActas}
            onSelect={handleSelect}
          />
        )}

      </section>

    </main>
  );
}
