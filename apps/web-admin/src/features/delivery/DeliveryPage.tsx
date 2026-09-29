import { useEffect, useState } from "react";
import type { DeliveryActa } from "./deliveryService";
import { getDeliveryActas } from "./deliveryService";
import { ActaTable } from "@/features/actas/components/ActaTable";
import {
  MessengerIcon,
  ClockIcon,
  SignatureIcon,
  PackageIcon,
  RefreshIcon,
  SearchIcon,
} from "@/components/ui/Icons";

export interface DeliveryPageProps {
  onSelectActa: (id: string) => void;
}
export type CourierPageProps = DeliveryPageProps;
export type MensajeroPageProps = DeliveryPageProps;

export function DeliveryPage({ onSelectActa }: DeliveryPageProps) {
  const [actas, setActas] = useState<DeliveryActa[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");

  async function loadActas() {
    try {
      setLoading(true);
      setError(null);
      const data = await getDeliveryActas();
      setActas(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "No fue posible cargar las actas asignadas.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadActas();
  }, []);

  const term = search.trim().toLowerCase();
  const filteredActas = term === "" ? actas : actas.filter((a) => (
    (a.numeroActa ?? "").toLowerCase().includes(term) ||
    (a.nombreEquipo ?? "").toLowerCase().includes(term) ||
    (a.inventoryKey ?? "").toLowerCase().includes(term)
  ));

  const { totalPendientes, totalFirmadas, totalEntregadas } = actas.reduce(
    (acc, a) => {
      const estado = a.estado.toLowerCase();
      if (!a.actaEntregada && estado !== "anulada") acc.totalPendientes++;
      if (estado === "firmada" || estado === "finalizada") acc.totalFirmadas++;
      if (a.actaEntregada) acc.totalEntregadas++;
      return acc;
    },
    { totalPendientes: 0, totalFirmadas: 0, totalEntregadas: 0 }
  );

  if (loading) {
    return (
      <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", minHeight: "300px", gap: "12px" }}>
        <div style={{ width: "32px", height: "32px", border: "3px solid var(--border-default)", borderTopColor: "var(--color-primary-600)", borderRadius: "50%", animation: "spin 0.8s linear infinite" }} />
        <span style={{ fontSize: "13px", color: "var(--text-muted)" }}>Cargando traslados asignados...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            PANEL OPERATIVO DE CAMPO
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Mis Entregas y Traslados
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Actas asignadas para recolección de firma en campo y entrega física de equipos.
          </p>
        </div>

        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={loadActas}
          disabled={loading}
          style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
        >
          <RefreshIcon size={14} />
          <span>Actualizar traslados</span>
        </button>
      </div>

      {/* KPI Cards Grid */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(210px, 1fr))", gap: "14px", marginBottom: "20px" }}>
        <div className="stat-card">
          <div className="stat-info">
            <span className="stat-label">Total Asignadas</span>
            <span className="stat-value">{actas.length}</span>
          </div>
          <div className="stat-icon-wrapper">
            <MessengerIcon size={20} />
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-info">
            <span className="stat-label" style={{ color: "var(--color-warning-700)" }}>Pendientes por Entrega</span>
            <span className="stat-value" style={{ color: "var(--color-warning-700)" }}>{totalPendientes}</span>
          </div>
          <div className="stat-icon-wrapper" style={{ background: "var(--color-warning-50)", color: "var(--color-warning-600)" }}>
            <ClockIcon size={20} />
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-info">
            <span className="stat-label" style={{ color: "var(--color-success-700)" }}>Firmadas Digitalmente</span>
            <span className="stat-value" style={{ color: "var(--color-success-700)" }}>{totalFirmadas}</span>
          </div>
          <div className="stat-icon-wrapper" style={{ background: "var(--color-success-50)", color: "var(--color-success-600)" }}>
            <SignatureIcon size={20} />
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-info">
            <span className="stat-label" style={{ color: "var(--color-info-700)" }}>Entregadas en Campo</span>
            <span className="stat-value" style={{ color: "var(--color-info-700)" }}>{totalEntregadas}</span>
          </div>
          <div className="stat-icon-wrapper" style={{ background: "var(--color-info-50)", color: "var(--color-info-600)" }}>
            <PackageIcon size={20} />
          </div>
        </div>
      </div>

      {error && (
        <div className="auth-alert error" style={{ marginBottom: "16px" }}>
          <span>!</span>
          <span>{error}</span>
        </div>
      )}

      {/* Toolbar Search */}
      <div className="toolbar-container" style={{ background: "var(--bg-surface)", padding: "14px 18px", borderRadius: "var(--radius-lg)", border: "1px solid var(--border-default)" }}>
        <div className="search-input-wrapper" style={{ maxWidth: "100%" }}>
          <span className="search-icon">
            <SearchIcon size={16} />
          </span>
          <input
            id="mensajero-search"
            name="search"
            className="form-input"
            type="text"
            placeholder="Buscar por placa, modelo de equipo o número de acta..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      {/* Table */}
      <ActaTable
        actas={filteredActas}
        onSelect={onSelectActa}
        variant="mensajero"
      />
    </div>
  );
}

// Retrocompatibilidad
export { DeliveryPage as CourierPage, DeliveryPage as MensajeroPage };

