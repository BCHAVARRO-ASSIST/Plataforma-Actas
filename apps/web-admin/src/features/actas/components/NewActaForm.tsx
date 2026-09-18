import { useEffect, useState } from "react";
import { createActa, getActas } from "@/features/actas/actaService";
import type { Acta } from "@/features/actas/actaService";
import { getEquipos, type Equipo } from "@/features/equipos/equipoService";
import { getUsuarios, type Usuario } from "@/features/usuarios/usuarioService";
import { useAuth } from "@/features/auth/useAuth";
import { ActaTypeSelector } from "./ActaTypeSelector";
import { ActaLivePreview } from "./ActaLivePreview";
import {
  ArrowLeftIcon,
  EquipmentIcon,
  MessengerIcon,
  UserIcon,
} from "@/components/ui/Icons";

interface NewActaFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

export function NewActaForm({ onCreated, onCancel }: NewActaFormProps) {
  const { user } = useAuth();
  const [equipment, setEquipment] = useState<Equipo[]>([]);
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [actasEntrega, setActasEntrega] = useState<Acta[]>([]);

  const [equipmentId, setEquipmentId] = useState("");
  const [actaType, setActaType] = useState<"Entrega" | "Devolución">("Entrega");
  const [actaOrigenId, setActaOrigenId] = useState<string>("");
  const [destinatarioUsuarioId, setDestinatarioUsuarioId] = useState<number | "">("");
  const [mensajeroUsuarioId, setMensajeroUsuarioId] = useState<number | "">("");

  const [loadingEquipment, setLoadingEquipment] = useState(true);
  const [loadingUsuarios, setLoadingUsuarios] = useState(true);
  const [loadingActas, setLoadingActas] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadInitialData() {
      try {
        setLoadingEquipment(true);
        setLoadingUsuarios(true);
        setLoadingActas(true);
        setError(null);

        const [equipmentData, usuariosData, actasData] = await Promise.all([
          getEquipos(),
          getUsuarios(),
          getActas().catch(() => [] as Acta[]),
        ]);

        setEquipment(equipmentData);
        setUsuarios(usuariosData);
        setActasEntrega(
          actasData.filter((a: Acta) => {
            const tipo = a.tipoActa?.trim().toLowerCase();
            const estado = a.estado?.trim().toLowerCase();
            const esEntregaValida =
              tipo === "entrega" && (estado === "firmada" || estado === "finalizada");

            const yaDevuelta = actasData.some(
              (d: Acta) =>
                d.tipoActa?.trim().toLowerCase() === "devolución" &&
                d.actaOrigenId === a.id &&
                d.estado?.trim().toLowerCase() !== "anulada"
            );

            return esEntregaValida && !yaDevuelta;
          })
        );
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "No fue posible cargar la información inicial."
        );
      } finally {
        setLoadingEquipment(false);
        setLoadingUsuarios(false);
        setLoadingActas(false);
      }
    }

    loadInitialData();
  }, []);

  function handleActaOrigenChange(selectedId: string) {
    setActaOrigenId(selectedId);

    if (!selectedId) {
      return;
    }

    const origen = actasEntrega.find((a: Acta) => a.id === selectedId);
    if (origen) {
      setEquipmentId(origen.equipmentId);
      if (origen.destinatarioUsuarioId) {
        setDestinatarioUsuarioId(origen.destinatarioUsuarioId);
      } else {
        const targetEq = equipment.find((e) => e.id === origen.equipmentId);
        if (targetEq?.asignadoA) {
          const match = usuarios.find((u) => {
            const fullName = `${u.nombre} ${u.apellido}`.toLowerCase().trim();
            const asignado = targetEq.asignadoA!.toLowerCase().trim();
            return fullName.includes(asignado) || asignado.includes(fullName);
          });
          if (match) {
            setDestinatarioUsuarioId(match.id);
          }
        }
      }

      if (origen.mensajeroUsuarioId) {
        setMensajeroUsuarioId(origen.mensajeroUsuarioId);
      }
    }
  }

  const isValid =
    Boolean(equipmentId) &&
    Boolean(destinatarioUsuarioId) &&
    (actaType === "Entrega" || Boolean(actaOrigenId));

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (actaType === "Devolución" && !actaOrigenId) {
      setError("Seleccione el acta de entrega de origen para registrar la Devolución.");
      return;
    }

    if (!equipmentId) {
      setError("Seleccione un equipo tecnológico.");
      return;
    }

    if (!destinatarioUsuarioId) {
      setError("Seleccione un destinatario / consultor.");
      return;
    }

    try {
      setSaving(true);
      setError(null);

      const formData = {
        tipo: actaType,
        observacion: "",
      };

      const currentUserId = user?.usuarioId ? parseInt(user.usuarioId, 10) : 0;

      await createActa({
        equipmentId,
        usuarioId: currentUserId > 0 ? currentUserId : 1,
        destinatarioUsuarioId,
        mensajeroUsuarioId: mensajeroUsuarioId === "" ? null : mensajeroUsuarioId,
        actaEntregada: false,
        deviceId: "WEB-ADMIN",
        formDataJson: JSON.stringify(formData),
        tipoActa: actaType,
        actaOrigenId: actaType === "Devolución" ? actaOrigenId : null,
      });

      onCreated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "No fue posible crear el acta.");
    } finally {
      setSaving(false);
    }
  }

  const selectedEquipment = equipment.find((item) => item.id === equipmentId);
  const selectedDestinatario = usuarios.find((u) => u.id === destinatarioUsuarioId);
  const selectedMensajero = usuarios.find((u) => u.id === mensajeroUsuarioId);
  const isLoading = loadingEquipment || loadingUsuarios || loadingActas;

  return (
    <div style={{ maxWidth: "1100px", margin: "0 auto" }}>
      {/* Botón Volver y Encabezado Limpio */}
      <div style={{ marginBottom: "22px" }}>
        <button
          type="button"
          onClick={onCancel}
          disabled={saving}
          className="btn btn-ghost btn-sm"
          style={{ display: "inline-flex", alignItems: "center", gap: "6px", color: "var(--text-secondary)", marginBottom: "8px" }}
        >
          <ArrowLeftIcon size={14} />
          <span>Volver al listado de actas</span>
        </button>

        <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--color-primary-600)", textTransform: "uppercase", letterSpacing: "0.6px", display: "block" }}>
          GESTIÓN DOCUMENTAL Y CUSTODIA
        </span>
        <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
          Crear Nueva Acta Oficial
        </h1>
        <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
          Completa los datos del activo y los responsables para generar el documento oficial.
        </p>
      </div>

      {/* Alerta de Error */}
      {error && (
        <div className="auth-alert error" style={{ marginBottom: "20px" }}>
          <span>!</span>
          <span>{error}</span>
        </div>
      )}

      {/* Formulario Principal en Cuadrícula Balanceada */}
      <form onSubmit={handleSubmit}>
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 340px",
            gap: "20px",
            alignItems: "start",
          }}
        >
          {/* Columna Izquierda: Formulario Unificado y Estructurado */}
          <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
            {/* Paso 1: Selector de Tipo */}
            <ActaTypeSelector
              actaType={actaType}
              onTypeChange={(type) => {
                setActaType(type);
                if (type === "Entrega") {
                  setActaOrigenId("");
                }
              }}
              actaOrigenId={actaOrigenId}
              onActaOrigenChange={handleActaOrigenChange}
              actasEntrega={actasEntrega}
              disabled={saving}
            />

            {/* Paso 2: Activo y Responsables */}
            <div className="card" style={{ padding: "22px" }}>
              <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "18px" }}>
                <div
                  style={{
                    width: "28px",
                    height: "28px",
                    borderRadius: "50%",
                    background: "var(--color-primary-100)",
                    color: "var(--color-primary-700)",
                    fontWeight: 800,
                    fontSize: "12px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  2
                </div>
                <div>
                  <h2 style={{ margin: 0, fontSize: "16px", fontWeight: 700, color: "var(--text-primary)" }}>
                    Activo y Personas Asignadas
                  </h2>
                  <p style={{ margin: 0, fontSize: "12px", color: "var(--text-muted)" }}>
                    Selecciona el equipo a custodiar y los usuarios responsables.
                  </p>
                </div>
              </div>

              <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
                {/* Selector de Equipo */}
                <div className="form-group">
                  <label className="form-label" htmlFor="acta-equipment" style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                    <EquipmentIcon size={14} style={{ color: "var(--color-primary-600)" }} />
                    <span>Equipo Tecnológico *</span>
                  </label>
                  <select
                    id="acta-equipment"
                    name="equipmentId"
                    className="form-select"
                    value={equipmentId}
                    onChange={(event) => setEquipmentId(event.target.value)}
                    disabled={isLoading || saving || Boolean(actaOrigenId && actaType === "Devolución")}
                    required
                  >
                    <option value="">
                      {loadingEquipment ? "Cargando catálogo de equipos..." : "Selecciona un equipo..."}
                    </option>
                    {equipment.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.inventoryKey} — {item.nombreEquipo} {item.marca ? `(${item.marca})` : ""} · {item.estatusEquipo ?? "Activo"}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Destinatario y Mensajero en 2 columnas */}
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "14px" }}>
                  {/* Destinatario */}
                  <div className="form-group">
                    <label className="form-label" htmlFor="acta-destinatario" style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                      <UserIcon size={14} style={{ color: "var(--color-primary-600)" }} />
                      <span>Destinatario / Consultor *</span>
                    </label>
                    <select
                      id="acta-destinatario"
                      name="destinatarioUsuarioId"
                      className="form-select"
                      value={destinatarioUsuarioId}
                      onChange={(event) => setDestinatarioUsuarioId(event.target.value ? Number(event.target.value) : "")}
                      disabled={loadingUsuarios || saving || Boolean(actaOrigenId && actaType === "Devolución" && destinatarioUsuarioId)}
                      required
                    >
                      <option value="">
                        {loadingUsuarios ? "Cargando directorio..." : "Seleccione usuario..."}
                      </option>
                      {usuarios.map((usuario) => (
                        <option key={usuario.id} value={usuario.id}>
                          {usuario.nombre} {usuario.apellido} ({usuario.usuarioRed})
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Mensajero */}
                  <div className="form-group">
                    <label className="form-label" htmlFor="acta-mensajero" style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                      <MessengerIcon size={14} style={{ color: "var(--color-primary-600)" }} />
                      <span>Mensajero / Entrega (Opcional)</span>
                    </label>
                    <select
                      id="acta-mensajero"
                      name="mensajeroUsuarioId"
                      className="form-select"
                      value={mensajeroUsuarioId}
                      onChange={(event) => setMensajeroUsuarioId(event.target.value ? Number(event.target.value) : "")}
                      disabled={loadingUsuarios || saving}
                    >
                      <option value="">Sin mensajero (Entrega en oficina)</option>
                      {usuarios.map((usuario) => (
                        <option key={usuario.id} value={usuario.id}>
                          {usuario.nombre} {usuario.apellido} ({usuario.usuarioRed})
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Columna Derecha: Tarjeta de Resumen y Acciones Integradas */}
          <ActaLivePreview
            actaType={actaType}
            actaOrigenId={actaOrigenId}
            selectedEquipment={selectedEquipment}
            selectedDestinatario={selectedDestinatario}
            selectedMensajero={selectedMensajero}
            isValid={isValid}
            saving={saving}
            onCancel={onCancel}
          />
        </div>
      </form>
    </div>
  );
}
