import {
  useEffect,
  useRef,
  useState,
} from "react";


// =====================================================
// PROPS
// =====================================================

interface FirmaManuscritaProps {
  onFirmaChange: (firma: string | null) => void;
  disabled?: boolean;
}


// =====================================================
// COMPONENTE
// =====================================================

export function FirmaManuscrita({
  onFirmaChange,
  disabled = false,
}: FirmaManuscritaProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const drawingRef = useRef(false);
  const [hayFirma, setHayFirma] = useState(false);

  // ===================================================
  // CONFIGURAR CANVAS
  // ===================================================

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const context = canvas.getContext("2d");
    if (!context) return;

    // Configurar resolución de trazo nítida
    context.lineWidth = 2.5;
    context.lineCap = "round";
    context.lineJoin = "round";
    context.strokeStyle = "#0f172a";
  }, []);

  // ===================================================
  // OBTENER POSICIÓN
  // ===================================================

  function getPosition(event: React.PointerEvent<HTMLCanvasElement>) {
    const canvas = canvasRef.current;
    if (!canvas) return null;

    const rect = canvas.getBoundingClientRect();
    return {
      x: (event.clientX - rect.left) * (canvas.width / rect.width),
      y: (event.clientY - rect.top) * (canvas.height / rect.height),
    };
  }

  // ===================================================
  // INICIAR FIRMA
  // ===================================================

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

  // ===================================================
  // DIBUJAR
  // ===================================================

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

  // ===================================================
  // FINALIZAR FIRMA
  // ===================================================

  function handlePointerUp() {
    if (!drawingRef.current) return;
    drawingRef.current = false;

    const canvas = canvasRef.current;
    if (!canvas) return;

    onFirmaChange(canvas.toDataURL("image/png"));
  }

  // ===================================================
  // LIMPIAR
  // ===================================================

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
        <canvas
          ref={canvasRef}
          width={800}
          height={280}
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
        <div className="firma-guideline">
          <span>Firme sobre la línea</span>
        </div>
      </div>

      <div className="firma-toolbar">
        <div className="firma-status">
          <span className={`firma-dot ${hayFirma ? "has-signature" : ""}`} />
          <span>
            {hayFirma
              ? "✓ Firma manuscrita capturada"
              : "Utilice su dedo o lápiz táctil dentro del recuadro"}
          </span>
        </div>

        <button
          type="button"
          className="firma-clear-button"
          onClick={limpiarFirma}
          disabled={disabled || !hayFirma}
        >
          Borrar / Limpiar
        </button>
      </div>
    </div>
  );
}
