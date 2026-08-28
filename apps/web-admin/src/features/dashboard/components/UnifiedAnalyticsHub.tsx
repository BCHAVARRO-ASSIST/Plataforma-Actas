import { useState } from "react";
import { EquipmentStatusChart } from "./EquipmentStatusChart";
import { ActasStatusChart } from "./ActasStatusChart";
import { ActasTimelineChart } from "./ActasTimelineChart";
import { DeliveriesChart } from "./DeliveriesChart";
import { MaintenanceChart } from "./MaintenanceChart";
import type {
  StatusCountItem,
  MessengerDeliveryItem,
  TimelinePointItem,
  MaintenanceStats,
} from "../dashboardService";

interface UnifiedAnalyticsHubProps {
  equipmentStatus: StatusCountItem[];
  actasStatus: StatusCountItem[];
  deliveries: MessengerDeliveryItem[];
  timeline: TimelinePointItem[];
  timelineRange: string;
  onTimelineRangeChange: (range: string) => void;
  maintenance: MaintenanceStats | null;
}

type TabType = "inventario" | "actas" | "tendencia" | "logistica" | "mantenimiento";

export function UnifiedAnalyticsHub({
  equipmentStatus,
  actasStatus,
  deliveries,
  timeline,
  timelineRange,
  onTimelineRangeChange,
  maintenance,
}: UnifiedAnalyticsHubProps) {
  const [activeTab, setActiveTab] = useState<TabType>("inventario");

  const tabs: { id: TabType; label: string; icon: string }[] = [
    { id: "inventario", label: "Inventario de Equipos", icon: "ðŸ’»" },
    { id: "actas", label: "Estado de Actas", icon: "ðŸ“‹" },
    { id: "tendencia", label: "Evolución Temporal", icon: "ðŸ“ˆ" },
    { id: "logistica", label: "Rendimiento Logístico", icon: "ðŸšš" },
    { id: "mantenimiento", label: "Soporte y Mantenimiento", icon: "⚙" },
  ];

  return (
    <div
      style={{
        background: "#ffffff",
        borderRadius: "14px",
        border: "1px solid #e2e8f0",
        boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
      }}
    >
      {/* HEADER DE PESTAÑAS UNIFICADO */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "4px",
          padding: "8px 12px",
          background: "#f8fafc",
          borderBottom: "1px solid #e2e8f0",
          overflowX: "auto",
        }}
      >
        {tabs.map((tab) => {
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              type="button"
              onClick={() => setActiveTab(tab.id)}
              style={{
                padding: "8px 14px",
                border: "none",
                borderRadius: "8px",
                background: isActive ? "#0f172a" : "transparent",
                color: isActive ? "#ffffff" : "#475569",
                fontSize: "13px",
                fontWeight: isActive ? 700 : 500,
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                gap: "6px",
                whiteSpace: "nowrap",
                transition: "all 0.15s ease",
              }}
            >
              <span>{tab.icon}</span>
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* CONTENIDO DEL MÓDULO UNIFICADO */}
      <div style={{ padding: "16px" }}>
        {activeTab === "inventario" && <EquipmentStatusChart data={equipmentStatus} />}
        {activeTab === "actas" && <ActasStatusChart data={actasStatus} />}
        {activeTab === "tendencia" && (
          <ActasTimelineChart
            data={timeline}
            selectedRange={timelineRange}
            onRangeChange={onTimelineRangeChange}
          />
        )}
        {activeTab === "logistica" && <DeliveriesChart data={deliveries} />}
        {activeTab === "mantenimiento" && <MaintenanceChart stats={maintenance} />}
      </div>
    </div>
  );
}

