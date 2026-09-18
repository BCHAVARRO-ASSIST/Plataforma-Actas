import { useEffect, useRef, useState } from "react";
import { PenIcon, TrashIcon, CheckCircleIcon } from "./Icons";

interface FirmaManuscritaProps {
  onFirmaChange: (firma: string | null) => void;
  disabled?: boolean;
}

export function FirmaManuscrita({
  onFirmaChange,
  disabled = false,
}: FirmaManuscritaProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const drawingRef = useRef(false);
  const [hayFirma, setHayFirma] = useState(false);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const context = canvas.getContext("2d");
    if (!context) return;

    // Configurar trazo nítido y fluido
    context.lineWidth = 2.8;
    context.lineCap = "round";
    context.lineJoin = "round";
    context.strokeStyle = "#0f172a";
  }, []);

  function getPosition(event: React.PointerEvent<HTMLCanvasElement>) {
    const canvas = canvasRef.current;
    if (!canvas) return null;

    const rect = canvas.getBoundingClientRect();
    return {
      x: (event.clientX - rect.left) * (canvas.width / rect.width),
      y: (event.clientY - rect.top) * (canvas.height / rect.height),
    };
  }

  function handlePointerDown(event: React.PointerEvent<HTMLCanvasElement>) {
    if (disabled) return;

    const canvas = canvasRef.current;
    const context = canvas?.getContext("2d");
    const position = getPosition(event);

    if (!canvas || !context || !position) return;

    drawingRef.current = true;
    try {
      canvas.setPointerCapture(event.pointerId);
    } catch {
      // Ignorar si el puntero ya está capturado
    }

    context.beginPath();
    context.moveTo(position.x, position.y);
  }

  function handlePointerMove(event: React.PointerEvent<HTMLCanvasElement>) {
    if (disabled || !drawingRef.current) return;

    const canvas = canvasRef.current;
    const context = canvas?.getContext("2d");
    const position = getPosition(event);

    if (!canvas || !context || !position) return;

    context.lineTo(position.x, position.y);
    context.stroke();

    if (!hayFirma) {
      setHayFirma(true);
      onFirmaChange(canvas.toDataURL("image/png"));
    }
  }

  function handlePointerUp() {
    if (!drawingRef.current) return;
    drawingRef.current = false;

    const canvas = canvasRef.current;
    if (!canvas) return;

    onFirmaChange(canvas.toDataURL("image/png"));
  }

  function limpiarFirma() {
    if (disabled) return;

    const canvas = canvasRef.current;
    if (!canvas) return;

    const context = canvas.getContext("2d");
    if (!context) return;

    context.clearRect(0, 0, canvas.width, canvas.height);
    setHayFirma(false);
    onFirmaChange(null);
  }

  return (
    <div className="firma-manuscrita-container">
      <div className="firma-canvas-box">
        {/* Cabecera del Panel de Firma */}
        <div className="firma-canvas-header">
          <span style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
            <PenIcon size={14} style={{ color: "var(--color-primary-600)" }} />
            <span>Panel de Firma Manuscrita Digital</span>
          </span>
          <span style={{ fontSize: "11px", color: "var(--text-muted)", fontWeight: 500 }}>
            ASSIST · Validez Legal
          </span>
        </div>

        {/* Lienzo Interactivo */}
        <canvas
          ref={canvasRef}
          width={800}
          height={260}
          className="firma-canvas"
          style={{
            touchAction: "none",
            cursor: disabled ? "not-allowed" : "crosshair",
          }}
          onPointerDown={handlePointerDown}
          onPointerMove={handlePointerMove}
          onPointerUp={handlePointerUp}
          onPointerCancel={handlePointerUp}
          onPointerLeave={handlePointerUp}
        />

        {/* Línea Guía de Firma */}
        <div className="firma-guideline">
          <span className="firma-guideline-x">X</span>
          <span className="firma-guideline-text">Firme sobre la línea</span>
        </div>
      </div>

      {/* Barra de Estado y Botón de Limpieza */}
      <div className="firma-toolbar">
        <div className="firma-status">
          <span className={`firma-dot ${hayFirma ? "has-signature" : ""}`} />
          <span>
            {hayFirma ? (
              <span style={{ color: "var(--color-success-700)", fontWeight: 600, display: "inline-flex", alignItems: "center", gap: "4px" }}>
                <CheckCircleIcon size={13} />
                Firma manuscrita capturada correctamente
              </span>
            ) : (
              <span style={{ color: "var(--text-secondary)" }}>
                Utilice su dedo, lápiz táctil o ratón dentro del recuadro
              </span>
            )}
          </span>
        </div>

        <button
          type="button"
          className="firma-clear-button"
          onClick={limpiarFirma}
          disabled={disabled || !hayFirma}
          title="Borrar trazo actual"
        >
          <TrashIcon size={13} />
          <span>Limpiar trazo</span>
        </button>
      </div>
    </div>
  );
}

