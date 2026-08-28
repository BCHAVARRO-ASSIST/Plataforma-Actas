import { useState, useMemo } from "react";
import type { StatusCountItem } from "../dashboardService";

interface EquipmentStatusChartProps {
  data: StatusCountItem[];
}

export function EquipmentStatusChart({ data }: EquipmentStatusChartProps) {
  const [hovered, setHovered] = useState<StatusCountItem | null>(null);

  const total = useMemo(() => data.reduce((acc, curr) => acc + curr.cantidad, 0), [data]);

  const radius = 70;
  const strokeWidth = 28;
  const circumference = 2 * Math.PI * radius;

  const slices = useMemo(() => {
    return data.map((item, idx) => {
      const priorSum = data.slice(0, idx).reduce((sum, p) => sum + p.porcentaje, 0);
      const strokeDashoffset = -((priorSum / 100) * circumference);
      const strokeDasharray = `${(item.porcentaje / 100) * circumference} ${circumference}`;

      return {
        ...item,
        strokeDasharray,
        strokeDashoffset,
      };
    });
  }, [data, circumference]);

  if (data.length === 0 || total === 0) {
    return (
      <div className="dashboard-block" style={{ padding: "20px" }}>
        <div className="dashboard-block-top">
          <div>
            <span className="block-kicker">INVENTARIO</span>
            <h3>Distribución de Equipos</h3>
          </div>
        </div>
        <div style={{ padding: "40px 20px", textAlign: "center", color: "#64748b" }}>
          No existen equipos registrados para mostrar.
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-block" style={{ padding: "20px" }}>
      <div className="dashboard-block-top" style={{ marginBottom: "16px" }}>
        <div>
          <span className="block-kicker">INVENTARIO DE HARDWARE</span>
          <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            Distribución por Estado
          </h3>
        </div>
        <span style={{ fontSize: "12px", color: "#64748b", fontWeight: 600 }}>
          {total} equipos
        </span>
      </div>

      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-around", flexWrap: "wrap", gap: "20px" }}>
        {/* SVG DONUT */}
        <div style={{ position: "relative", width: "180px", height: "180px" }}>
          <svg viewBox="0 0 200 200" width="100%" height="100%" style={{ transform: "rotate(-90deg)" }}>
            <circle
              cx="100"
              cy="100"
              r={radius}
              fill="transparent"
              stroke="#f1f5f9"
              strokeWidth={strokeWidth}
            />
            {slices.map((slice, idx) => (
              <circle
                key={idx}
                cx="100"
                cy="100"
                r={radius}
                fill="transparent"
                stroke={slice.color || "#3b82f6"}
                strokeWidth={strokeWidth}
                strokeDasharray={slice.strokeDasharray}
                strokeDashoffset={slice.strokeDashoffset}
                style={{
                  transition: "stroke-width 0.2s ease, opacity 0.2s ease",
                  cursor: "pointer",
                  opacity: hovered && hovered.estado !== slice.estado ? 0.45 : 1,
                }}
                onMouseEnter={() => setHovered(slice)}
                onMouseLeave={() => setHovered(null)}
              />
            ))}
          </svg>

          {/* CENTER TEXT */}
          <div
            style={{
              position: "absolute",
              top: "50%",
              left: "50%",
              transform: "translate(-50%, -50%)",
              textAlign: "center",
              pointerEvents: "none",
            }}
          >
            <strong style={{ display: "block", fontSize: "20px", fontWeight: 800, color: "#0f172a" }}>
              {hovered ? hovered.cantidad : total}
            </strong>
            <span style={{ fontSize: "11px", color: "#64748b", fontWeight: 600 }}>
              {hovered ? hovered.estado : "Total"}
            </span>
          </div>
        </div>

        {/* LEGEND */}
        <div style={{ display: "flex", flexDirection: "column", gap: "10px", minWidth: "150px" }}>
          {data.map((item, idx) => (
            <div
              key={idx}
              onMouseEnter={() => setHovered(item)}
              onMouseLeave={() => setHovered(null)}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                gap: "12px",
                padding: "6px 10px",
                borderRadius: "8px",
                background: hovered?.estado === item.estado ? "#f8fafc" : "transparent",
                cursor: "pointer",
                transition: "background 0.15s ease",
              }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                <span
                  style={{
                    width: "10px",
                    height: "10px",
                    borderRadius: "50%",
                    backgroundColor: item.color || "#6b7280",
                  }}
                />
                <span style={{ fontSize: "13px", fontWeight: 600, color: "#334155" }}>
                  {item.estado}
                </span>
              </div>
              <div style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                <strong style={{ fontSize: "13px", color: "#0f172a" }}>{item.cantidad}</strong>
                <small style={{ fontSize: "11px", color: "#64748b" }}>({item.porcentaje}%)</small>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

