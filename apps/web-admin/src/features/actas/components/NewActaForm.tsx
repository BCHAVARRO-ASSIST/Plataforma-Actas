import { useEffect, useState } from "react";

import { createActa, getActas } from "../actaService";
import type { Acta } from "../actaService";

import { getEquipment, type Equipment } from "../../equipment/equipmentService";


import { getUsuarios, type Usuario } from "../../usuarios/usuarioService";


interface NewActaFormProps {
  onCreated: () => void;
  onCancel: () => void;
}

export function NewActaForm({
  onCreated,
  onCancel,
}: NewActaFormProps) {
  const [equipment, setEquipment] = useState<Equipment[]>([]);
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [actasEntrega, setActasEntrega] = useState<Acta[]>([]);

  const [equipmentId, setEquipmentId] = useState("");
  const [actaType, setActaType] =
    useState<"Entrega" | "Devolución">("Entrega");
  const [actaOrigenId, setActaOrigenId] = useState<string>("");

  const [destinatarioUsuarioId, setDestinatarioUsuarioId] =
    useState<number | "">("");

  const [mensajeroUsuarioId, setMensajeroUsuarioId] =
    useState<number | "">("");

  const [observacion, setObservacion] = useState("");

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
        setError(null);

        const [equipmentData, usuariosData, actasData] =
          await Promise.all([
            getEquipment(),
            getUsuarios(),
            getActas().catch(() => [] as Acta[]),
          ]);

        setEquipment(equipmentData);
        setUsuarios(usuariosData);
        setActasEntrega(
          actasData.filter(
            (a: Acta) => a.tipoActa === "Entrega" && a.estado === "Firmada"
          )
        );
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "No fue posible cargar la información."
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
      }
      if (origen.mensajeroUsuarioId) {
        setMensajeroUsuarioId(origen.mensajeroUsuarioId);
      }
    }
  }

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    if (actaType === "Devolución" && !actaOrigenId) {
      setError(
        "Seleccione el acta de entrega de origen para registrar la Devolución."
      );
      return;
    }

    if (!equipmentId) {
      setError("Seleccione un equipo.");
      return;
    }

    if (!destinatarioUsuarioId) {
      setError("Seleccione un destinatario.");
      return;
    }

    try {
      setSaving(true);
      setError(null);

      const formData = {
        tipo: actaType,
        observacion,
      };

      await createActa({
        equipmentId,
        usuarioId: 0, // El backend asigna el usuario autenticado del token JWT
        destinatarioUsuarioId,
        mensajeroUsuarioId:
          mensajeroUsuarioId === ""
            ? null
            : mensajeroUsuarioId,
        actaEntregada: false,
        deviceId: "WEB-ADMIN",
        formDataJson: JSON.stringify(formData),
        tipoActa: actaType,
        actaOrigenId: actaType === "Devolución" ? actaOrigenId : null,
      });

      onCreated();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "No fue posible crear el acta."
      );
    } finally {
      setSaving(false);
    }
  }

  const selectedEquipment = equipment.find(
    (item) => item.id === equipmentId
  );

  const selectedDestinatario = usuarios.find(
    (usuario) =>
      usuario.id === destinatarioUsuarioId
  );

  const selectedMensajero = usuarios.find(
    (usuario) =>
      usuario.id === mensajeroUsuarioId
  );

  const isLoading =
    loadingEquipment ||
    loadingUsuarios ||
    loadingActas;

  return (
    <section className="new-acta-page">
      <div className="new-acta-topbar">
        <button
          type="button"
          className="new-acta-back"
          onClick={onCancel}
          disabled={saving}
        >
          â† Volver a actas
        </button>
      </div>

      <header className="new-acta-hero">
        <div>
          <span className="actas-kicker">
            GestiÃ³n documental
          </span>

          <h1>Nueva acta</h1>

          <p>
            Crea un nuevo documento de entrega
            o Devolución de equipo.
          </p>
        </div>

        <div className="new-acta-step">
          <span>01</span>

          <div>
            <strong>
              información inicial
            </strong>

            <small>
              Borrador
            </small>
          </div>
        </div>
      </header>

      {error && (
        <div
          className="new-acta-error"
          role="alert"
        >
          <strong>
            No fue posible continuar
          </strong>

          <span>{error}</span>
        </div>
      )}

      <form
        className="new-acta-layout"
        onSubmit={handleSubmit}
      >
        <div className="new-acta-main">
          {/* INFORMACIÃ“N DEL DOCUMENTO */}

          <section className="new-acta-card">
            <div className="new-acta-card-heading">
              <div className="new-acta-number">
                01
              </div>

              <div>
                <h2>
                  información del documento
                </h2>

                <p>
                  Define el tipo de acta y las
                  personas relacionadas.
                </p>
              </div>
            </div>

            <div className="new-acta-fields">
              {/* TIPO DE ACTA */}

              <div className="new-acta-field">
                <label htmlFor="acta-type">
                  Tipo de acta
                  <span>Requerido</span>
                </label>

                <select
                  id="acta-type"
                  value={actaType}
                  onChange={(event) => {
                    const newType = event.target.value as "Entrega" | "Devolución";
                    setActaType(newType);
                    if (newType === "Entrega") {
                      setActaOrigenId("");
                    }
                  }}
                  disabled={saving}
                >
                  <option value="Entrega">Entrega</option>
                  <option value="Devolución">Devolución</option>
                </select>

                <small>
                  {actaType === "Devolución"
                    ? "Selecciona el acta de entrega firmada de origen."
                    : "Entrega inicial de equipo a usuario."}
                </small>
              </div>

              {/* ACTA ORIGEN (SI ES DEVOLUCIÃ“N) */}

              {actaType === "Devolución" && (
                <div className="new-acta-field new-acta-field-wide">
                  <label htmlFor="acta-origen">
                    Acta de entrega de origen
                    <span>Requerido</span>
                  </label>

                  <select
                    id="acta-origen"
                    value={actaOrigenId}
                    onChange={(e) => handleActaOrigenChange(e.target.value)}
                    disabled={saving}
                    required
                  >
                    <option value="">
                      {actasEntrega.length === 0
                        ? "No hay actas de entrega firmadas disponibles"
                        : "Seleccione el acta de entrega previa"}
                    </option>
                    {actasEntrega.map((a: Acta) => (
                      <option key={a.id} value={a.id}>
                        {a.numeroActa} â€” {a.inventoryKey ?? a.equipmentId} ({a.nombreEquipo ?? "Equipo"})
                      </option>
                    ))}
                  </select>

                  <small>
                    El equipo y destinatario se asociarÃ¡n automÃ¡ticamente segÃºn el acta seleccionada.
                  </small>
                </div>
              )}

              {/* EQUIPO */}

              <div className="new-acta-field new-acta-field-wide">
                <label htmlFor="acta-equipment">
                  Equipo
                  <span>Requerido</span>
                </label>

                <select
                  id="acta-equipment"
                  value={equipmentId}
                  onChange={(event) =>
                    setEquipmentId(
                      event.target.value
                    )
                  }
                  disabled={
                    isLoading ||
                    saving ||
                    Boolean(actaOrigenId && actaType === "Devolución")
                  }
                  required
                >
                  <option value="">
                    {loadingEquipment
                      ? "Cargando equipos..."
                      : "Seleccione un equipo"}
                  </option>

                  {equipment.map((item) => (
                    <option
                      key={item.id}
                      value={item.id}
                    >
                      {item.inventoryKey}
                      {" â€” "}
                      {item.nombreEquipo}

                      {item.marca
                        ? ` â€” ${item.marca}`
                        : ""}
                    </option>
                  ))}
                </select>

                <small>
                  Selecciona el equipo que quedarÃ¡
                  asociado al documento.
                </small>
              </div>

              {/* ESTADO */}

              <div className="new-acta-field">
                <label>
                  Estado inicial
                </label>

                <div className="new-acta-readonly">
                  <span className="new-acta-status-dot" />
                  Borrador
                </div>

                <small>
                  El documento podrÃ¡ continuar su
                  proceso posteriormente.
                </small>
              </div>

              {/* DESTINATARIO */}

              <div className="new-acta-field">
                <label htmlFor="acta-destinatario">
                  Destinatario
                  <span>Requerido</span>
                </label>

                <select
                  id="acta-destinatario"
                  value={
                    destinatarioUsuarioId
                  }
                  onChange={(event) =>
                    setDestinatarioUsuarioId(
                      event.target.value
                        ? Number(
                            event.target.value
                          )
                        : ""
                    )
                  }
                  disabled={
                    loadingUsuarios ||
                    saving
                  }
                  required
                >
                  <option value="">
                    {loadingUsuarios
                      ? "Cargando usuarios..."
                      : "Seleccione un destinatario"}
                  </option>

                  {usuarios.map((usuario) => (
                    <option
                      key={usuario.id}
                      value={usuario.id}
                    >
                      {usuario.nombre}{" "}
                      {usuario.apellido}
                      {" â€” "}
                      {usuario.usuarioRed}
                    </option>
                  ))}
                </select>

                <small>
                  Persona que recibirÃ¡ o devolverÃ¡
                  el equipo.
                </small>
              </div>

              {/* MENSAJERO */}

              <div className="new-acta-field">
                <label htmlFor="acta-mensajero">
                  Mensajero
                  <span>Opcional</span>
                </label>

                <select
                  id="acta-mensajero"
                  value={
                    mensajeroUsuarioId
                  }
                  onChange={(event) =>
                    setMensajeroUsuarioId(
                      event.target.value
                        ? Number(
                            event.target.value
                          )
                        : ""
                    )
                  }
                  disabled={
                    loadingUsuarios ||
                    saving
                  }
                >
                  <option value="">
                    {loadingUsuarios
                      ? "Cargando usuarios..."
                      : "Sin mensajero"}
                  </option>

                  {usuarios.map((usuario) => (
                    <option
                      key={usuario.id}
                      value={usuario.id}
                    >
                      {usuario.nombre}{" "}
                      {usuario.apellido}
                      {" â€” "}
                      {usuario.usuarioRed}
                    </option>
                  ))}
                </select>

                <small>
                  Usuario encargado del traslado,
                  si aplica.
                </small>
              </div>
            </div>
          </section>

          {/* OBSERVACIONES */}

          <section className="new-acta-card">
            <div className="new-acta-card-heading">
              <div className="new-acta-number">
                02
              </div>

              <div>
                <h2>
                  Observaciones
                </h2>

                <p>
                  AÃ±ade información adicional al
                  documento.
                </p>
              </div>
            </div>

            <div className="new-acta-field">
              <label htmlFor="acta-observacion">
                ObservaciÃ³n
                <span>Opcional</span>
              </label>

              <textarea
                id="acta-observacion"
                value={observacion}
                onChange={(event) =>
                  setObservacion(
                    event.target.value
                  )
                }
                placeholder="Describe novedades, condiciones del equipo u otra información relevante..."
                rows={7}
                disabled={saving}
              />

              <small>
                Esta información quedarÃ¡ almacenada
                dentro del documento.
              </small>
            </div>
          </section>
        </div>

        {/* VISTA PREVIA */}

        <aside className="new-acta-sidebar">
          <section className="new-acta-preview">
            <span className="new-acta-preview-label">
              VISTA PREVIA
            </span>

            <div className="new-acta-preview-icon">
              A
            </div>

            <h3>
              Acta de{" "}
              {actaType.toLowerCase()}
            </h3>

            <p>
              Documento en estado borrador.
            </p>

            <div className="new-acta-preview-line" />

            <div className="new-acta-preview-data">
              <div>
                <span>
                  Equipo
                </span>

                <strong>
                  {selectedEquipment
                    ? selectedEquipment.inventoryKey
                    : "Sin seleccionar"}
                </strong>
              </div>

              <div>
                <span>
                  Marca
                </span>

                <strong>
                  {selectedEquipment?.marca ??
                    "â€”"}
                </strong>
              </div>

              <div>
                <span>
                  Destinatario
                </span>

                <strong>
                  {selectedDestinatario
                    ? `${selectedDestinatario.nombre} ${selectedDestinatario.apellido}`
                    : "Sin seleccionar"}
                </strong>
              </div>

              <div>
                <span>
                  Mensajero
                </span>

                <strong>
                  {selectedMensajero
                    ? `${selectedMensajero.nombre} ${selectedMensajero.apellido}`
                    : "Sin mensajero"}
                </strong>
              </div>
            </div>
          </section>

          <section className="new-acta-help">
            <span>
              NOTA
            </span>

            <p>
              Al guardar se crearÃ¡ un borrador
              asociado al equipo y usuarios
              seleccionados.
            </p>
          </section>
        </aside>

        {/* ACCIONES */}

        <div className="new-acta-actions">
          <button
            type="button"
            className="new-acta-cancel"
            onClick={onCancel}
            disabled={saving}
          >
            Cancelar
          </button>

          <button
            type="submit"
            className="new-acta-save"
            disabled={
              saving ||
              isLoading ||
              !equipmentId ||
              !destinatarioUsuarioId ||
              (actaType === "Devolución" && !actaOrigenId)
            }
          >
            {saving
              ? "Guardando..."
              : "Guardar borrador â†’"}
          </button>
        </div>
      </form>
    </section>
  );
}

