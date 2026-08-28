import {
  useEffect,
  useMemo,
  useState,
} from "react";

import {
  createRol,
  deleteRol,
  getPermisos,
  getRol,
  getRoles,
  setRolActivo,
  updateRol,
  updateRolPermisos,
  type Permiso,
  type Rol,
  type RolDetalle,
} from "./rolService";

type ModalType =
  | "crear"
  | "editar"
  | "permisos"
  | null;

type ConfirmationAction =
  | "activar"
  | "desactivar"
  | "eliminar"
  | null;

// =========================================================
// FORMATEAR NOMBRE VISIBLE DEL PERMISO
// =========================================================
// =========================================================

function getPermissionLabel(
  permissionName: string
): string {

  const action =
    permissionName
      .split("_")
      .pop();

  if (!action) {
    return permissionName;
  }

  const labels: Record<string, string> = {
    CONSULTAR: "Consultar",
    REGISTRAR: "Registrar",
    CREAR: "Crear",
    EDITAR: "Editar",
    DESACTIVAR: "Desactivar",
    FIRMAR: "Firmar",
    EXPORTAR: "Exportar",
    ELIMINAR: "Eliminar",
  };

  return (
    labels[action] ??
    (
      action.charAt(0).toUpperCase() +
      action.slice(1).toLowerCase()
    )
  );
}


