import { useEffect, useMemo, useState } from "react";
import {
  getUsuarios,
  createUsuario,
  updateUsuario,
  assignUsuarioRol,
  resetUsuarioPassword,
} from "./usuarioService";
import type { Usuario } from "./usuarioService";
import { getRoles } from "../roles/rolService";
import type { Rol } from "../roles/rolService";
import {
  SearchIcon,
  RefreshIcon,
  PlusIcon,
  EditIcon,
  KeyIcon,
  CloseIcon,
  ShieldIcon,
  UsersIcon,
  MoreVerticalIcon,
  MailIcon,
  UserCheckIcon,
} from "@/components/ui/Icons";
import { AutoDismissAlert } from "@/components/ui/AutoDismissAlert";
import { Modal } from "@/components/ui/Modal";
import { Pagination } from "@/components/ui/Pagination";

const ITEMS_PER_PAGE = 15;

export function UsuariosPage() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [roles, setRoles] = useState<Rol[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mensaje, setMensaje] = useState<string | null>(null);

  // Filters
  const [busqueda, setBusqueda] = useState("");
  const [estadoFiltro, setEstadoFiltro] = useState<"todos" | "activos" | "inactivos">("todos");
  const [procesoFiltro, setProcesoFiltro] = useState<string>("todos");
  const [paginaActual, setPaginaActual] = useState(1);
  const [menuAbiertoId, setMenuAbiertoId] = useState<number | null>(null);

  // Modal Usuario Crear / Editar
  const [modalAbierto, setModalAbierto] = useState(false);
  const [modoFormulario, setModoFormulario] = useState<"crear" | "editar">("crear");
  const [usuarioSeleccionado, setUsuarioSeleccionado] = useState<Usuario | null>(null);
  const [formulario, setFormulario] = useState({
    nombre: "",
    apellido: "",
    usuarioRed: "",
    correo: "",
    proceso: "General",
    activo: true,
    password: "",
    rolId: "",
  });
  const [guardando, setGuardando] = useState(false);

  // Modal Asignar Rol
  const [rolModalAbierto, setRolModalAbierto] = useState(false);
  const [usuarioRolSeleccionado, setUsuarioRolSeleccionado] = useState<Usuario | null>(null);
  const [rolSeleccionadoId, setRolSeleccionadoId] = useState<number | string>("");
  const [guardandoRol, setGuardandoRol] = useState(false);

  // Modal Restablecer Contraseña (Admin)
  const [modalResetAbierto, setModalResetAbierto] = useState(false);
  const [usuarioReset, setUsuarioReset] = useState<Usuario | null>(null);
  const [nuevaPassword, setNuevaPassword] = useState("");
  const [confirmarPassword, setConfirmarPassword] = useState("");
  const [guardandoPassword, setGuardandoPassword] = useState(false);

  // Close dropdown menu on outside click or escape
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (!(e.target as HTMLElement).closest(".user-actions-dropdown")) {
        setMenuAbiertoId(null);
      }
    }
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setMenuAbiertoId(null);
    }
    document.addEventListener("click", handleClickOutside);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("click", handleClickOutside);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, []);

  async function cargarDatos() {
    try {
      setLoading(true);
      setError(null);
      const [uData, rData] = await Promise.all([getUsuarios(), getRoles()]);
      setUsuarios(uData);
      setRoles(rData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los usuarios y roles.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    cargarDatos();
  }, []);

  const procesosDisponibles = useMemo(() => {
    return Array.from(new Set(usuarios.map((u) => u.proceso).filter(Boolean))).sort();
  }, [usuarios]);

  const usuariosFiltrados = useMemo(() => {
    const term = busqueda.trim().toLowerCase();
    return usuarios.filter((u) => {
      const coincideBusqueda =
        term === "" ||
        u.nombre.toLowerCase().includes(term) ||
        u.apellido.toLowerCase().includes(term) ||
        u.usuarioRed.toLowerCase().includes(term) ||
        u.correo.toLowerCase().includes(term);

      const coincideEstado =
        estadoFiltro === "todos" ||
        (estadoFiltro === "activos" ? u.activo : !u.activo);

      const coincideProceso =
        procesoFiltro === "todos" || u.proceso === procesoFiltro;

      return coincideBusqueda && coincideEstado && coincideProceso;
    });
  }, [usuarios, busqueda, estadoFiltro, procesoFiltro]);

  const totalPaginas = Math.max(1, Math.ceil(usuariosFiltrados.length / ITEMS_PER_PAGE));
  const usuariosPaginados = useMemo(() => {
    const start = (paginaActual - 1) * ITEMS_PER_PAGE;
    return usuariosFiltrados.slice(start, start + ITEMS_PER_PAGE);
  }, [usuariosFiltrados, paginaActual]);

  function abrirModalCrear() {
    setModoFormulario("crear");
    setUsuarioSeleccionado(null);
    setFormulario({
      nombre: "",
      apellido: "",
      usuarioRed: "",
      correo: "",
      proceso: "Tecnología",
      activo: true,
      password: "",
      rolId: "",
    });
    setModalAbierto(true);
  }

  function abrirModalEditar(u: Usuario) {
    setModoFormulario("editar");
    setUsuarioSeleccionado(u);
    setFormulario({
      nombre: u.nombre,
      apellido: u.apellido,
      usuarioRed: u.usuarioRed,
      correo: u.correo,
      proceso: u.proceso || "General",
      activo: u.activo,
      password: "",
      rolId: u.rolId ? String(u.rolId) : "",
    });
    setModalAbierto(true);
  }

  function abrirModalRol(u: Usuario) {
    setUsuarioRolSeleccionado(u);
    setRolSeleccionadoId(u.rolId ?? "");
    setRolModalAbierto(true);
  }

  function abrirModalReset(u: Usuario) {
    setUsuarioReset(u);
    setNuevaPassword("");
    setConfirmarPassword("");
    setModalResetAbierto(true);
  }

  async function handleGuardarUsuario(e: React.FormEvent) {
    e.preventDefault();
    if (!formulario.nombre || !formulario.apellido || !formulario.usuarioRed || !formulario.correo) {
      setError("Todos los campos con asterisco (*) son obligatorios.");
      return;
    }

    try {
      setGuardando(true);
      setError(null);
      if (modoFormulario === "crear") {
        await createUsuario({
          nombre: formulario.nombre.trim(),
          apellido: formulario.apellido.trim(),
          usuarioRed: formulario.usuarioRed.trim(),
          correo: formulario.correo.trim(),
          proceso: formulario.proceso.trim(),
          password: formulario.password.trim() || undefined,
          rolId: formulario.rolId ? Number(formulario.rolId) : undefined,
        });
        setMensaje(`Usuario "${formulario.usuarioRed}" registrado con éxito.`);
      } else if (usuarioSeleccionado) {
        await updateUsuario(usuarioSeleccionado.id, {
          nombre: formulario.nombre.trim(),
          apellido: formulario.apellido.trim(),
          usuarioRed: formulario.usuarioRed.trim(),
          correo: formulario.correo.trim(),
          proceso: formulario.proceso.trim(),
          activo: formulario.activo,
        });
        setMensaje(`Usuario "${formulario.usuarioRed}" actualizado con éxito.`);
      }
      setModalAbierto(false);
      await cargarDatos();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al guardar el usuario.");
    } finally {
      setGuardando(false);
    }
  }

  async function handleGuardarRol(e: React.FormEvent) {
    e.preventDefault();
    if (!usuarioRolSeleccionado) return;

    try {
      setGuardandoRol(true);
      setError(null);
      const rolIdNum = rolSeleccionadoId ? Number(rolSeleccionadoId) : null;
      await assignUsuarioRol(usuarioRolSeleccionado.id, rolIdNum);
      setMensaje(`Rol asignado correctamente a "${usuarioRolSeleccionado.nombre}".`);
      setRolModalAbierto(false);
      await cargarDatos();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al asignar rol.");
    } finally {
      setGuardandoRol(false);
    }
  }

  async function handleConfirmarResetPassword(e: React.FormEvent) {
    e.preventDefault();
    if (!usuarioReset) return;

    if (nuevaPassword !== confirmarPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    if (nuevaPassword.length < 8) {
      setError("La nueva contraseña debe tener al menos 8 caracteres.");
      return;
    }

    try {
      setGuardandoPassword(true);
      setError(null);
      await resetUsuarioPassword(usuarioReset.id, nuevaPassword, confirmarPassword);
      setMensaje(`Contraseña restablecida exitosamente para el colaborador "${usuarioReset.nombre} ${usuarioReset.apellido}".`);
      setModalResetAbierto(false);
      setUsuarioReset(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al restablecer la contraseña.");
    } finally {
      setGuardandoPassword(false);
    }
  }

  async function handleToggleActivo(u: Usuario) {
    if (u.usuarioRed.toLowerCase() === "admin" && u.activo) {
      setError("Acción Bloqueada: El usuario principal 'admin' no puede ser desactivado para evitar el bloqueo del sistema.");
      return;
    }

    const accion = u.activo ? "desactivar" : "activar";
    if (!confirm(`¿Estás seguro de que deseas ${accion} al usuario ${u.nombre} ${u.apellido}?`)) {
      return;
    }

    try {
      setError(null);
      await updateUsuario(u.id, {
        nombre: u.nombre,
        apellido: u.apellido,
        usuarioRed: u.usuarioRed,
        correo: u.correo,
        proceso: u.proceso,
        activo: !u.activo,
      });
      setMensaje(`Usuario "${u.usuarioRed}" ${u.activo ? "desactivado" : "activado"} correctamente.`);
      await cargarDatos();
    } catch (err) {
      setError(err instanceof Error ? err.message : `Error al ${accion} el usuario.`);
    }
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando directorio de colaboradores...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="page-header-container" style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            ADMINISTRACIÓN DE USUARIOS
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Directorio de Colaboradores
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Gestión de cuentas, perfiles corporativos, asignación de roles y control de credenciales.
          </p>
        </div>

        <div className="page-header-actions">
          <button type="button" className="btn btn-secondary btn-sm" onClick={cargarDatos} style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
            <RefreshIcon size={14} />
            <span>Actualizar</span>
          </button>
          <button type="button" className="btn btn-primary btn-sm" onClick={abrirModalCrear} style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
            <PlusIcon size={14} />
            <span>Nuevo usuario</span>
          </button>
        </div>
      </div>

      <AutoDismissAlert
        type="success"
        message={mensaje}
        duration={4500}
        onDismiss={() => setMensaje(null)}
      />

      <AutoDismissAlert
        type="error"
        message={error}
        duration={5000}
        onDismiss={() => setError(null)}
      />

      {/* Summary KPI Badges */}
      <div className="kpi-grid-strip">
        <div className="card" style={{ padding: "12px 16px", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>Total Colaboradores</span>
            <div style={{ fontSize: "20px", fontWeight: 800, color: "var(--text-primary)" }}>{usuarios.length}</div>
          </div>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <UsersIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "12px 16px", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>Activos</span>
            <div style={{ fontSize: "20px", fontWeight: 800, color: "var(--color-success-700)" }}>{usuarios.filter((u) => u.activo).length}</div>
          </div>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-success-50)", color: "var(--color-success-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <UserCheckIcon size={18} />
          </div>
        </div>

        <div className="card" style={{ padding: "12px 16px", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div>
            <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>Roles Configurados</span>
            <div style={{ fontSize: "20px", fontWeight: 800, color: "var(--color-info-700)" }}>{roles.length}</div>
          </div>
          <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-info-50)", color: "var(--color-info-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <ShieldIcon size={18} />
          </div>
        </div>
      </div>

      {/* Toolbar Filters */}
      <div className="toolbar-container" style={{ background: 'var(--bg-surface)', padding: '14px 18px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--border-default)', marginBottom: '18px', display: 'flex', gap: '12px', flexWrap: 'wrap', alignItems: 'center' }}>
        <div className="search-input-wrapper" style={{ flex: 1, minWidth: '220px' }}>
          <span className="search-icon">
            <SearchIcon size={16} />
          </span>
          <input
            id="usuariosSearchInput"
            name="usuariosSearch"
            className="form-input"
            type="text"
            placeholder="Buscar por nombre, usuario de red o correo..."
            value={busqueda}
            onChange={(e) => {
              setBusqueda(e.target.value);
              setPaginaActual(1);
            }}
          />
        </div>

        <div className="toolbar-filter-group" style={{ display: "flex", gap: "10px", alignItems: "center", flexWrap: "wrap" }}>
          <select
            id="usuariosEstadoFiltro"
            name="estadoFiltro"
            className="form-select"
            value={estadoFiltro}
            onChange={(e) => {
              setEstadoFiltro(e.target.value as "todos" | "activos" | "inactivos");
              setPaginaActual(1);
            }}
          >
            <option value="todos">Todos los estados</option>
            <option value="activos">Activos</option>
            <option value="inactivos">Inactivos</option>
          </select>

          <select
            id="usuariosProcesoFiltro"
            name="procesoFiltro"
            className="form-select"
            value={procesoFiltro}
            onChange={(e) => {
              setProcesoFiltro(e.target.value);
              setPaginaActual(1);
            }}
          >
            <option value="todos">Todos los departamentos</option>
            {procesosDisponibles.map((p) => (
              <option key={p} value={p}>
                {p}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Tabla de Usuarios */}
      <div className="table-wrapper" style={{ minHeight: "380px" }}>
        <table className="data-table">
          <thead>
            <tr>
              <th>Colaborador</th>
              <th>Correo Electrónico</th>
              <th>Departamento / Proceso</th>
              <th>Rol Asignado</th>
              <th>Estado</th>
              <th style={{ textAlign: "right", paddingRight: "20px" }}>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {usuariosPaginados.length === 0 ? (
              <tr>
                <td colSpan={6} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
                  No se encontraron usuarios coincidentes.
                </td>
              </tr>
            ) : (
              usuariosPaginados.map((u) => {
                const esAdmin = u.rolNombre?.toLowerCase().includes("admin");
                const esMenuAbierto = menuAbiertoId === u.id;

                return (
                  <tr key={u.id}>
                    <td className="primary-cell">
                      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
                        <div
                          style={{
                            width: "36px",
                            height: "36px",
                            borderRadius: "50%",
                            background: esAdmin ? "var(--color-primary-100)" : "var(--color-slate-100)",
                            color: esAdmin ? "var(--color-primary-700)" : "var(--color-slate-700)",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            fontSize: "12px",
                            fontWeight: 700,
                            flexShrink: 0,
                          }}
                        >
                          {u.nombre.charAt(0)}{u.apellido.charAt(0)}
                        </div>
                        <div style={{ fontWeight: 700, color: "var(--text-primary)", fontSize: "13.5px" }}>
                          {u.nombre} {u.apellido}
                        </div>
                      </div>
                    </td>
                    <td style={{ color: "var(--text-secondary)", fontSize: "13px" }}>
                      <div style={{ display: "flex", alignItems: "center", gap: "6px" }}>
                        <MailIcon size={14} style={{ color: "var(--text-muted)", flexShrink: 0 }} />
                        <span>{u.correo}</span>
                      </div>
                    </td>
                    <td style={{ fontSize: "13px", color: "var(--text-secondary)" }}>
                      {u.proceso || "General"}
                    </td>
                    <td>
                      <span
                        className={
                          u.rolNombre
                            ? esAdmin
                              ? "badge badge-primary"
                              : "badge badge-info"
                            : "badge badge-neutral"
                        }
                        style={{ fontWeight: 600 }}
                      >
                        {u.rolNombre ?? "Sin Rol"}
                      </span>
                    </td>
                    <td>
                      <span className={u.activo ? "badge badge-success" : "badge badge-danger"}>
                        <span className="badge-dot" />
                        {u.activo ? "Activo" : "Inactivo"}
                      </span>
                    </td>
                    <td style={{ textAlign: "right", paddingRight: "16px" }}>
                      <div className="user-actions-dropdown" style={{ display: "inline-flex", alignItems: "center", gap: "6px", position: "relative" }}>
                        {/* Botón rápido de edición */}
                        <button
                          type="button"
                          className="btn btn-secondary btn-sm"
                          onClick={() => abrirModalEditar(u)}
                          style={{ display: "inline-flex", alignItems: "center", gap: "4px", padding: "5px 10px" }}
                          title="Editar datos del colaborador"
                        >
                          <EditIcon size={13} />
                          <span>Editar</span>
                        </button>

                        {/* Botón de más opciones */}
                        <button
                          type="button"
                          className={`btn btn-secondary btn-sm ${esMenuAbierto ? "active" : ""}`}
                          onClick={(e) => {
                            e.stopPropagation();
                            setMenuAbiertoId(esMenuAbierto ? null : u.id);
                          }}
                          style={{ padding: "5px 7px", display: "inline-flex", alignItems: "center", justifyContent: "center" }}
                          title="Más opciones de gestión"
                          aria-haspopup="true"
                          aria-expanded={esMenuAbierto}
                        >
                          <MoreVerticalIcon size={14} />
                        </button>

                        {/* Menú flotante de acciones */}
                        {esMenuAbierto && (
                          <div
                            style={{
                              position: "absolute",
                              right: 0,
                              top: "calc(100% + 4px)",
                              background: "#ffffff",
                              border: "1px solid var(--border-default)",
                              borderRadius: "var(--radius-md)",
                              boxShadow: "0 10px 25px -5px rgba(0,0,0,0.12), 0 0 1px rgba(0,0,0,0.1)",
                              zIndex: 60,
                              minWidth: "190px",
                              padding: "4px",
                              display: "flex",
                              flexDirection: "column",
                              gap: "2px",
                              textAlign: "left",
                            }}
                            onClick={(e) => e.stopPropagation()}
                          >
                            <button
                              type="button"
                              onClick={() => {
                                setMenuAbiertoId(null);
                                abrirModalRol(u);
                              }}
                              style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "8px",
                                width: "100%",
                                padding: "7px 10px",
                                fontSize: "12.5px",
                                border: "none",
                                background: "none",
                                borderRadius: "4px",
                                cursor: "pointer",
                                color: "var(--text-primary)",
                                transition: "background 0.15s ease",
                              }}
                              onMouseEnter={(e) => (e.currentTarget.style.background = "var(--color-slate-100)")}
                              onMouseLeave={(e) => (e.currentTarget.style.background = "none")}
                            >
                              <ShieldIcon size={14} style={{ color: "var(--color-primary-600)" }} />
                              <span>Asignar / Cambiar Rol</span>
                            </button>

                            <button
                              type="button"
                              onClick={() => {
                                setMenuAbiertoId(null);
                                abrirModalReset(u);
                              }}
                              style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "8px",
                                width: "100%",
                                padding: "7px 10px",
                                fontSize: "12.5px",
                                border: "none",
                                background: "none",
                                borderRadius: "4px",
                                cursor: "pointer",
                                color: "var(--text-primary)",
                                transition: "background 0.15s ease",
                              }}
                              onMouseEnter={(e) => (e.currentTarget.style.background = "var(--color-slate-100)")}
                              onMouseLeave={(e) => (e.currentTarget.style.background = "none")}
                            >
                              <KeyIcon size={14} style={{ color: "var(--color-warning-600)" }} />
                              <span>Restablecer Clave</span>
                            </button>

                            <div style={{ height: "1px", background: "var(--border-subtle)", margin: "3px 0" }} />

                            <button
                              type="button"
                              onClick={() => {
                                setMenuAbiertoId(null);
                                handleToggleActivo(u);
                              }}
                              style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "8px",
                                width: "100%",
                                padding: "7px 10px",
                                fontSize: "12.5px",
                                border: "none",
                                background: "none",
                                borderRadius: "4px",
                                cursor: "pointer",
                                color: u.activo ? "var(--color-danger-700)" : "var(--color-success-700)",
                                transition: "background 0.15s ease",
                              }}
                              onMouseEnter={(e) => (e.currentTarget.style.background = u.activo ? "var(--color-danger-50)" : "var(--color-success-50)")}
                              onMouseLeave={(e) => (e.currentTarget.style.background = "none")}
                            >
                              <UserCheckIcon size={14} />
                              <span>{u.activo ? "Desactivar Colaborador" : "Activar Colaborador"}</span>
                            </button>
                          </div>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Paginación */}
      <Pagination
        currentPage={paginaActual}
        totalPages={totalPaginas}
        totalItems={usuariosFiltrados.length}
        currentCount={usuariosPaginados.length}
        itemName="usuarios"
        onPageChange={setPaginaActual}
      />

      {/* Modal Crear / Editar Usuario */}
      {modalAbierto && (
        <Modal isOpen={modalAbierto} onClose={() => setModalAbierto(false)}>
          <div className="modal-header">
              <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
                <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-700)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                  <UsersIcon size={18} />
                </div>
                <div>
                  <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>DIRECTORIO</span>
                  <h2 style={{ margin: "1px 0 0" }}>{modoFormulario === "crear" ? "Registrar Nuevo Usuario" : "Editar Usuario"}</h2>
                </div>
              </div>
              <button type="button" className="modal-close" onClick={() => setModalAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleGuardarUsuario}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "14px" }}>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioNombreInput">Nombre *</label>
                    <input
                      id="usuarioNombreInput"
                      name="nombre"
                      type="text"
                      className="form-input"
                      required
                      placeholder="Ej. Juan"
                      value={formulario.nombre}
                      onChange={(e) => setFormulario({ ...formulario, nombre: e.target.value })}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioApellidoInput">Apellido *</label>
                    <input
                      id="usuarioApellidoInput"
                      name="apellido"
                      type="text"
                      className="form-input"
                      required
                      placeholder="Ej. Pérez"
                      value={formulario.apellido}
                      onChange={(e) => setFormulario({ ...formulario, apellido: e.target.value })}
                    />
                  </div>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioRedInput">Usuario de Red / Login *</label>
                    <input
                      id="usuarioRedInput"
                      name="usuarioRed"
                      type="text"
                      className="form-input"
                      required
                      disabled={modoFormulario === "editar" && usuarioSeleccionado?.usuarioRed.toLowerCase() === "admin"}
                      placeholder="Ej. juan.perez"
                      value={formulario.usuarioRed}
                      onChange={(e) => setFormulario({ ...formulario, usuarioRed: e.target.value })}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioCorreoInput">Correo Electrónico *</label>
                    <input
                      id="usuarioCorreoInput"
                      name="correo"
                      type="email"
                      className="form-input"
                      required
                      placeholder="ejemplo@empresa.com"
                      value={formulario.correo}
                      onChange={(e) => setFormulario({ ...formulario, correo: e.target.value })}
                    />
                  </div>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioProcesoInput">Proceso / Departamento</label>
                    <input
                      id="usuarioProcesoInput"
                      name="proceso"
                      type="text"
                      className="form-input"
                      placeholder="Ej. Tecnología, Operaciones..."
                      value={formulario.proceso}
                      onChange={(e) => setFormulario({ ...formulario, proceso: e.target.value })}
                    />
                  </div>
                  {modoFormulario === "crear" && (
                    <div className="form-group">
                      <label className="form-label" htmlFor="usuarioRolSelect">Rol Inicial</label>
                      <select
                        id="usuarioRolSelect"
                        name="rolId"
                        className="form-select"
                        value={formulario.rolId}
                        onChange={(e) => setFormulario({ ...formulario, rolId: e.target.value })}
                      >
                        <option value="">-- Sin Rol Inicial --</option>
                        {roles.map((r) => (
                          <option key={r.id} value={r.id}>
                            {r.nombre}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>

                {modoFormulario === "crear" && (
                  <div className="form-group">
                    <label className="form-label" htmlFor="usuarioPasswordInput">Contraseña Inicial (Opcional)</label>
                    <input
                      id="usuarioPasswordInput"
                      name="password"
                      type="text"
                      className="form-input"
                      placeholder="Dejar en blanco para asignar: Actas2026*"
                      value={formulario.password}
                      onChange={(e) => setFormulario({ ...formulario, password: e.target.value })}
                    />
                    <span style={{ fontSize: "11px", color: "var(--text-muted)", marginTop: "4px", display: "block" }}>
                      Si se deja en blanco, la contraseña genérica inicial será <strong>Actas2026*</strong>.
                    </span>
                  </div>
                )}
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={() => setModalAbierto(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={guardando}>
                  {guardando ? "Guardando..." : modoFormulario === "crear" ? "Crear Usuario" : "Guardar Cambios"}
                </button>
              </div>
            </form>
        </Modal>
      )}

      {/* Modal Asignar Rol */}
      {rolModalAbierto && usuarioRolSeleccionado && (
        <Modal isOpen={rolModalAbierto} onClose={() => setRolModalAbierto(false)}>
          <div className="modal-header">
              <div>
                <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>SEGURIDAD RBAC</span>
                <h2>Asignar Rol: {usuarioRolSeleccionado.nombre} {usuarioRolSeleccionado.apellido}</h2>
              </div>
              <button type="button" className="modal-close" onClick={() => setRolModalAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleGuardarRol}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "14px" }}>
                <p style={{ fontSize: "13px", color: "var(--text-secondary)", margin: 0 }}>
                  Selecciona el perfil de acceso y permisos para este colaborador:
                </p>

                <div className="form-group">
                  <label className="form-label" htmlFor="rolAsignarSelect">Rol del Sistema *</label>
                  <select
                    id="rolAsignarSelect"
                    name="rolId"
                    className="form-select"
                    required
                    value={rolSeleccionadoId}
                    onChange={(e) => setRolSeleccionadoId(e.target.value)}
                  >
                    <option value="">-- Seleccionar Rol --</option>
                    {roles.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.nombre} ({r.descripcion || "Sin descripción"})
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={() => setRolModalAbierto(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={guardandoRol || !rolSeleccionadoId}>
                  {guardandoRol ? "Asignando..." : "Guardar Rol"}
                </button>
              </div>
            </form>
        </Modal>
      )}

      {/* Modal Restablecer Contraseña (Admin) */}
      {modalResetAbierto && usuarioReset && (
        <Modal isOpen={modalResetAbierto} onClose={() => setModalResetAbierto(false)} size="small">
          <div className="modal-header">
              <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
                <div style={{ width: "34px", height: "34px", borderRadius: "var(--radius-md)", background: "var(--color-primary-50)", color: "var(--color-primary-700)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                  <KeyIcon size={18} />
                </div>
                <div>
                  <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--color-primary-600)", textTransform: "uppercase" }}>SEGURIDAD DE ACCESO</span>
                  <h2 style={{ margin: "1px 0 0", fontSize: "17px" }}>Restablecer Contraseña</h2>
                </div>
              </div>
              <button type="button" className="modal-close" onClick={() => setModalResetAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleConfirmarResetPassword}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "14px", padding: "20px 24px" }}>
                <p style={{ fontSize: "13px", color: "var(--text-secondary)", margin: 0 }}>
                  Establecer una nueva contraseña de acceso para <strong>{usuarioReset.nombre} {usuarioReset.apellido}</strong> ({usuarioReset.usuarioRed}):
                </p>

                <div className="form-group">
                  <label className="form-label" htmlFor="resetNuevaPasswordInput">Nueva Contraseña *</label>
                  <input
                    id="resetNuevaPasswordInput"
                    name="nuevaPassword"
                    type="password"
                    className="form-input"
                    required
                    placeholder="Mínimo 8 caracteres, mayúscula, minúscula, número..."
                    value={nuevaPassword}
                    onChange={(e) => setNuevaPassword(e.target.value)}
                  />
                </div>

                <div className="form-group">
                  <label className="form-label" htmlFor="resetConfirmarPasswordInput">Confirmar Contraseña *</label>
                  <input
                    id="resetConfirmarPasswordInput"
                    name="confirmarPassword"
                    type="password"
                    className="form-input"
                    required
                    placeholder="Repite la nueva contraseña..."
                    value={confirmarPassword}
                    onChange={(e) => setConfirmarPassword(e.target.value)}
                  />
                </div>

                <div style={{ background: "var(--color-slate-50)", border: "1px solid var(--border-default)", borderRadius: "var(--radius-md)", padding: "10px 12px", fontSize: "11.5px", color: "var(--text-secondary)" }}>
                  La contraseña debe tener al menos 8 caracteres e incluir mayúsculas, minúsculas, números y un símbolo.
                </div>
              </div>

              <div className="modal-footer" style={{ borderTop: "1px solid var(--border-default)", padding: "14px 24px" }}>
                <button type="button" className="btn btn-ghost" onClick={() => setModalResetAbierto(false)} disabled={guardandoPassword}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={guardandoPassword || !nuevaPassword}>
                  {guardandoPassword ? "Guardando..." : "Actualizar Contraseña"}
                </button>
              </div>
            </form>
        </Modal>
      )}
    </div>
  );
}
