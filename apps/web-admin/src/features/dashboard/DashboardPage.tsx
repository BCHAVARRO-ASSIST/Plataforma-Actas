import { useEffect, useState } from "react";
import { ExecutiveSummaryCards } from "../../components/ExecutiveSummaryCards";
import { RecentActasCard } from "../../components/RecentActasCard";
import { RefreshIcon } from "../../components/Icons";
import {
  getDashboardKpis,
  getRecentActivity,
} from "./dashboardService";
import type {
  DashboardKpis,
  RecentActivityItem,
} from "./dashboardService";

interface DashboardPageProps {
  onNavigate?: (view: "actas" | "equipment" | "reportes") => void;
}

export function DashboardPage({ onNavigate = () => {} }: DashboardPageProps) {
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [kpis, setKpis] = useState<DashboardKpis | null>(null);
  const [recentActivity, setRecentActivity] = useState<RecentActivityItem[]>([]);

  const fetchDashboardData = async (isRefresh = false) => {
    try {
      if (isRefresh) setRefreshing(true);
      else setLoading(true);
      setError(null);

      const [kpisRes, activityRes] = await Promise.allSettled([
        getDashboardKpis(),
        getRecentActivity(10),
      ]);

      if (kpisRes.status === "fulfilled") setKpis(kpisRes.value);
      if (activityRes.status === "fulfilled") setRecentActivity(activityRes.value);

      if (kpisRes.status === "rejected") {
        setError("No fue posible conectar con el servidor para obtener los indicadores.");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el dashboard.");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    let mounted = true;
    Promise.allSettled([
      getDashboardKpis(),
      getRecentActivity(10),
    ]).then(([kpisRes, actRes]) => {
      if (!mounted) return;
      if (kpisRes.status === "fulfilled") setKpis(kpisRes.value);
      if (actRes.status === "fulfilled") setRecentActivity(actRes.value);
      setLoading(false);
    }).catch((err) => {
      if (!mounted) return;
      setError(err instanceof Error ? err.message : "Error al cargar el dashboard.");
      setLoading(false);
    });

    return () => {
      mounted = false;
    };
  }, []);

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando panel de control...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "12px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            PANEL DE CONTROL
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Resumen Operativo
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Visión global del estado del inventario, actas y trazabilidad documental.
          </p>
        </div>

        <button
          type="button"
          onClick={() => fetchDashboardData(true)}
          disabled={refreshing}
          className="btn btn-secondary btn-sm"
          style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}
        >
          <RefreshIcon size={14} className={refreshing ? "spin-animation" : ""} />
          <span>{refreshing ? "Actualizando..." : "Actualizar métricas"}</span>
        </button>
      </div>

      {error && (
        <div className="auth-alert error" style={{ marginBottom: "20px" }}>
          <span>!</span>
          <span>{error}</span>
        </div>
      )}

      {/* KPI Cards */}
      {kpis ? (
        <ExecutiveSummaryCards kpis={kpis} onNavigate={onNavigate} />
      ) : (
        <div className="card" style={{ padding: "30px", textAlign: "center", color: "var(--text-muted)" }}>
          No hay indicadores disponibles.
        </div>
      )}

      {/* Recent Actas */}
      <RecentActasCard
        items={recentActivity}
        onNavigateToActas={() => onNavigate("actas")}
      />
    </div>
  );
}