export function RolesPage() {

  const [roles, setRoles] =
    useState<Rol[]>([]);

  const [permisos, setPermisos] =
    useState<Permiso[]>([]);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState<string | null>(null);

  const [busqueda, setBusqueda] =
    useState("");

  const [modal, setModal] =
    useState<ModalType>(null);

  const [rolSeleccionado, setRolSeleccionado] =
    useState<RolDetalle | null>(null);

  const [nombre, setNombre] =
    useState("");

  const [descripcion, setDescripcion] =
    useState("");

  const [permisosSeleccionados, setPermisosSeleccionados] =
    useState<number[]>([]);

  const [guardando, setGuardando] =
    useState(false);

  const [cambiandoEstado, setCambiandoEstado] =
    useState<number | null>(null);


  // =========================================================
  // CONFIRMACIÓN DE ACCIONES
  // =========================================================

  const [confirmationAction, setConfirmationAction] =
    useState<ConfirmationAction>(null);

  const [rolParaConfirmar, setRolParaConfirmar] =
    useState<Rol | null>(null);

  const [procesandoConfirmacion, setProcesandoConfirmacion] =
    useState(false);


  // =========================================================
  // CARGAR DATOS
  // =========================================================

  async function cargarDatos() {

    try {

      setLoading(true);
      setError(null);

      const [
        rolesData,
        permisosData,
      ] = await Promise.all([
        getRoles(),
        getPermisos(),
      ]);

      setRoles(rolesData);
      setPermisos(permisosData);

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible cargar los roles."
      );

    } finally {

      setLoading(false);

    }
  }


  useEffect(() => {

    void Promise.resolve().then(cargarDatos);

  }, []);


  // =========================================================
  // FILTRAR
  // =========================================================

  const rolesFiltrados =
    useMemo(() => {

      const texto =
        busqueda
          .trim()
          .toLowerCase();

      if (!texto) {
        return roles;
      }

      return roles.filter(
        (rol) =>
          rol.nombre
            .toLowerCase()
            .includes(texto) ||
          (
            rol.descripcion ?? ""
          )
            .toLowerCase()
            .includes(texto)
      );

    }, [
      roles,
      busqueda,
    ]);


  const rolesActivos =
    roles.filter(
      (rol) => rol.activo
    ).length;


  const rolesInactivos =
    roles.filter(
      (rol) => !rol.activo
    ).length;


  // =========================================================
  // PERMISOS AGRUPADOS
  // =========================================================

  const permisosPorModulo =
    useMemo(() => {

      const grupos =
        new Map<
          string,
          Permiso[]
        >();

      permisos.forEach(
        (permiso) => {

          const grupo =
            grupos.get(
              permiso.modulo
            ) ?? [];

          grupo.push(
            permiso
          );

          grupos.set(
            permiso.modulo,
            grupo
          );

        }
      );

      return Array.from(
        grupos.entries()
      );

    }, [permisos]);


  // =========================================================
  // CREAR
  // =========================================================

  function abrirCrear() {

    setRolSeleccionado(null);
    setNombre("");
    setDescripcion("");
    setPermisosSeleccionados([]);
    setError(null);
    setModal("crear");

  }


  // =========================================================
  // EDITAR
  // =========================================================

  async function abrirEditar(
    rol: Rol
  ) {

    try {

      setError(null);

      const detalle =
        await getRol(rol.id);

      setRolSeleccionado(
        detalle
      );

      setNombre(
        detalle.nombre
      );

      setDescripcion(
        detalle.descripcion ?? ""
      );

      setPermisosSeleccionados(
        detalle.permisos.map(
          (permiso) =>
            permiso.id
        )
      );

      setModal("editar");

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible obtener el rol."
      );

    }

  }


  // =========================================================
  // PERMISOS
  // =========================================================

  async function abrirPermisos(
    rol: Rol
  ) {

    try {

      setError(null);

      const detalle =
        await getRol(rol.id);

      setRolSeleccionado(
        detalle
      );

      setPermisosSeleccionados(
        detalle.permisos.map(
          (permiso) =>
            permiso.id
        )
      );

      setModal("permisos");

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible obtener los permisos."
      );

    }

  }


  function togglePermiso(
    permisoId: number
  ) {

    setPermisosSeleccionados(
      (actuales) =>
        actuales.includes(
          permisoId
        )
          ? actuales.filter(
              (id) =>
                id !== permisoId
            )
          : [
              ...actuales,
              permisoId,
            ]
    );

  }


  function seleccionarModulo(
    permisosModulo: Permiso[]
  ) {

    const ids =
      permisosModulo.map(
        (permiso) =>
          permiso.id
      );

    const todosSeleccionados =
      ids.every((id) =>
        permisosSeleccionados.includes(
          id
        )
      );

    setPermisosSeleccionados(
      (actuales) =>
        todosSeleccionados
          ? actuales.filter(
              (id) =>
                !ids.includes(id)
            )
          : Array.from(
              new Set([
                ...actuales,
                ...ids,
              ])
            )
    );

  }


  function seleccionarTodos() {

    setPermisosSeleccionados(
      permisos.map(
        (permiso) =>
          permiso.id
      )
    );

  }


  function limpiarPermisos() {

    setPermisosSeleccionados([]);

  }


  // =========================================================
  // GUARDAR ROL
  // =========================================================

  async function guardarRol() {

    if (!nombre.trim()) {

      setError(
        "El nombre del rol es obligatorio."
      );

      return;
    }

    try {

      setGuardando(true);
      setError(null);

      let rolId: number;

      if (modal === "crear") {

        const nuevo =
          await createRol({
            nombre,
            descripcion,
          });

        rolId =
          nuevo.id;

      } else if (
        modal === "editar" &&
        rolSeleccionado
      ) {

        await updateRol(
          rolSeleccionado.id,
          {
            nombre,
            descripcion,
          }
        );

        rolId =
          rolSeleccionado.id;

      } else {

        return;

      }

      await updateRolPermisos(
        rolId,
        permisosSeleccionados
      );

      cerrarModal();

      await cargarDatos();

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible guardar el rol."
      );

    } finally {

      setGuardando(false);

    }

  }


  // =========================================================
  // GUARDAR PERMISOS
  // =========================================================

  async function guardarPermisos() {

    if (!rolSeleccionado) {
      return;
    }

    try {

      setGuardando(true);
      setError(null);

      await updateRolPermisos(
        rolSeleccionado.id,
        permisosSeleccionados
      );

      cerrarModal();

      await cargarDatos();

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : "No fue posible actualizar los permisos."
      );

    } finally {

      setGuardando(false);

    }

  }


  // =========================================================
  // ABRIR CONFIRMACIÓN
  // =========================================================

  function abrirConfirmacion(
    rol: Rol,
    accion: Exclude<
      ConfirmationAction,
      null
    >
  ) {

    setError(null);
    setRolParaConfirmar(rol);
    setConfirmationAction(accion);

  }


  // =========================================================
  // CERRAR CONFIRMACIÓN
  // =========================================================

  function cerrarConfirmacion() {

    if (procesandoConfirmacion) {
      return;
    }

    setConfirmationAction(null);
    setRolParaConfirmar(null);

  }


  // =========================================================
  // EJECUTAR CONFIRMACIÓN
  // =========================================================

  async function ejecutarConfirmacion() {

    if (
      !rolParaConfirmar ||
      !confirmationAction
    ) {
      return;
    }

    const rol =
      rolParaConfirmar;

    const accion =
      confirmationAction;

    try {

      setProcesandoConfirmacion(true);
      setError(null);


      // =====================================================
      // ACTIVAR / DESACTIVAR
      // =====================================================

      if (
        accion === "activar" ||
        accion === "desactivar"
      ) {

        const activar =
          accion === "activar";

        setCambiandoEstado(
          rol.id
        );

        await setRolActivo(
          rol.id,
          activar
        );

        setRoles(
          (actuales) =>
            actuales.map(
              (actual) =>
                actual.id === rol.id
                  ? {
                      ...actual,
                      activo: activar,
                    }
                  : actual
            )
        );

      }


      // =====================================================
      // ELIMINAR
      // =====================================================

      if (
        accion === "eliminar"
      ) {

        await deleteRol(
          rol.id
        );

        setRoles(
          (actuales) =>
            actuales.filter(
              (actual) =>
                actual.id !== rol.id
            )
        );

      }


      cerrarConfirmacion();

    } catch (err) {

      setError(
        err instanceof Error
          ? err.message
          : obtenerMensajeError(
              accion
            )
      );

    } finally {

      setProcesandoConfirmacion(false);
      setCambiandoEstado(null);

    }

  }


  // =========================================================
  // MENSAJES DE ERROR
  // =========================================================

  function obtenerMensajeError(
    accion: Exclude<
      ConfirmationAction,
      null
    >
  ): string {

    if (accion === "activar") {
      return "No fue posible activar el rol.";
    }

    if (accion === "desactivar") {
      return "No fue posible desactivar el rol.";
    }

    return "No fue posible eliminar el rol.";

  }


  // =========================================================
  // CERRAR MODAL
  // =========================================================

  function cerrarModal() {

    setModal(null);
    setRolSeleccionado(null);
    setNombre("");
    setDescripcion("");
    setPermisosSeleccionados([]);

  }


  // =========================================================
  // PROPIEDADES DE CONFIRMACIÓN
  // =========================================================

  const confirmationContent =
    useMemo(() => {

      if (
        !confirmationAction ||
        !rolParaConfirmar
      ) {
        return null;
      }

      if (
        confirmationAction === "eliminar"
      ) {

        return {
          eyebrow: "ACCIÓN PERMANENTE",
          title: "Eliminar rol",
          description:
            `¿Estás seguro de eliminar el rol "${rolParaConfirmar.nombre}"?`,
          detail:
            "Esta acción eliminará el perfil y sus relaciones de permisos. No se puede deshacer.",
          actionLabel: "Eliminar rol",
          icon: "×",
          variant: "danger",
        };

      }

      if (
        confirmationAction === "desactivar"
      ) {

        return {
          eyebrow: "CAMBIO DE ESTADO",
          title: "Desactivar rol",
          description:
            `¿Deseas desactivar el rol "${rolParaConfirmar.nombre}"?`,
          detail:
            "El rol dejará de estar disponible como perfil activo dentro de la plataforma.",
          actionLabel: "Desactivar",
          icon: "⊘",
          variant: "warning",
        };

      }

      return {
        eyebrow: "CAMBIO DE ESTADO",
        title: "Activar rol",
        description:
          `¿Deseas activar el rol "${rolParaConfirmar.nombre}"?`,
        detail:
          "El rol volverá a estar disponible para la gestión de acceso.",
        actionLabel: "Activar",
        icon: "✓",
        variant: "success",
      };

    }, [
      confirmationAction,
      rolParaConfirmar,
    ]);


  // =========================================================
  // LOADING
  // =========================================================

  if (loading) {

    return (

      <section className="roles-page">

        <div className="roles-loading">

          <div className="roles-loading-spinner" />

          <strong>
            Cargando roles y permisos
          </strong>

          <span>
            Consultando la configuración de seguridad...
          </span>

        </div>

      </section>

    );

  }


  // =========================================================
  // VISTA
  // =========================================================

  return (

    <section className="roles-page">

      {/* HEADER */}

      <div className="roles-page-header">

        <div className="roles-title-area">

          <span className="roles-kicker">
            SEGURIDAD Y CONTROL DE ACCESO (RBAC)
          </span>

          <h1>Perfiles, Roles y Matriz de Permisos</h1>

          <p>
            Administración de políticas de autorización y asignación granular de capacidades operativas.
          </p>

        </div>

        <button
          type="button"
          className="roles-add-button"
          onClick={abrirCrear}
        >

          <span className="roles-button-icon">
            +
          </span>

          Crear nuevo rol

        </button>

      </div>

      {/* RESUMEN */}

      <div className="roles-summary">

        <div className="roles-summary-card">

          <div className="roles-summary-icon">
            ◈
          </div>

          <div>

            <span>
              Total de roles
            </span>

            <strong>
              {roles.length}
            </strong>

          </div>

        </div>


        <div className="roles-summary-card">

          <div className="roles-summary-icon roles-summary-icon-active">
            ✓
          </div>

          <div>

            <span>
              Roles activos
            </span>

            <strong>
              {rolesActivos}
            </strong>

          </div>

        </div>


        <div className="roles-summary-card">

          <div className="roles-summary-icon roles-summary-icon-inactive">
            ◯
          </div>

          <div>

            <span>
              Roles inactivos
            </span>

            <strong>
              {rolesInactivos}
            </strong>

          </div>

        </div>


        <div className="roles-summary-card">

          <div className="roles-summary-icon">
            ◫
          </div>

          <div>

            <span>
              Permisos disponibles
            </span>

            <strong>
              {permisos.length}
            </strong>

          </div>

        </div>

      </div>


      {/* ERROR */}

      {error && (

        <div className="roles-error">

          <span>
            !
          </span>

          <div>

            <strong>
              No fue posible completar la operación
            </strong>

            <p>
              {error}
            </p>

          </div>

        </div>

      )}


      {/* LISTADO */}

      <div className="roles-panel">

        <div className="roles-panel-header">

          <div>

            <span className="roles-panel-kicker">
              CONFIGURACIÓN
            </span>

            <h2>
              Perfiles de acceso
            </h2>

          </div>

          <span className="roles-count">

            {rolesFiltrados.length}{" "}

            {rolesFiltrados.length === 1
              ? "rol"
              : "roles"}

          </span>

        </div>


        <div className="roles-toolbar">

          <div className="roles-search">

            <span>
              🔍
            </span>

            <input
              type="text"
              placeholder="Buscar por nombre o descripción..."
              value={busqueda}
              onChange={(event) =>
                setBusqueda(
                  event.target.value
                )
              }
            />

            {busqueda && (

              <button
                type="button"
                onClick={() =>
                  setBusqueda("")
                }
                aria-label="Limpiar búsqueda"
              >
                ×
              </button>

            )}

          </div>

        </div>


        {/* TARJETAS DE ROLES */}

        <div className="roles-grid">

          {rolesFiltrados.length === 0 ? (

            <div className="roles-empty">

              <div className="roles-empty-icon">
                ◈
              </div>

              <h3>
                No encontramos roles
              </h3>

              <p>
                Prueba con otro término de búsqueda
                o crea un nuevo rol.
              </p>

              <button
                type="button"
                className="roles-primary-button"
                onClick={abrirCrear}
              >
                + Crear rol
              </button>

            </div>

          ) : (

            rolesFiltrados.map(
              (rol) => (

                <article
                  className="role-card"
                  key={rol.id}
                >

                  <div className="role-card-top">

                    <div className="role-avatar">

                      {rol.nombre
                        .charAt(0)
                        .toUpperCase()}

                    </div>

                    <div className="role-card-heading">

                      <h3>
                        {rol.nombre}
                      </h3>

                      <span
                        className={
                          rol.activo
                            ? "role-status role-status-active"
                            : "role-status role-status-inactive"
                        }
                      >

                        <span />

                        {rol.activo
                          ? "Activo"
                          : "Inactivo"}

                      </span>

                    </div>

                  </div>


                  <p className="role-description">

                    {rol.descripcion ||
                      "Este rol no tiene una descripción definida."}

                  </p>


                  <div className="role-card-meta">

                    <span>
                      ID #{rol.id}
                    </span>

                    <span>

                      {new Date(
                        rol.fechaCreacion
                      ).toLocaleDateString(
                        "es-CO"
                      )}

                    </span>

                  </div>


                  <div className="role-card-actions">

                    <button
                      type="button"
                      className="role-action-primary"
                      onClick={() =>
                        abrirPermisos(
                          rol
                        )
                      }
                    >
                      Administrar permisos
                    </button>


                    <div className="role-action-secondary">

                      <button
                        type="button"
                        onClick={() =>
                          abrirEditar(
                            rol
                          )
                        }
                      >
                        Editar
                      </button>


                      <button
                        type="button"
                        disabled={
                          cambiandoEstado === rol.id
                        }
                        onClick={() =>
                          abrirConfirmacion(
                            rol,
                            rol.activo
                              ? "desactivar"
                              : "activar"
                          )
                        }
                      >

                        {cambiandoEstado === rol.id
                          ? "Guardando..."
                          : rol.activo
                            ? "Desactivar"
                            : "Activar"}

                      </button>


                      <button
                        type="button"
                        className="role-action-danger"
                        onClick={() =>
                          abrirConfirmacion(
                            rol,
                            "eliminar"
                          )
                        }
                      >
                        Eliminar
                      </button>

                    </div>

                  </div>

                </article>

              )
            )

          )}

        </div>

      </div>


      {/* =====================================================
          MODAL CREAR / EDITAR
          ===================================================== */}

      {(modal === "crear" ||
        modal === "editar") && (

        <div className="roles-modal-overlay">

          <div className="roles-modal roles-modal-large">

            <div className="roles-modal-header">

              <div className="roles-modal-title">

                <div className="roles-modal-icon">

                  {modal === "crear"
                    ? "+"
                    : "✍"}

                </div>

                <div>

                  <span>

                    {modal === "crear"
                      ? "NUEVO PERFIL"
                      : "CONFIGURACIÓN"}

                  </span>

                  <h2>

                    {modal === "crear"
                      ? "Crear nuevo rol"
                      : "Editar rol"}

                  </h2>

                  <p>

                    {modal === "crear"
                      ? "Define la identidad y los permisos del nuevo perfil."
                      : "Actualiza la información y capacidades del perfil."}

                  </p>

                </div>

              </div>


              <button
                type="button"
                className="roles-modal-close"
                onClick={
                  cerrarModal
                }
              >
                ×
              </button>

            </div>


            <div className="roles-modal-content">

              <section className="roles-form-section">

                <div className="roles-section-heading">

                  <div className="roles-section-number">
                    01
                  </div>

                  <div>

                    <h3>
                      Información del rol
                    </h3>

                    <p>
                      Define los datos básicos del perfil.
                    </p>

                  </div>

                </div>


                <div className="roles-form-grid">

                  <label className="roles-field">

                    <span>

                      Nombre del rol

                      <b>*</b>

                    </span>

                    <input
                      type="text"
                      value={nombre}
                      onChange={(event) =>
                        setNombre(
                          event.target.value
                        )
                      }
                      placeholder="Ej. Supervisor de Equipos"
                    />

                    <small>
                      Utiliza un nombre claro y fácil de identificar.
                    </small>

                  </label>


                  <label className="roles-field roles-field-full">

                    <span>
                      Descripción
                    </span>

                    <textarea
                      value={descripcion}
                      onChange={(event) =>
                        setDescripcion(
                          event.target.value
                        )
                      }
                      placeholder="Describe las responsabilidades y alcance de este rol..."
                      rows={3}
                    />

                    <small>
                      Una descripción ayuda a los administradores
                      a identificar el propósito del perfil.
                    </small>

                  </label>

                </div>

              </section>


              <section className="roles-form-section">

                <div className="roles-section-heading roles-permission-heading">

                  <div className="roles-section-number">
                    02
                  </div>

                  <div>

                    <h3>
                      Permisos de acceso
                    </h3>

                    <p>
                      Selecciona las acciones que podrá realizar este rol.
                    </p>

                  </div>


                  <div className="roles-permission-counter">

                    <strong>
                      {permisosSeleccionados.length}
                    </strong>

                    <span>
                      de {permisos.length} permisos
                    </span>

                  </div>

                </div>


                <div className="roles-permission-toolbar">

                  <button
                    type="button"
                    onClick={
                      seleccionarTodos
                    }
                  >
                    Seleccionar todos
                  </button>

                  <button
                    type="button"
                    onClick={
                      limpiarPermisos
                    }
                  >
                    Limpiar selección
                  </button>

                </div>


                <div className="roles-permission-groups">

                  {permisosPorModulo.map(
                    ([
                      modulo,
                      permisosModulo,
                    ]) => {

                      const seleccionadosModulo =
                        permisosModulo.filter(
                          (permiso) =>
                            permisosSeleccionados.includes(
                              permiso.id
                            )
                        ).length;


                      const todosModulo =
                        seleccionadosModulo ===
                        permisosModulo.length;


                      return (

                        <div
                          className="roles-permission-group"
                          key={modulo}
                        >

                          <div className="roles-permission-group-header">

                            <div>

                              <span className="roles-module-icon">
                                ◫
                              </span>

                              <div>

                                <h4>
                                  {modulo}
                                </h4>

                                <span>
                                  {seleccionadosModulo} de{" "}
                                  {permisosModulo.length} seleccionados
                                </span>

                              </div>

                            </div>


                            <button
                              type="button"
                              onClick={() =>
                                seleccionarModulo(
                                  permisosModulo
                                )
                              }
                            >

                              {todosModulo
                                ? "Quitar todos"
                                : "Seleccionar todos"}

                            </button>

                          </div>


                          <div className="roles-permission-grid">

                            {permisosModulo.map(
                              (permiso) => {

                                const seleccionado =
                                  permisosSeleccionados.includes(
                                    permiso.id
                                  );


                                return (

                                  <label
                                    className={
                                      seleccionado
                                        ? "role-permission-card selected"
                                        : "role-permission-card"
                                    }
                                    key={
                                      permiso.id
                                    }
                                  >

                                    <input
                                      type="checkbox"
                                      checked={
                                        seleccionado
                                      }
                                      onChange={() =>
                                        togglePermiso(
                                          permiso.id
                                        )
                                      }
                                    />


                                    <span className="permission-check">
                                      ✓
                                    </span>


                                    <span className="permission-content">

                                      <strong>
                                        {getPermissionLabel(
                                          permiso.nombre
                                        )}
                                      </strong>

                                    </span>

                                  </label>

                                );

                              }
                            )}

                          </div>

                        </div>

                      );

                    }
                  )}

                </div>

              </section>

            </div>


            <div className="roles-modal-footer">

              <div className="roles-footer-info">

                <span className="roles-footer-dot" />

                <span>
                  Los cambios se guardarán inmediatamente.
                </span>

              </div>


              <div className="roles-footer-actions">

                <button
                  type="button"
                  className="roles-cancel-button"
                  onClick={
                    cerrarModal
                  }
                >
                  Cancelar
                </button>


                <button
                  type="button"
                  className="roles-primary-button"
                  disabled={
                    guardando
                  }
                  onClick={
                    guardarRol
                  }
                >

                  {guardando
                    ? "Guardando..."
                    : modal === "crear"
                      ? "Crear rol"
                      : "Guardar cambios"}

                </button>

              </div>

            </div>

          </div>

        </div>

      )}


      {/* =====================================================
          MODAL PERMISOS
          ===================================================== */}

      {modal === "permisos" && (

        <div className="roles-modal-overlay">

          <div className="roles-modal roles-modal-large">

            <div className="roles-modal-header">

              <div className="roles-modal-title">

                <div className="roles-modal-icon">
                  ◫
                </div>

                <div>

                  <span>
                    CONTROL DE ACCESO
                  </span>

                  <h2>
                    Administrar permisos
                  </h2>

                  <p>

                    Configura las capacidades de{" "}

                    <strong>
                      {rolSeleccionado?.nombre}
                    </strong>.

                  </p>

                </div>

              </div>


              <button
                type="button"
                className="roles-modal-close"
                onClick={
                  cerrarModal
                }
              >
                ×
              </button>

            </div>


            <div className="roles-modal-content">

              <section className="roles-form-section">

                <div className="roles-section-heading roles-permission-heading">

                  <div className="roles-section-number">
                    01
                  </div>

                  <div>

                    <h3>
                      Permisos disponibles
                    </h3>

                    <p>
                      Activa o desactiva las capacidades
                      de este perfil.
                    </p>

                  </div>


                  <div className="roles-permission-counter">

                    <strong>
                      {permisosSeleccionados.length}
                    </strong>

                    <span>
                      seleccionados
                    </span>

                  </div>

                </div>


                <div className="roles-permission-toolbar">

                  <button
                    type="button"
                    onClick={
                      seleccionarTodos
                    }
                  >
                    Seleccionar todos
                  </button>

                  <button
                    type="button"
                    onClick={
                      limpiarPermisos
                    }
                  >
                    Limpiar selección
                  </button>

                </div>


                <div className="roles-permission-groups">

                  {permisosPorModulo.map(
                    ([
                      modulo,
                      permisosModulo,
                    ]) => {

                      const seleccionadosModulo =
                        permisosModulo.filter(
                          (permiso) =>
                            permisosSeleccionados.includes(
                              permiso.id
                            )
                        ).length;


                      const todosModulo =
                        seleccionadosModulo ===
                        permisosModulo.length;


                      return (

                        <div
                          className="roles-permission-group"
                          key={modulo}
                        >

                          <div className="roles-permission-group-header">

                            <div>

                              <span className="roles-module-icon">
                                ◫
                              </span>

                              <div>

                                <h4>
                                  {modulo}
                                </h4>

                                <span>
                                  {seleccionadosModulo} de{" "}
                                  {permisosModulo.length} seleccionados
                                </span>

                              </div>

                            </div>


                            <button
                              type="button"
                              onClick={() =>
                                seleccionarModulo(
                                  permisosModulo
                                )
                              }
                            >

                              {todosModulo
                                ? "Quitar todos"
                                : "Seleccionar todos"}

                            </button>

                          </div>


                          <div className="roles-permission-grid">

                            {permisosModulo.map(
                              (permiso) => {

                                const seleccionado =
                                  permisosSeleccionados.includes(
                                    permiso.id
                                  );


                                return (

                                  <label
                                    className={
                                      seleccionado
                                        ? "role-permission-card selected"
                                        : "role-permission-card"
                                    }
                                    key={
                                      permiso.id
                                    }
                                  >

                                    <input
                                      type="checkbox"
                                      checked={
                                        seleccionado
                                      }
                                      onChange={() =>
                                        togglePermiso(
                                          permiso.id
                                        )
                                      }
                                    />


                                    <span className="permission-check">
                                      ✓
                                    </span>


                                    <span className="permission-content">

                                      <strong>
                                        {getPermissionLabel(
                                          permiso.nombre
                                        )}
                                      </strong>

                                    </span>

                                  </label>

                                );

                              }
                            )}

                          </div>

                        </div>

                      );

                    }
                  )}

                </div>

              </section>

            </div>


            <div className="roles-modal-footer">

              <div className="roles-footer-info">

                <span className="roles-footer-dot" />

                <span>
                  Los cambios se guardarán inmediatamente.
                </span>

              </div>


              <div className="roles-footer-actions">

                <button
                  type="button"
                  className="roles-cancel-button"
                  onClick={
                    cerrarModal
                  }
                >
                  Cancelar
                </button>


                <button
                  type="button"
                  className="roles-primary-button"
                  disabled={
                    guardando
                  }
                  onClick={
                    guardarPermisos
                  }
                >

                  {guardando
                    ? "Guardando..."
                    : "Guardar permisos"}

                </button>

              </div>

            </div>

          </div>

        </div>

      )}


      {/* =====================================================
          MODAL DE CONFIRMACIÓN
          ===================================================== */}

      {confirmationContent &&
        rolParaConfirmar && (

        <div
          className="roles-confirmation-overlay"
          onMouseDown={(event) => {

            if (
              event.target ===
              event.currentTarget &&
              !procesandoConfirmacion
            ) {
              cerrarConfirmacion();
            }

          }}
        >

          <div
            className={
              `roles-confirmation-modal roles-confirmation-${confirmationContent.variant}`
            }
            role="dialog"
            aria-modal="true"
            aria-labelledby="roles-confirmation-title"
          >

            <div className="roles-confirmation-icon">

              {confirmationContent.icon}

            </div>


            <div className="roles-confirmation-content">

              <span className="roles-confirmation-eyebrow">

                {confirmationContent.eyebrow}

              </span>


              <h2 id="roles-confirmation-title">

                {confirmationContent.title}

              </h2>


              <p className="roles-confirmation-description">

                {confirmationContent.description}

              </p>


              <div className="roles-confirmation-detail">

                <span className="roles-confirmation-detail-icon">
                  i
                </span>

                <span>
                  {confirmationContent.detail}
                </span>

              </div>

            </div>


            <div className="roles-confirmation-actions">

              <button
                type="button"
                className="roles-confirmation-cancel"
                disabled={
                  procesandoConfirmacion
                }
                onClick={
                  cerrarConfirmacion
                }
              >
                Cancelar
              </button>


              <button
                type="button"
                className={
                  `roles-confirmation-submit roles-confirmation-submit-${confirmationContent.variant}`
                }
                disabled={
                  procesandoConfirmacion
                }
                onClick={
                  ejecutarConfirmacion
                }
              >

                {procesandoConfirmacion
                  ? "Procesando..."
                  : confirmationContent.actionLabel}

              </button>

            </div>

          </div>

        </div>

      )}

    </section>

  );
}


