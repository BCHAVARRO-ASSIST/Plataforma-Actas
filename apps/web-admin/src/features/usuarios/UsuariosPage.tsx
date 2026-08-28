import {
  useEffect,
  useMemo,
  useState,
} from "react";

import {
  createUsuario,
  deleteUsuario,
  getUsuarios,
  updateUsuario,
  assignUsuarioRol,
  type Usuario,
} from "./usuarioService";

import {
  getRoles,
  type Rol,
} from "../roles/rolService";

type EstadoFiltro =
  | "todos"
  | "activos"
  | "inactivos";

type FormularioUsuario = {
  nombre: string;
  apellido: string;
  usuarioRed: string;
  correo: string;
  proceso: string;
  activo: boolean;
};

const formularioInicial: FormularioUsuario = {
  nombre: "",
  apellido: "",
  usuarioRed: "",
  correo: "",
  proceso: "",
  activo: true,
};

export function UsuariosPage() {
  const [usuarios, setUsuarios] =
    useState<Usuario[]>([]);

  const [roles, setRoles] =
    useState<Rol[]>([]);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState<string | null>(null);

  const [mensaje, setMensaje] =
    useState<string | null>(null);

  const [busqueda, setBusqueda] =
    useState("");

  const [estadoFiltro, setEstadoFiltro] =
    useState<EstadoFiltro>("todos");

  const [procesoFiltro, setProcesoFiltro] =
    useState("todos");

  const [modalAbierto, setModalAbierto] =
    useState(false);

  const [modoFormulario, setModoFormulario] =
    useState<"crear" | "editar">("crear");

  const [usuarioSeleccionado, setUsuarioSeleccionado] =
    useState<Usuario | null>(null);

  const [formulario, setFormulario] =
    useState<FormularioUsuario>(
      formularioInicial
    );

  const [guardando, setGuardando] =
    useState(false);

  const [eliminandoId, setEliminandoId] =
    useState<number | null>(null);

  // =========================================================
  // MODAL ASIGNAR / CAMBIAR ROL
  // =========================================================

  const [rolModalAbierto, setRolModalAbierto] =
    useState(false);

  const [usuarioRolSeleccionado, setUsuarioRolSeleccionado] =
    useState<Usuario | null>(null);

  const [rolSeleccionadoId, setRolSeleccionadoId] =
    useState<number | "">("");

  const [guardandoRol, setGuardandoRol] =
    useState(false);

  // =========================================================
  // CONFIRMACIÓN ACTIVAR / DESACTIVAR
  // =========================================================

  const [confirmacionAbierta, setConfirmacionAbierta] =
    useState(false);

  const [usuarioConfirmacion, setUsuarioConfirmacion] =
    useState<Usuario | null>(null);

  const [accionConfirmacion, setAccionConfirmacion] =
    useState<"activar" | "desactivar">(
      "desactivar"
    );

  // =========================================================
  // CARGAR USUARIOS
  // =========================================================

  async function cargarUsuarios() {
    try {
      setLoading(true);
      setError(null);

      const data =
        await getUsuarios();

      setUsuarios(data);
    } catch (err) {
      console.error(
        "Error cargando usuarios:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible cargar los usuarios."
      );
    } finally {
      setLoading(false);
    }
  }

  // =========================================================
  // CARGA INICIAL
  // =========================================================

  useEffect(() => {
    async function cargarDatosIniciales() {
      try {
        setLoading(true);
        setError(null);

        const [
          usuariosData,
          rolesData,
        ] = await Promise.all([
          getUsuarios(),
          getRoles(),
        ]);

        setUsuarios(
          usuariosData
        );

        setRoles(
          rolesData.filter(
            (rol) => rol.activo
          )
        );
      } catch (err) {
        console.error(
          "Error cargando usuarios y roles:",
          err
        );

        setError(
          err instanceof Error
            ? err.message
            : "No fue posible cargar la información."
        );
      } finally {
        setLoading(false);
      }
    }

    void cargarDatosIniciales();
  }, []);

  // =========================================================
  // MÉTRICAS
  // =========================================================

  const usuariosActivos =
    usuarios.filter(
      (usuario) => usuario.activo
    ).length;

  const usuariosSinRol =
    usuarios.filter(
      (usuario) =>
        usuario.rolId === null ||
        !usuario.rolNombre
    ).length;

  // =========================================================
  // PROCESOS
  // =========================================================

  const procesos = useMemo(() => {
    const valores = usuarios
      .map(
        (usuario) =>
          usuario.proceso
      )
      .filter(Boolean);

    return Array.from(
      new Set(valores)
    ).sort((a, b) =>
      a.localeCompare(b, "es")
    );
  }, [usuarios]);

  // =========================================================
  // FILTROS
  // =========================================================

  const usuariosFiltrados =
    useMemo(() => {
      const texto =
        busqueda
          .trim()
          .toLowerCase();

      return usuarios.filter(
        (usuario) => {
          const nombreCompleto =
            `${usuario.nombre} ${usuario.apellido}`
              .toLowerCase();

          const coincideBusqueda =
            !texto ||
            nombreCompleto.includes(
              texto
            ) ||
            usuario.usuarioRed
              .toLowerCase()
              .includes(texto) ||
            usuario.correo
              .toLowerCase()
              .includes(texto) ||
            usuario.proceso
              .toLowerCase()
              .includes(texto) ||
            (
              usuario.rolNombre ??
              ""
            )
              .toLowerCase()
              .includes(texto);

          const coincideEstado =
            estadoFiltro === "todos" ||
            (
              estadoFiltro === "activos" &&
              usuario.activo
            ) ||
            (
              estadoFiltro === "inactivos" &&
              !usuario.activo
            );

          const coincideProceso =
            procesoFiltro === "todos" ||
            usuario.proceso ===
              procesoFiltro;

          return (
            coincideBusqueda &&
            coincideEstado &&
            coincideProceso
          );
        }
      );
    }, [
      usuarios,
      busqueda,
      estadoFiltro,
      procesoFiltro,
    ]);

  // =========================================================
  // ABRIR CREAR
  // =========================================================

  function abrirCrear() {
    setModoFormulario("crear");

    setUsuarioSeleccionado(null);

    setFormulario(
      formularioInicial
    );

    setError(null);
    setMensaje(null);

    setModalAbierto(true);
  }

  // =========================================================
  // ABRIR EDITAR
  // =========================================================

  function abrirEditar(
    usuario: Usuario
  ) {
    setModoFormulario("editar");

    setUsuarioSeleccionado(
      usuario
    );

    setFormulario({
      nombre: usuario.nombre,
      apellido: usuario.apellido,
      usuarioRed:
        usuario.usuarioRed,
      correo: usuario.correo,
      proceso: usuario.proceso,
      activo: usuario.activo,
    });

    setError(null);
    setMensaje(null);

    setModalAbierto(true);
  }

  // =========================================================
  // CERRAR MODAL CREAR / EDITAR
  // =========================================================

  function cerrarModal() {
    if (guardando) {
      return;
    }

    setModalAbierto(false);

    setUsuarioSeleccionado(null);

    setFormulario(
      formularioInicial
    );
  }

  // =========================================================
  // CAMBIAR FORMULARIO
  // =========================================================

  function actualizarCampo(
    campo: keyof FormularioUsuario,
    valor: string | boolean
  ) {
    setFormulario(
      (actual) => ({
        ...actual,
        [campo]: valor,
      })
    );
  }

  // =========================================================
  // GUARDAR USUARIO
  // =========================================================

  async function guardarUsuario(
    event: React.FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    setError(null);
    setMensaje(null);

    if (
      !formulario.nombre.trim() ||
      !formulario.apellido.trim() ||
      !formulario.usuarioRed.trim() ||
      !formulario.correo.trim() ||
      !formulario.proceso.trim()
    ) {
      setError(
        "Todos los campos son obligatorios."
      );

      return;
    }

    try {
      setGuardando(true);

      if (
        modoFormulario ===
        "crear"
      ) {
        await createUsuario({
          nombre:
            formulario.nombre,
          apellido:
            formulario.apellido,
          usuarioRed:
            formulario.usuarioRed,
          correo:
            formulario.correo,
          proceso:
            formulario.proceso,
        });

        setMensaje(
          "Usuario creado correctamente."
        );
      } else if (
        usuarioSeleccionado
      ) {
        await updateUsuario(
          usuarioSeleccionado.id,
          formulario
        );

        setMensaje(
          "Usuario actualizado correctamente."
        );
      }

      await cargarUsuarios();

      setModalAbierto(false);

      setUsuarioSeleccionado(null);

      setFormulario(
        formularioInicial
      );
    } catch (err) {
      console.error(
        "Error guardando usuario:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible guardar el usuario."
      );
    } finally {
      setGuardando(false);
    }
  }

  // =========================================================
  // ABRIR ASIGNACIÓN / CAMBIO DE ROL
  // =========================================================

  function abrirAsignarRol(
    usuario: Usuario
  ) {
    setUsuarioRolSeleccionado(
      usuario
    );

    setRolSeleccionadoId(
      usuario.rolId ?? ""
    );

    setError(null);
    setMensaje(null);

    setRolModalAbierto(true);
  }

  // =========================================================
  // CERRAR ASIGNACIÓN DE ROL
  // =========================================================

  function cerrarAsignarRol() {
    if (guardandoRol) {
      return;
    }

    setRolModalAbierto(false);

    setUsuarioRolSeleccionado(
      null
    );

    setRolSeleccionadoId("");
  }

  // =========================================================
  // GUARDAR ROL
  // =========================================================

  async function guardarRolUsuario() {
    if (
      !usuarioRolSeleccionado
    ) {
      return;
    }

    if (
      rolSeleccionadoId === ""
    ) {
      setError(
        "Debe seleccionar un rol."
      );

      return;
    }

    try {
      setGuardandoRol(true);

      setError(null);
      setMensaje(null);

      await assignUsuarioRol(
        usuarioRolSeleccionado.id,
        Number(rolSeleccionadoId)
      );

      await cargarUsuarios();

      setRolModalAbierto(false);

      setUsuarioRolSeleccionado(
        null
      );

      setRolSeleccionadoId("");

      setMensaje(
        usuarioRolSeleccionado.rolId === null
          ? "Rol asignado correctamente."
          : "Rol actualizado correctamente."
      );
    } catch (err) {
      console.error(
        "Error asignando rol:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible asignar el rol."
      );
    } finally {
      setGuardandoRol(false);
    }
  }

  // =========================================================
  // SOLICITAR DESACTIVACIÓN
  // =========================================================

  function solicitarDesactivacion(
    usuario: Usuario
  ) {
    if (!usuario.activo) {
      return;
    }

    setUsuarioConfirmacion(
      usuario
    );

    setAccionConfirmacion(
      "desactivar"
    );

    setConfirmacionAbierta(
      true
    );

    setError(null);
    setMensaje(null);
  }

  // =========================================================
  // CONFIRMAR DESACTIVACIÓN
  // =========================================================

  async function confirmarDesactivacion() {
    if (!usuarioConfirmacion) {
      return;
    }

    try {
      setEliminandoId(
        usuarioConfirmacion.id
      );

      setError(null);
      setMensaje(null);

      await deleteUsuario(
        usuarioConfirmacion.id
      );

      await cargarUsuarios();

      setMensaje(
        "Usuario desactivado correctamente."
      );

      setConfirmacionAbierta(
        false
      );

      setUsuarioConfirmacion(
        null
      );
    } catch (err) {
      console.error(
        "Error desactivando usuario:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible desactivar el usuario."
      );
    } finally {
      setEliminandoId(null);
    }
  }

  // =========================================================
  // SOLICITAR ACTIVACIÓN
  // =========================================================

  function solicitarActivacion(
    usuario: Usuario
  ) {
    if (usuario.activo) {
      return;
    }

    setUsuarioConfirmacion(
      usuario
    );

    setAccionConfirmacion(
      "activar"
    );

    setConfirmacionAbierta(
      true
    );

    setError(null);
    setMensaje(null);
  }

  // =========================================================
  // CONFIRMAR ACTIVACIÓN
  // =========================================================

  async function confirmarActivacion() {
    if (!usuarioConfirmacion) {
      return;
    }

    try {
      setEliminandoId(
        usuarioConfirmacion.id
      );

      setError(null);
      setMensaje(null);

      await updateUsuario(
        usuarioConfirmacion.id,
        {
          nombre:
            usuarioConfirmacion.nombre,
          apellido:
            usuarioConfirmacion.apellido,
          usuarioRed:
            usuarioConfirmacion.usuarioRed,
          correo:
            usuarioConfirmacion.correo,
          proceso:
            usuarioConfirmacion.proceso,
          activo: true,
        }
      );

      await cargarUsuarios();

      setMensaje(
        "Usuario activado correctamente."
      );

      setConfirmacionAbierta(
        false
      );

      setUsuarioConfirmacion(
        null
      );
    } catch (err) {
      console.error(
        "Error activando usuario:",
        err
      );

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible activar el usuario."
      );
    } finally {
      setEliminandoId(null);
    }
  }

  // =========================================================
  // CONFIRMAR ACCIÓN
  // =========================================================

  async function confirmarAccion() {
    if (
      accionConfirmacion ===
      "activar"
    ) {
      await confirmarActivacion();
      return;
    }

    await confirmarDesactivacion();
  }

  // =========================================================
  // CERRAR CONFIRMACIÓN
  // =========================================================

  function cerrarConfirmacion() {
    if (
      eliminandoId !== null
    ) {
      return;
    }

    setConfirmacionAbierta(
      false
    );

    setUsuarioConfirmacion(
      null
    );
  }

  return (
    <section className="usuarios-page">

      {/* =====================================================
          ENCABEZADO
          ===================================================== */}

      <header className="usuarios-header">

        <div>

          <span className="page-eyebrow">
            ADMINISTRACIÓN DE PERSONAL Y ACCESOS
          </span>

          <h1>Directorio de Usuarios y Cuentas de Red</h1>

          <p>
            Control de cuentas de red, estado de acceso, área o proceso y asignación de roles de seguridad.
          </p>

        </div>

        <button
          type="button"
          className="usuarios-add-button"
          onClick={abrirCrear}
        >
          <span>+</span>
          Crear nuevo usuario
        </button>

      </header>



      {/* =====================================================
          MENSAJE DE ÉXITO
          ===================================================== */}

      {mensaje && (
        <div className="usuarios-success">

          <span>
            ✓
          </span>

          {mensaje}

          <button
            type="button"
            onClick={() =>
              setMensaje(null)
            }
            aria-label="Cerrar mensaje"
          >
            ×
          </button>

        </div>
      )}


      {/* =====================================================
          MÉTRICAS
          ===================================================== */}

      <section className="usuarios-summary">

        <article className="usuarios-stat-card">


          <div className="usuarios-stat-top">

            <span>
              TOTAL USUARIOS
            </span>

            <span className="usuarios-stat-icon">
              ◉
            </span>

          </div>

          <strong>
            {loading
              ? "..."
              : usuarios.length}
          </strong>

          <small>
            Usuarios registrados
          </small>

        </article>


        <article className="usuarios-stat-card usuarios-stat-active">

          <div className="usuarios-stat-top">

            <span>
              ACTIVOS
            </span>

            <span className="usuarios-stat-icon">
              ● 
            </span>

          </div>

          <strong>
            {loading
              ? "..."
              : usuariosActivos}
          </strong>

          <small>
            Con acceso habilitado
          </small>

        </article>


        <article className="usuarios-stat-card">

          <div className="usuarios-stat-top">

            <span>
              SIN ROL
            </span>

            <span className="usuarios-stat-icon">
              !
            </span>

          </div>

          <strong>
            {loading
              ? "..."
              : usuariosSinRol}
          </strong>

          <small>
            Requieren asignación de rol
          </small>

        </article>

      </section>


      {/* =====================================================
          ERROR
          ===================================================== */}

      {error && (
        <div
          className="usuarios-error"
          role="alert"
        >

          <strong>
            Atención
          </strong>

          <span>
            {error}
          </span>

        </div>
      )}


      {/* =====================================================
          PANEL
          ===================================================== */}

      <section className="usuarios-panel">

        <div className="usuarios-toolbar">

          <div className="usuarios-toolbar-title">

            <div>

              <span className="usuarios-toolbar-kicker">
                DIRECTORIO
              </span>

              <h2>
                Usuarios registrados
              </h2>

            </div>

            <span className="usuarios-result-count">
              {usuariosFiltrados.length}
            </span>

          </div>


          <div className="usuarios-toolbar-controls">

            <div className="usuarios-search">

              <span className="usuarios-search-icon">
                🔍
              </span>

              <input
                type="search"
                value={busqueda}
                onChange={(event) =>
                  setBusqueda(
                    event.target.value
                  )
                }
                placeholder="Buscar usuario, correo o proceso..."
              />

              {busqueda && (
                <button
                  type="button"
                  className="usuarios-search-clear"
                  onClick={() =>
                    setBusqueda("")
                  }
                  aria-label="Limpiar búsqueda"
                >
                  ×
                </button>
              )}

            </div>


            <select
              className="usuarios-filter"
              value={estadoFiltro}
              onChange={(event) =>
                setEstadoFiltro(
                  event.target
                    .value as EstadoFiltro
                )
              }
            >

              <option value="todos">
                Todos los estados
              </option>

              <option value="activos">
                Activos
              </option>

              <option value="inactivos">
                Inactivos
              </option>

            </select>


            <select
              className="usuarios-filter"
              value={procesoFiltro}
              onChange={(event) =>
                setProcesoFiltro(
                  event.target.value
                )
              }
            >

              <option value="todos">
                Todos los procesos
              </option>

              {procesos.map(
                (proceso) => (
                  <option
                    key={proceso}
                    value={proceso}
                  >
                    {proceso}
                  </option>
                )
              )}

            </select>

          </div>

        </div>


        {/* ===================================================
            TABLA
            =================================================== */}

        <div className="usuarios-table-wrapper">

          <table className="usuarios-table">

            <thead>

              <tr>

                <th>
                  USUARIO
                </th>

                <th>
                  CORREO
                </th>

                <th>
                  PROCESO
                </th>

                <th>
                  ROL
                </th>

                <th>
                  ESTADO
                </th>

                <th>
                  ACCIONES
                </th>

              </tr>

            </thead>

            <tbody>

              {loading && (
                <tr>

                  <td
                    colSpan={6}
                    className="usuarios-empty"
                  >

                    <div className="usuarios-loading">

                      <span />
                      <span />
                      <span />

                    </div>

                    Cargando usuarios...

                  </td>

                </tr>
              )}


              {!loading &&
                !error &&
                usuariosFiltrados.length === 0 && (
                  <tr>

                    <td
                      colSpan={6}
                      className="usuarios-empty"
                    >

                      <div className="usuarios-empty-icon">
                        ◉
                      </div>

                      <strong>
                        No se encontraron usuarios
                      </strong>

                      <span>
                        Modifica los filtros o
                        el término de búsqueda.
                      </span>

                    </td>

                  </tr>
                )}


              {!loading &&
                usuariosFiltrados.map(
                  (usuario) => {

                    const iniciales =
                      `${usuario.nombre.charAt(0)}${usuario.apellido.charAt(0)}`
                        .toUpperCase();

                    const tieneRol =
                      usuario.rolId !== null &&
                      Boolean(
                        usuario.rolNombre
                      );

                    return (
                      <tr
                        key={usuario.id}
                      >

                        {/* USUARIO */}

                        <td>

                          <div className="usuario-identity">

                            <div className="usuario-avatar">
                              {iniciales}
                            </div>

                            <div className="usuario-identity-info">

                              <strong>
                                {usuario.nombre}{" "}
                                {usuario.apellido}
                              </strong>

                              <span>
                                @{usuario.usuarioRed}
                              </span>

                            </div>

                          </div>

                        </td>


                        {/* CORREO */}

                        <td>

                          <div className="usuario-email">

                            <strong>
                              {usuario.correo}
                            </strong>

                          </div>

                        </td>


                        {/* PROCESO */}

                        <td>

                          <span className="usuario-proceso">
                            {usuario.proceso}
                          </span>

                        </td>


                        {/* ROL */}

                        <td>

                          {tieneRol ? (
                            <span
                              className="usuario-rol usuario-rol-assigned"
                              title={`Rol: ${usuario.rolNombre}`}
                            >

                              <span className="usuario-rol-dot" />

                              {usuario.rolNombre}

                            </span>
                          ) : (
                            <span
                              className="usuario-rol usuario-rol-empty"
                              title="Este usuario no tiene un rol asignado"
                            >

                              <span className="usuario-rol-dot" />

                              Sin rol asignado

                            </span>
                          )}

                        </td>


                        {/* ESTADO */}

                        <td>

                          <span
                            className={
                              usuario.activo
                                ? "usuario-status active"
                                : "usuario-status inactive"
                            }
                          >

                            <span />

                            {usuario.activo
                              ? "Activo"
                              : "Inactivo"}

                          </span>

                        </td>


                        {/* ACCIONES */}

                        <td>

                          <div className="usuario-actions">

                            <button
                              type="button"
                              className={`usuario-role-button ${
                                !tieneRol
                                  ? "usuario-role-button-pending"
                                  : ""
                              }`}
                              onClick={() =>
                                abrirAsignarRol(
                                  usuario
                                )
                              }
                              title={
                                tieneRol
                                  ? "Cambiar rol del usuario"
                                  : "Asignar un rol al usuario"
                              }
                            >

                              {tieneRol
                                ? "Cambiar rol"
                                : "Asignar rol"}

                            </button>


                            <button
                              type="button"
                              className="usuario-edit-button"
                              onClick={() =>
                                abrirEditar(
                                  usuario
                                )
                              }
                            >
                              Editar
                            </button>


                            {usuario.activo ? (
                              <button
                                type="button"
                                className="usuario-delete-button"
                                disabled={
                                  eliminandoId ===
                                  usuario.id
                                }
                                onClick={() =>
                                  solicitarDesactivacion(
                                    usuario
                                  )
                                }
                              >

                                {eliminandoId ===
                                usuario.id
                                  ? "..."
                                  : "Desactivar"}

                              </button>
                            ) : (
                              <button
                                type="button"
                                className="usuario-activate-button"
                                disabled={
                                  eliminandoId ===
                                  usuario.id
                                }
                                onClick={() =>
                                  solicitarActivacion(
                                    usuario
                                  )
                                }
                              >

                                {eliminandoId ===
                                usuario.id
                                  ? "..."
                                  : "Activar"}

                              </button>
                            )}

                          </div>

                        </td>

                      </tr>
                    );
                  }
                )}

            </tbody>

          </table>

        </div>


        {/* ===================================================
            PIE
            =================================================== */}

        {!loading &&
          !error && (
            <footer className="usuarios-footer">

              <span>

                Mostrando{" "}

                <strong>
                  {usuariosFiltrados.length}
                </strong>

                {" "}de{" "}

                <strong>
                  {usuarios.length}
                </strong>

                {" "}usuarios

              </span>

              {(busqueda ||
                estadoFiltro !== "todos" ||
                procesoFiltro !== "todos") && (

                <button
                  type="button"
                  onClick={() => {
                    setBusqueda("");
                    setEstadoFiltro("todos");
                    setProcesoFiltro("todos");
                  }}
                >
                  Limpiar filtros
                </button>

              )}

            </footer>
          )}

      </section>


      {/* =====================================================
          MODAL CREAR / EDITAR
          ===================================================== */}

      {modalAbierto && (
        <div
          className="usuarios-modal-overlay"
          onMouseDown={(event) => {
            if (
              event.target ===
              event.currentTarget
            ) {
              cerrarModal();
            }
          }}
        >

          <div
            className="usuarios-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="usuarios-modal-title"
          >

            <div className="usuarios-modal-header">

              <div>

                <span>
                  {modoFormulario ===
                  "crear"
                    ? "NUEVO REGISTRO"
                    : "EDICIÓN DE USUARIO"}
                </span>

                <h2 id="usuarios-modal-title">
                  {modoFormulario ===
                  "crear"
                    ? "Crear usuario"
                    : "Editar usuario"}
                </h2>

              </div>

              <button
                type="button"
                className="usuarios-modal-close"
                onClick={
                  cerrarModal
                }
                disabled={guardando}
                aria-label="Cerrar"
              >
                ×
              </button>

            </div>


            <form
              className="usuarios-form"
              onSubmit={
                guardarUsuario
              }
            >

              <div className="usuarios-form-grid">

                <label className="usuarios-field">

                  <span>
                    Nombre *
                  </span>

                  <input
                    type="text"
                    value={
                      formulario.nombre
                    }
                    onChange={(event) =>
                      actualizarCampo(
                        "nombre",
                        event.target.value
                      )
                    }
                    placeholder="Ej. Juan"
                    autoComplete="off"
                    required
                  />

                </label>


                <label className="usuarios-field">

                  <span>
                    Apellido *
                  </span>

                  <input
                    type="text"
                    value={
                      formulario.apellido
                    }
                    onChange={(event) =>
                      actualizarCampo(
                        "apellido",
                        event.target.value
                      )
                    }
                    placeholder="Ej. Pérez"
                    autoComplete="off"
                    required
                  />

                </label>


                <label className="usuarios-field">

                  <span>
                    Usuario de red *
                  </span>

                  <input
                    type="text"
                    value={
                      formulario.usuarioRed
                    }
                    onChange={(event) =>
                      actualizarCampo(
                        "usuarioRed",
                        event.target.value
                      )
                    }
                    placeholder="Ej. jperez"
                    autoComplete="off"
                    required
                  />

                </label>


                <label className="usuarios-field">

                  <span>
                    Correo electrónico *
                  </span>

                  <input
                    type="email"
                    value={
                      formulario.correo
                    }
                    onChange={(event) =>
                      actualizarCampo(
                        "correo",
                        event.target.value
                      )
                    }
                    placeholder="usuario@empresa.com"
                    autoComplete="off"
                    required
                  />

                </label>


                <label className="usuarios-field">

                  <span>
                    Proceso *
                  </span>

                  <input
                    type="text"
                    value={
                      formulario.proceso
                    }
                    onChange={(event) =>
                      actualizarCampo(
                        "proceso",
                        event.target.value
                      )
                    }
                    placeholder="Ej. Tecnología"
                    autoComplete="off"
                    required
                  />

                </label>


                {modoFormulario ===
                  "editar" && (
                  <label className="usuarios-field">

                    <span>
                      Estado
                    </span>

                    <select
                      value={
                        formulario.activo
                          ? "activo"
                          : "inactivo"
                      }
                      onChange={(event) =>
                        actualizarCampo(
                          "activo",
                          event.target.value ===
                            "activo"
                        )
                      }
                    >

                      <option value="activo">
                        Activo
                      </option>

                      <option value="inactivo">
                        Inactivo
                      </option>

                    </select>

                  </label>
                )}

              </div>


              <div className="usuarios-form-note">

                <strong>
                  Información
                </strong>

                <span>
                  Los campos marcados con *
                  son obligatorios.
                </span>

              </div>


              <div className="usuarios-modal-actions">

                <button
                  type="button"
                  className="usuarios-modal-cancel"
                  onClick={
                    cerrarModal
                  }
                  disabled={
                    guardando
                  }
                >
                  Cancelar
                </button>

                <button
                  type="submit"
                  className="usuarios-modal-save"
                  disabled={
                    guardando
                  }
                >

                  {guardando
                    ? "Guardando..."
                    : modoFormulario ===
                      "crear"
                      ? "Crear usuario"
                      : "Guardar cambios"}

                </button>

              </div>

            </form>

          </div>

        </div>
      )}


      {/* =====================================================
          MODAL ASIGNAR / CAMBIAR ROL
          ===================================================== */}

      {rolModalAbierto &&
        usuarioRolSeleccionado && (
          <div
            className="usuarios-role-overlay"
            onMouseDown={(event) => {
              if (
                event.target ===
                event.currentTarget
              ) {
                cerrarAsignarRol();
              }
            }}
          >

            <div
              className="usuarios-role-modal"
              role="dialog"
              aria-modal="true"
              aria-labelledby="usuarios-role-title"
            >

              <div className="usuarios-role-header">

                <div>

                  <span>
                    GESTIÓN DE ACCESO
                  </span>

                  <h2 id="usuarios-role-title">

                    {usuarioRolSeleccionado.rolId !==
                      null &&
                    usuarioRolSeleccionado.rolNombre
                      ? "Cambiar rol"
                      : "Asignar rol"}

                  </h2>

                  <p>
                    Define el nivel de acceso
                    del usuario seleccionado.
                  </p>

                </div>

                <button
                  type="button"
                  className="usuarios-role-close"
                  onClick={
                    cerrarAsignarRol
                  }
                  disabled={
                    guardandoRol
                  }
                  aria-label="Cerrar"
                >
                  ×
                </button>

              </div>


              <div className="usuarios-role-user">

                <div className="usuarios-role-avatar">

                  {usuarioRolSeleccionado.nombre.charAt(0)}
                  {usuarioRolSeleccionado.apellido.charAt(0)}

                </div>

                <div>

                  <strong>
                    {usuarioRolSeleccionado.nombre}{" "}
                    {usuarioRolSeleccionado.apellido}
                  </strong>

                  <span>
                    @{usuarioRolSeleccionado.usuarioRed}
                  </span>

                </div>

              </div>


              <div className="usuarios-role-field">

                <label htmlFor="usuario-rol">
                  Perfil de acceso
                </label>

                <select
                  id="usuario-rol"
                  value={
                    rolSeleccionadoId
                  }
                  onChange={(event) => {
                    const value =
                      event.target.value;

                    setRolSeleccionadoId(
                      value === ""
                        ? ""
                        : Number(value)
                    );
                  }}
                  disabled={
                    guardandoRol
                  }
                >

                  <option value="">
                    Seleccione un rol
                  </option>

                  {roles.map(
                    (rol) => (
                      <option
                        key={rol.id}
                        value={rol.id}
                      >
                        {rol.nombre}
                      </option>
                    )
                  )}

                </select>

              </div>


              <div className="usuarios-role-info">

                <span>
                  i
                </span>

                <p>
                  El rol determina los permisos
                  disponibles para este usuario.
                </p>

              </div>


              <div className="usuarios-role-actions">

                <button
                  type="button"
                  className="usuarios-role-cancel"
                  onClick={
                    cerrarAsignarRol
                  }
                  disabled={
                    guardandoRol
                  }
                >
                  Cancelar
                </button>

                <button
                  type="button"
                  className="usuarios-role-save"
                  onClick={
                    guardarRolUsuario
                  }
                  disabled={
                    guardandoRol ||
                    rolSeleccionadoId === ""
                  }
                >

                  {guardandoRol
                    ? "Guardando..."
                    : usuarioRolSeleccionado.rolId !==
                        null &&
                      usuarioRolSeleccionado.rolNombre
                      ? "Guardar cambios"
                      : "Asignar rol"}

                </button>

              </div>

            </div>

          </div>
        )}


      {/* =====================================================
          MODAL CONFIRMACIÓN ACTIVAR / DESACTIVAR
          ===================================================== */}

      {confirmacionAbierta &&
        usuarioConfirmacion && (
          <div
            className={`usuarios-confirmation-overlay ${
              accionConfirmacion ===
              "activar"
                ? "usuarios-confirmation-success"
                : "usuarios-confirmation-warning"
            }`}
          >

            <div
              className="usuarios-confirmation-modal"
              role="dialog"
              aria-modal="true"
              aria-labelledby="usuarios-confirmation-title"
            >

              <div className="usuarios-confirmation-icon">

                {accionConfirmacion ===
                "activar"
                  ? "✓"
                  : "!"}

              </div>


              <div className="usuarios-confirmation-content">

                <span className="usuarios-confirmation-eyebrow">
                  CAMBIO DE ESTADO
                </span>

                <h2 id="usuarios-confirmation-title">

                  {accionConfirmacion ===
                  "activar"
                    ? "Activar usuario"
                    : "Desactivar usuario"}

                </h2>

                <p className="usuarios-confirmation-description">

                  ¿Deseas{" "}

                  {accionConfirmacion ===
                  "activar"
                    ? "activar"
                    : "desactivar"}

                  {" "}al usuario{" "}

                  <strong>
                    {usuarioConfirmacion.nombre}{" "}
                    {usuarioConfirmacion.apellido}
                  </strong>
                  ?

                </p>


                <div className="usuarios-confirmation-detail">

                  <span className="usuarios-confirmation-detail-icon">
                    i
                  </span>

                  <span>

                    {accionConfirmacion ===
                    "activar"
                      ? "El usuario volverá a tener acceso habilitado a la plataforma."
                      : "El usuario perderá su acceso activo a la plataforma, pero su información permanecerá registrada."}

                  </span>

                </div>

              </div>


              <div className="usuarios-confirmation-actions">

                <button
                  type="button"
                  className="usuarios-confirmation-cancel"
                  disabled={
                    eliminandoId !== null
                  }
                  onClick={
                    cerrarConfirmacion
                  }
                >
                  Cancelar
                </button>


                <button
                  type="button"
                  className={`usuarios-confirmation-submit ${
                    accionConfirmacion ===
                    "activar"
                      ? "usuarios-confirmation-submit-success"
                      : "usuarios-confirmation-submit-warning"
                  }`}
                  disabled={
                    eliminandoId !== null
                  }
                  onClick={
                    confirmarAccion
                  }
                >

                  {eliminandoId !== null
                    ? "Procesando..."
                    : accionConfirmacion ===
                      "activar"
                      ? "Activar"
                      : "Desactivar"}

                </button>

              </div>

            </div>

          </div>
        )}

    </section>
  );
}
