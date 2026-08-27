import { useEffect, useState } from "react";
import { ExecutiveSummaryCards } from "../components/ExecutiveSummaryCards";
import { RecentActasCard } from "../components/RecentActasCard";
import {
  getDashboardKpis,
  getRecentActivity,
} from "../services/dashboardService";
import type {
  DashboardKpis,
  RecentActivityItem,
} from "../types/dashboard";

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
      <main className="dashboard-page" style={{ padding: "28px" }}>
        <div className="inventory-state">
          <span className="inventory-loader" />
          <strong>Cargando panel principal</strong>
          <small>Consultando información en tiempo real...</small>
        </div>
      </main>
    );
  }

  return (
    <main className="dashboard-page" style={{ padding: "28px", maxWidth: "1500px", margin: "0 auto" }}>
      {/* HEADER LIMPIO Y EJECUTIVO */}
      <header
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-end",
          marginBottom: "24px",
          flexWrap: "wrap",
          gap: "12px",
        }}
      >
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            PANEL DE CONTROL
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "#0f172a", letterSpacing: "-0.5px" }}>
            Resumen General
          </h1>
          <p style={{ margin: 0, color: "#64748b", fontSize: "13px" }}>
            Visión ejecutiva de inventario de hardware y trazabilidad de actas.
          </p>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <button
            type="button"
            className="actas-reset"
            onClick={() => fetchDashboardData(true)}
            disabled={refreshing}
            style={{ display: "flex", alignItems: "center", gap: "6px", fontSize: "13px", padding: "8px 14px" }}
          >
            <span>{refreshing ? "⟳" : "↻"}</span>
            {refreshing ? "Actualizando..." : "Actualizar"}
          </button>
        </div>
      </header>

      {/* ERROR ALERT */}
      {error && (
        <div className="actas-error" style={{ marginBottom: "20px" }}>
          {error}
        </div>
      )}

      {/* 3 TARJETAS EJECUTIVAS PRINCIPALES */}
      {kpis && <ExecutiveSummaryCards kpis={kpis} onNavigate={onNavigate} />}

      {/* ÚLTIMAS ACTAS EMITIDAS A ANCHO COMPLETO */}
      <section style={{ width: "100%", marginTop: "12px" }}>
        <RecentActasCard
          items={recentActivity}
          onNavigateToActas={() => onNavigate("actas")}
        />
      </section>
    </main>
  );
}
