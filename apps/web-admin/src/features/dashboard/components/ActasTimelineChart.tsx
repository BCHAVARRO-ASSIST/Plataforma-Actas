import { useState } from "react";
import type { TimelinePointItem } from "../dashboardService";

interface ActasTimelineChartProps {
  data: TimelinePointItem[];
  selectedRange: string;
  onRangeChange: (range: string) => void;
}

export function ActasTimelineChart({
  data,
  selectedRange,
  onRangeChange,
}: ActasTimelineChartProps) {
  const [hoveredPoint, setHoveredPoint] = useState<TimelinePointItem | null>(null);

  if (data.length === 0) {
    return (
      <div className="dashboard-block" style={{ padding: "20px" }}>
        <div className="dashboard-block-top">
          <div>
            <span className="block-kicker">TENDENCIA HISTÃ“RICA</span>
            <h3>Actas en el Tiempo</h3>
          </div>
          <div style={{ display: "flex", gap: "6px" }}>
            {["7d", "30d", "90d"].map((r) => (
              <button
                key={r}
                type="button"
                onClick={() => onRangeChange(r)}
                style={{
                  padding: "4px 10px",
                  borderRadius: "6px",
                  fontSize: "12px",
                  fontWeight: 600,
                  border: "1px solid #cbd5e1",
                  background: selectedRange === r ? "#0f172a" : "#ffffff",
                  color: selectedRange === r ? "#ffffff" : "#475569",
                  cursor: "pointer",
                }}
              >
                {r === "7d" ? "7 dÃ­as" : r === "30d" ? "30 dÃ­as" : "90 dÃ­as"}
              </button>
            ))}
          </div>
        </div>
        <div style={{ padding: "40px 20px", textAlign: "center", color: "#64748b" }}>
          No existen actas registradas en el perÃ­odo seleccionado.
        </div>
      </div>
    );
  }

  const maxVal = Math.max(...data.map((d) => Math.max(d.totalCreadas, d.totalFirmadas)), 5);
  const chartHeight = 160;

  return (
    <div className="dashboard-block" style={{ padding: "20px" }}>
      <div className="dashboard-block-top" style={{ marginBottom: "20px" }}>
        <div>
          <span className="block-kicker">TENDENCIA HISTÃ“RICA</span>
          <h3 style={{ margin: "4px 0 0", fontSize: "16px", fontWeight: 700, color: "#0f172a" }}>
            EvoluciÃ³n Temporal de Actas
          </h3>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: "14px" }}>
          <div style={{ display: "flex", gap: "10px", fontSize: "12px", color: "#64748b" }}>
            <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
              <span style={{ width: "8px", height: "8px", borderRadius: "2px", background: "#3b82f6" }} /> Creadas
            </span>
            <span style={{ display: "flex", alignItems: "center", gap: "4px" }}>
              <span style={{ width: "8px", height: "8px", borderRadius: "2px", background: "#10b981" }} /> Firmadas
            </span>
          </div>

          <div style={{ display: "flex", gap: "4px" }}>
            {[
              { id: "7d", label: "7 dÃ­as" },
              { id: "30d", label: "30 dÃ­as" },
              { id: "90d", label: "90 dÃ­as" },
            ].map((r) => (
              <button
                key={r.id}
                type="button"
                onClick={() => onRangeChange(r.id)}
                style={{
                  padding: "4px 10px",
                  borderRadius: "6px",
                  fontSize: "12px",
                  fontWeight: 600,
                  border: "1px solid #cbd5e1",
                  background: selectedRange === r.id ? "#0f172a" : "#ffffff",
                  color: selectedRange === r.id ? "#ffffff" : "#475569",
                  cursor: "pointer",
                }}
              >
                {r.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* SVG BAR / AREA CHART */}
      <div style={{ position: "relative", width: "100%", height: `${chartHeight + 40}px` }}>
        <svg viewBox={`0 0 ${data.length * 36} ${chartHeight + 30}`} width="100%" height="100%">
          {/* Horizontal grid lines */}
          {[0, 0.25, 0.5, 0.75, 1].map((pct, i) => (
            <line
              key={i}
              x1="0"
              y1={chartHeight * (1 - pct)}
              x2={data.length * 36}
              y2={chartHeight * (1 - pct)}
              stroke="#f1f5f9"
              strokeDasharray="4 4"
            />
          ))}

          {/* Bars */}
          {data.map((point, idx) => {
            const x = idx * 36 + 10;
            const hCreadas = (point.totalCreadas / maxVal) * chartHeight;
            const hFirmadas = (point.totalFirmadas / maxVal) * chartHeight;

            return (
              <g
                key={idx}
                onMouseEnter={() => setHoveredPoint(point)}
                onMouseLeave={() => setHoveredPoint(null)}
                style={{ cursor: "pointer" }}
              >
                {/* Bar creadas */}
                <rect
                  x={x}
                  y={chartHeight - hCreadas}
                  width={9}
                  height={Math.max(hCreadas, 2)}
                  fill="#3b82f6"
                  rx="3"
                  opacity={hoveredPoint?.fecha === point.fecha ? 1 : 0.85}
                />
                {/* Bar firmadas */}
                <rect
                  x={x + 11}
                  y={chartHeight - hFirmadas}
                  width={9}
                  height={Math.max(hFirmadas, 2)}
                  fill="#10b981"
                  rx="3"
                  opacity={hoveredPoint?.fecha === point.fecha ? 1 : 0.85}
                />

                {/* X axis labels (sparse) */}
                {(idx % Math.ceil(data.length / 7) === 0 || idx === data.length - 1) && (
                  <text
                    x={x + 10}
                    y={chartHeight + 18}
                    textAnchor="middle"
                    fontSize="10"
                    fill="#94a3b8"
                  >
                    {point.fecha.slice(5)}
                  </text>
                )}
              </g>
            );
          })}
        </svg>

        {/* HOVER TOOLTIP */}
        {hoveredPoint && (
          <div
            style={{
              position: "absolute",
              top: "-10px",
              right: "10px",
              background: "#0f172a",
              color: "#ffffff",
              padding: "6px 12px",
              borderRadius: "8px",
              fontSize: "12px",
              boxShadow: "0 4px 12px rgba(0,0,0,0.15)",
              display: "flex",
              gap: "10px",
            }}
          >
            <span>ðŸ“… {hoveredPoint.fecha}</span>
            <span><strong>{hoveredPoint.totalCreadas}</strong> creadas</span>
            <span><strong>{hoveredPoint.totalFirmadas}</strong> firmadas</span>
          </div>
        )}
      </div>
    </div>
  );
}

