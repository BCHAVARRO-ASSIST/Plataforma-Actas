import { useEffect, useMemo, useState } from "react";
import {
  getRoles,
  createRol,
  updateRol,
  getPermisos,
  getRol,
  updateRolPermisos,
  deleteRol,
} from "./rolService";
import type { Rol, Permiso } from "./rolService";
import {
  SearchIcon,
  RefreshIcon,
  PlusIcon,
  EditIcon,
  KeyIcon,
  CloseIcon,
  RolesIcon,
  PackageIcon,
  ActaIcon,
  DashboardIcon,
  EquipmentIcon,
  MessengerIcon,
  ReportsIcon,
  UsersIcon,
  TrashIcon,
  ShieldIcon,
} from "../../components/Icons";
import { AutoDismissAlert } from "../../components/AutoDismissAlert";

const PERMISSION_LABEL_MAP: Record<string, { label: string; desc: string }> = {
  // Actas
  ACTAS_CONSULTAR: { label: "Consultar actas", desc: "Ver listado y consultar detalle de actas" },
  ACTAS_REGISTRAR: { label: "Emitir / Crear acta", desc: "Generar nuevas actas de entrega o devolución" },
  ACTAS_EDITAR: { label: "Modificar acta", desc: "Editar campos e información antes del cierre" },
  ACTAS_FIRMAR: { label: "Firmar digitalmente", desc: "Firmar y certificar validez legal del acta" },
  ACTAS_EXPORTAR: { label: "Exportar actas", desc: "Descargar actas en PDF y planillas" },

  // Dashboard
  DASHBOARD_CONSULTAR: { label: "Visualizar indicadores", desc: "Acceso al resumen ejecutivo y KPIs globales" },

  // Equipos
  EQUIPOS_CONSULTAR: { label: "Consultar inventario", desc: "Ver catálogo y fichas técnicas de equipos" },
  EQUIPOS_REGISTRAR: { label: "Registrar equipo", desc: "Ingresar nuevos activos al inventario" },
  EQUIPOS_EDITAR: { label: "Modificar ficha", desc: "Actualizar datos, marca y asignación de equipo" },
  EQUIPOS_DESACTIVAR: { label: "Inactivar / Dar de baja", desc: "Desactivar equipos obsoletos o retirados" },

  // Mensajero
  MENSAJERO_ACTAS_CONSULTAR: { label: "Consultar entregas", desc: "Ver rutas y actas asignadas para entrega" },
  MENSAJERO_ACTAS_FIRMAR: { label: "Recoger firma en campo", desc: "Capturar firma digital del receptor" },
  MENSAJERO_ACTAS_ENTREGAR: { label: "Completar entrega", desc: "Confirmar entrega exitosa en destino" },
  MENSAJERO_ACTAS_DEVOLVER: { label: "Registrar devolución", desc: "Procesar reintegro de equipo a bodega" },

  // Reportes
  REPORTES_CONSULTAR: { label: "Consultar reportes", desc: "Acceso al centro de reportes y analítica" },
  REPORTES_EXPORTAR: { label: "Exportar datos", desc: "Descargar informes en CSV / Excel y PDF" },

  // Roles
  ROLES_CONSULTAR: { label: "Consultar roles", desc: "Ver catálogo de roles y perfiles RBAC" },
  ROLES_CREAR: { label: "Crear rol", desc: "Registrar nuevos perfiles de usuario" },
  ROLES_EDITAR: { label: "Asignar permisos", desc: "Configurar matriz de privilegios por rol" },
  ROLES_ELIMINAR: { label: "Eliminar rol", desc: "Remover roles sin usuarios asociados" },

  // Usuarios
  USUARIOS_CONSULTAR: { label: "Consultar directorio", desc: "Ver directorio corporativo de colaboradores" },
  USUARIOS_REGISTRAR: { label: "Crear usuario", desc: "Dar de alta nuevos colaboradores" },
  USUARIOS_EDITAR: { label: "Editar usuario", desc: "Modificar datos personales y rol asignado" },
  USUARIOS_DESACTIVAR: { label: "Inactivar usuario", desc: "Suspender acceso de colaboradores retirados" },
};

function formatPermission(p: Permiso): { label: string; desc: string } {
  if (PERMISSION_LABEL_MAP[p.nombre]) {
    return PERMISSION_LABEL_MAP[p.nombre];
  }
  const clean = p.nombre
    .replace(/^([A-Z]+)_/, "")
    .replace(/_/g, " ")
    .toLowerCase();
  const label = clean.charAt(0).toUpperCase() + clean.slice(1);
  return { label, desc: p.descripcion || "Permiso de operación en el módulo" };
}

function getModuloIcon(modulo: string) {
  const m = modulo.toLowerCase();
  if (m.includes("acta")) return <ActaIcon size={16} />;
  if (m.includes("dash")) return <DashboardIcon size={16} />;
  if (m.includes("equip")) return <EquipmentIcon size={16} />;
  if (m.includes("mensaj")) return <MessengerIcon size={16} />;
  if (m.includes("rep")) return <ReportsIcon size={16} />;
  if (m.includes("rol")) return <RolesIcon size={16} />;
  if (m.includes("usu")) return <UsersIcon size={16} />;
  return <PackageIcon size={16} />;
}

function getRoleAccessBadge(name: string) {
  const n = name.toLowerCase();
  if (n.includes("admin")) {
    return (
      <span className="badge" style={{ background: "var(--color-primary-50)", color: "var(--color-primary-700)", border: "1px solid var(--color-primary-200)", whiteSpace: "nowrap" }}>
        <span className="badge-dot" style={{ background: "var(--color-primary-600)" }} />
        Acceso Total (Superadmin)
      </span>
    );
  }
  if (n.includes("audit")) {
    return (
      <span className="badge badge-info" style={{ whiteSpace: "nowrap" }}>
        <span className="badge-dot" />
        Auditoría y Consulta
      </span>
    );
  }
  if (n.includes("gestor")) {
    return (
      <span className="badge badge-success" style={{ whiteSpace: "nowrap" }}>
        <span className="badge-dot" />
        Gestión Operativa
      </span>
    );
  }
  if (n.includes("mensaj")) {
    return (
      <span className="badge badge-warning" style={{ whiteSpace: "nowrap" }}>
        <span className="badge-dot" />
        Operaciones de Campo
      </span>
    );
  }
  return (
    <span className="badge badge-neutral" style={{ whiteSpace: "nowrap" }}>
      <span className="badge-dot" />
      Personalizado
    </span>
  );
}

function getRoleIcon(name: string) {
  const n = name.toLowerCase();
  if (n.includes("admin")) return <ShieldIcon size={16} />;
  if (n.includes("audit")) return <ReportsIcon size={16} />;
  if (n.includes("gestor")) return <EquipmentIcon size={16} />;
  if (n.includes("mensaj")) return <MessengerIcon size={16} />;
  return <RolesIcon size={16} />;
}

export function RolesPage() {
  const [roles, setRoles] = useState<Rol[]>([]);
  const [permisos, setPermisos] = useState<Permiso[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mensaje, setMensaje] = useState<string | null>(null);

  // Filters
  const [busqueda, setBusqueda] = useState("");

  // Modal Crear / Editar Rol
  const [modalRolAbierto, setModalRolAbierto] = useState(false);
  const [modoFormulario, setModoFormulario] = useState<"crear" | "editar">("crear");
  const [rolSeleccionado, setRolSeleccionado] = useState<Rol | null>(null);
  const [formularioRol, setFormularioRol] = useState({
    nombre: "",
    descripcion: "",
  });
  const [guardandoRol, setGuardandoRol] = useState(false);

  // Modal Matriz de Permisos
  const [modalPermisosAbierto, setModalPermisosAbierto] = useState(false);
  const [rolPermisosSeleccionado, setRolPermisosSeleccionado] = useState<Rol | null>(null);
  const [permisosSeleccionados, setPermisosSeleccionados] = useState<number[]>([]);
  const [guardandoPermisos, setGuardandoPermisos] = useState(false);

  // Modal Eliminar Rol
  const [modalEliminarAbierto, setModalEliminarAbierto] = useState(false);
  const [rolAEliminar, setRolAEliminar] = useState<Rol | null>(null);
  const [eliminandoRol, setEliminandoRol] = useState(false);

  async function cargarDatos() {
    try {
      setLoading(true);
      setError(null);
      const [rData, pData] = await Promise.all([getRoles(), getPermisos()]);
      setRoles(rData);
      setPermisos(pData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los roles y permisos.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    cargarDatos();
  }, []);

  const rolesFiltrados = useMemo(() => {
    const term = busqueda.trim().toLowerCase();
    return roles.filter((r) => {
      return (
        term === "" ||
        r.nombre.toLowerCase().includes(term) ||
        (r.descripcion ?? "").toLowerCase().includes(term)
      );
    });
  }, [roles, busqueda]);

  // Agrupar permisos por módulo
  const permisosPorModulo = useMemo(() => {
    const mapa: Record<string, Permiso[]> = {};
    for (const p of permisos) {
      const modulo = p.modulo || "General";
      if (!mapa[modulo]) mapa[modulo] = [];
      mapa[modulo].push(p);
    }
    return mapa;
  }, [permisos]);

  function abrirModalCrear() {
    setModoFormulario("crear");
    setRolSeleccionado(null);
    setFormularioRol({ nombre: "", descripcion: "" });
    setModalRolAbierto(true);
  }

  function abrirModalEditar(r: Rol) {
    setModoFormulario("editar");
    setRolSeleccionado(r);
    setFormularioRol({ nombre: r.nombre, descripcion: r.descripcion || "" });
    setModalRolAbierto(true);
  }

  function abrirModalEliminar(r: Rol) {
    setRolAEliminar(r);
    setModalEliminarAbierto(true);
  }

  async function abrirModalPermisos(r: Rol) {
    try {
      setError(null);
      setRolPermisosSeleccionado(r);
      const rolDetalle = await getRol(r.id);
      setPermisosSeleccionados(rolDetalle.permisos.map((p) => p.id));
      setModalPermisosAbierto(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar los permisos del rol.");
    }
  }

  async function handleGuardarRol(e: React.FormEvent) {
    e.preventDefault();
    if (!formularioRol.nombre.trim()) {
      setError("El nombre del rol es obligatorio.");
      return;
    }

    try {
      setGuardandoRol(true);
      setError(null);
      if (modoFormulario === "crear") {
        await createRol({
          nombre: formularioRol.nombre.trim(),
          descripcion: formularioRol.descripcion.trim(),
        });
        setMensaje(`Rol "${formularioRol.nombre}" creado con éxito.`);
      } else if (rolSeleccionado) {
        await updateRol(rolSeleccionado.id, {
          nombre: formularioRol.nombre.trim(),
          descripcion: formularioRol.descripcion.trim(),
        });
        setMensaje(`Rol "${formularioRol.nombre}" actualizado con éxito.`);
      }
      setModalRolAbierto(false);
      await cargarDatos();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al guardar el rol.");
    } finally {
      setGuardandoRol(false);
    }
  }

  async function handleConfirmarEliminarRol() {
    if (!rolAEliminar) return;

    try {
      setEliminandoRol(true);
      setError(null);
      await deleteRol(rolAEliminar.id);
      setMensaje(`Rol "${rolAEliminar.nombre}" eliminado exitosamente.`);
      setModalEliminarAbierto(false);
      setRolAEliminar(null);
      await cargarDatos();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al eliminar el rol.");
      setModalEliminarAbierto(false);
    } finally {
      setEliminandoRol(false);
    }
  }

  const CRITICAL_ADMIN_PERMISSIONS = [
    "ROLES_CONSULTAR",
    "ROLES_EDITAR",
    "USUARIOS_CONSULTAR",
    "DASHBOARD_CONSULTAR",
  ];

  async function handleGuardarPermisos(e: React.FormEvent) {
    e.preventDefault();
    if (!rolPermisosSeleccionado) return;

    const esAdmin = rolPermisosSeleccionado.nombre.toLowerCase().includes("admin");

    if (esAdmin) {
      if (permisosSeleccionados.length === 0) {
        setError("Acción Bloqueada: No es posible revocar todos los permisos del rol Administrador ya que provocaría un bloqueo total del sistema.");
        return;
      }

      const missingCritical = permisos.filter(
        (p) => CRITICAL_ADMIN_PERMISSIONS.includes(p.nombre) && !permisosSeleccionados.includes(p.id)
      );

      if (missingCritical.length > 0) {
        setError(
          `Advertencia de Seguridad: El rol Administrador debe conservar obligatoriamente los permisos esenciales de acceso (${missingCritical.map((p) => p.nombre).join(", ")}).`
        );
        return;
      }
    }

    try {
      setGuardandoPermisos(true);
      setError(null);
      await updateRolPermisos(rolPermisosSeleccionado.id, permisosSeleccionados);
      setMensaje(`Permisos actualizados para el rol "${rolPermisosSeleccionado.nombre}".`);
      setModalPermisosAbierto(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al asignar permisos.");
    } finally {
      setGuardandoPermisos(false);
    }
  }

  function togglePermiso(id: number) {
    const esAdmin = rolPermisosSeleccionado?.nombre.toLowerCase().includes("admin");
    const permisoObj = permisos.find((p) => p.id === id);

    if (esAdmin && permisoObj && CRITICAL_ADMIN_PERMISSIONS.includes(permisoObj.nombre) && permisosSeleccionados.includes(id)) {
      setError(`El permiso "${permisoObj.nombre}" es crítico para el rol Administrador y no puede ser desmarcado.`);
      return;
    }

    setPermisosSeleccionados((prev) =>
      prev.includes(id) ? prev.filter((p) => p !== id) : [...prev, id]
    );
  }

  function seleccionarModuloCompleto(modulo: string, seleccionar: boolean) {
    const idsModulo = (permisosPorModulo[modulo] || []).map((p) => p.id);
    const esAdmin = rolPermisosSeleccionado?.nombre.toLowerCase().includes("admin");

    if (seleccionar) {
      setPermisosSeleccionados((prev) => Array.from(new Set([...prev, ...idsModulo])));
    } else {
      setPermisosSeleccionados((prev) => {
        return prev.filter((id) => {
          if (!idsModulo.includes(id)) return true;
          if (esAdmin) {
            const pObj = permisos.find((p) => p.id === id);
            if (pObj && CRITICAL_ADMIN_PERMISSIONS.includes(pObj.nombre)) return true; // keep critical
          }
          return false;
        });
      });
    }
  }

  function seleccionarTodosGlobal(seleccionar: boolean) {
    const esAdmin = rolPermisosSeleccionado?.nombre.toLowerCase().includes("admin");
    if (seleccionar) {
      setPermisosSeleccionados(permisos.map((p) => p.id));
    } else {
      if (esAdmin) {
        // Keep critical admin permissions
        const criticalIds = permisos
          .filter((p) => CRITICAL_ADMIN_PERMISSIONS.includes(p.nombre))
          .map((p) => p.id);
        setPermisosSeleccionados(criticalIds);
        setMensaje("Se han desmarcado los permisos no esenciales. Los permisos críticos de administración se mantienen protegidos.");
      } else {
        setPermisosSeleccionados([]);
      }
    }
  }

  if (loading) {
    return (
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '300px', gap: '12px' }}>
        <div style={{ width: '32px', height: '32px', border: '3px solid var(--border-default)', borderTopColor: 'var(--color-primary-600)', borderRadius: '50%', animation: 'spin 0.8s linear infinite' }} />
        <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Cargando catálogo de roles y permisos...</span>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", marginBottom: "24px", flexWrap: "wrap", gap: "14px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase", letterSpacing: "0.6px" }}>
            CONTROL DE ACCESO BASADO EN ROLES (RBAC)
          </span>
          <h1 style={{ margin: "2px 0 4px", fontSize: "24px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.5px" }}>
            Roles y Permisos
          </h1>
          <p style={{ margin: 0, fontSize: "13.5px", color: "var(--text-muted)" }}>
            Configuración de perfiles de seguridad y asignación granular de privilegios en el sistema.
          </p>
        </div>

        <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
          <button type="button" className="btn btn-secondary btn-sm" onClick={cargarDatos} style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
            <RefreshIcon size={14} />
            <span>Actualizar</span>
          </button>
          <button type="button" className="btn btn-primary btn-sm" onClick={abrirModalCrear} style={{ display: "inline-flex", alignItems: "center", gap: "6px" }}>
            <PlusIcon size={14} />
            <span>Nuevo rol</span>
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

      {/* Toolbar Search */}
      <div className="toolbar-container" style={{ background: 'var(--bg-surface)', padding: '16px 20px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--border-default)', marginBottom: '20px' }}>
        <div className="search-input-wrapper" style={{ maxWidth: '400px' }}>
          <span className="search-icon">
            <SearchIcon size={16} />
          </span>
          <input
            id="rolesSearchInput"
            name="rolesSearch"
            className="form-input"
            type="text"
            placeholder="Buscar por nombre o descripción de rol..."
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
          />
        </div>
      </div>

      {/* Tabla de Roles */}
      <div className="table-wrapper">
        <table className="data-table">
          <thead>
            <tr>
              <th>Nombre del Rol</th>
              <th>Descripción Operativa</th>
              <th>Nivel de Acceso y Alcance</th>
              <th style={{ textAlign: "center" }}>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {rolesFiltrados.length === 0 ? (
              <tr>
                <td colSpan={4} style={{ textAlign: "center", padding: "36px", color: "var(--text-muted)" }}>
                  No se encontraron roles configurados.
                </td>
              </tr>
            ) : (
              rolesFiltrados.map((r) => (
                <tr key={r.id}>
                  <td className="primary-cell">
                    <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
                      <div style={{ width: "32px", height: "32px", borderRadius: "var(--radius-sm)", background: "var(--color-primary-50)", color: "var(--color-primary-600)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                        {getRoleIcon(r.nombre)}
                      </div>
                      <strong style={{ color: "var(--text-primary)" }}>{r.nombre}</strong>
                    </div>
                  </td>
                  <td style={{ color: "var(--text-secondary)" }}>{r.descripcion || "Sin descripción"}</td>
                  <td>
                    {getRoleAccessBadge(r.nombre)}
                  </td>
                  <td>
                    <div style={{ display: "flex", gap: "6px", justifyContent: "center" }}>
                      <button
                        type="button"
                        className="btn btn-secondary btn-sm"
                        onClick={() => abrirModalEditar(r)}
                        style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}
                        title="Editar nombre y descripción"
                      >
                        <EditIcon size={13} />
                        <span>Editar</span>
                      </button>
                      <button
                        type="button"
                        className="btn btn-primary btn-sm"
                        onClick={() => abrirModalPermisos(r)}
                        style={{ display: "inline-flex", alignItems: "center", gap: "4px" }}
                        title="Configurar matriz de permisos"
                      >
                        <KeyIcon size={13} />
                        <span>Permisos</span>
                      </button>
                      {r.nombre.toLowerCase() !== "administrador" && (
                        <button
                          type="button"
                          className="btn btn-ghost btn-sm"
                          onClick={() => abrirModalEliminar(r)}
                          style={{ color: "var(--color-danger-600)", display: "inline-flex", alignItems: "center", gap: "4px" }}
                          title="Eliminar rol del sistema"
                        >
                          <TrashIcon size={13} />
                          <span>Eliminar</span>
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal Crear / Editar Rol */}
      {modalRolAbierto && (
        <div className="modal-backdrop" onClick={() => setModalRolAbierto(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--text-muted)", textTransform: "uppercase" }}>CONFIGURACIÓN</span>
                <h2>{modoFormulario === "crear" ? "Registrar Nuevo Rol" : "Editar Rol"}</h2>
              </div>
              <button type="button" className="modal-close" onClick={() => setModalRolAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleGuardarRol}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "14px" }}>
                <div className="form-group">
                  <label className="form-label" htmlFor="rolNombreInput">Nombre del Rol *</label>
                  <input
                    id="rolNombreInput"
                    name="nombre"
                    type="text"
                    className="form-input"
                    required
                    placeholder="Ej. Auditor, Soporte Técnico, Mensajero..."
                    value={formularioRol.nombre}
                    onChange={(e) => setFormularioRol({ ...formularioRol, nombre: e.target.value })}
                  />
                </div>

                <div className="form-group">
                  <label className="form-label" htmlFor="rolDescripcionInput">Descripción Operativa</label>
                  <textarea
                    id="rolDescripcionInput"
                    name="descripcion"
                    className="form-input"
                    rows={3}
                    placeholder="Describe el alcance y responsabilidades asignadas a este rol..."
                    value={formularioRol.descripcion}
                    onChange={(e) => setFormularioRol({ ...formularioRol, descripcion: e.target.value })}
                  />
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-ghost" onClick={() => setModalRolAbierto(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={guardandoRol}>
                  {guardandoRol ? "Guardando..." : modoFormulario === "crear" ? "Crear Rol" : "Guardar Cambios"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Confirmar Eliminar Rol */}
      {modalEliminarAbierto && rolAEliminar && (
        <div className="modal-backdrop" onClick={() => setModalEliminarAbierto(false)}>
          <div className="modal-content small" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--color-danger-600)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
                  ELIMINACIÓN DE ROL
                </span>
                <h2 style={{ margin: "2px 0 0" }}>Eliminar Rol: {rolAEliminar.nombre}</h2>
              </div>
              <button type="button" className="modal-close" onClick={() => setModalEliminarAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "12px", padding: "20px 24px" }}>
              <p style={{ fontSize: "13.5px", color: "var(--text-primary)", margin: 0, lineHeight: 1.5 }}>
                ¿Estás seguro de que deseas eliminar permanentemente el rol <strong>"{rolAEliminar.nombre}"</strong>?
              </p>
              <div style={{ background: "var(--color-warning-50)", border: "1px solid var(--color-warning-200)", borderRadius: "var(--radius-md)", padding: "12px", fontSize: "12px", color: "var(--color-warning-900)" }}>
                <strong>Regla de integridad RBAC:</strong> Si este rol tiene usuarios asociados actualmente, el sistema rechazará la eliminación para no dejar usuarios sin permisos. Primero deberás reasignar sus colaboradores a otro rol.
              </div>
            </div>

            <div className="modal-footer" style={{ borderTop: "1px solid var(--border-default)", background: "var(--bg-surface)", padding: "16px 24px" }}>
              <button type="button" className="btn btn-ghost" onClick={() => setModalEliminarAbierto(false)} disabled={eliminandoRol}>
                Cancelar
              </button>
              <button
                type="button"
                className="btn btn-primary"
                style={{ background: "var(--color-danger-600)", borderColor: "var(--color-danger-600)" }}
                onClick={handleConfirmarEliminarRol}
                disabled={eliminandoRol}
              >
                {eliminandoRol ? "Eliminando..." : "Eliminar Rol"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Matriz de Permisos */}
      {modalPermisosAbierto && rolPermisosSeleccionado && (
        <div className="modal-backdrop" onClick={() => setModalPermisosAbierto(false)}>
          <div className="modal-content large" onClick={(e) => e.stopPropagation()} style={{ maxHeight: "90vh", display: "flex", flexDirection: "column" }}>
            <div className="modal-header">
              <div>
                <span style={{ fontSize: "11px", fontWeight: 700, color: "var(--color-primary-600)", textTransform: "uppercase", letterSpacing: "0.5px" }}>
                  MATRIZ RBAC DE PRIVILEGIOS
                </span>
                <h2 style={{ margin: "2px 0 0" }}>Permisos para: {rolPermisosSeleccionado.nombre}</h2>
              </div>
              <button type="button" className="modal-close" onClick={() => setModalPermisosAbierto(false)}>
                <CloseIcon size={16} />
              </button>
            </div>

            <form onSubmit={handleGuardarPermisos} style={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0 }}>
              {/* Barra de estado y acciones rápidas */}
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "12px 24px", background: "var(--color-slate-100)", borderBottom: "1px solid var(--border-default)", flexWrap: "wrap", gap: "10px" }}>
                <span style={{ fontSize: "13px", color: "var(--text-secondary)" }}>
                  Asignados: <strong style={{ color: "var(--color-primary-700)" }}>{permisosSeleccionados.length}</strong> de <strong>{permisos.length}</strong> permisos disponibles
                </span>
                <div style={{ display: "flex", gap: "8px" }}>
                  <button
                    type="button"
                    className="btn btn-secondary btn-sm"
                    onClick={() => seleccionarTodosGlobal(true)}
                  >
                    Seleccionar todos
                  </button>
                  <button
                    type="button"
                    className="btn btn-ghost btn-sm"
                    onClick={() => seleccionarTodosGlobal(false)}
                  >
                    Desmarcar todos
                  </button>
                </div>
              </div>

              <div className="modal-body" style={{ flex: 1, overflowY: "auto", padding: "20px 24px", display: "flex", flexDirection: "column", gap: "16px" }}>
                {rolPermisosSeleccionado?.nombre.toLowerCase().includes("admin") && (
                  <div
                    style={{
                      background: "var(--color-warning-50)",
                      border: "1px solid var(--color-warning-200)",
                      borderRadius: "var(--radius-md)",
                      padding: "12px 16px",
                      display: "flex",
                      alignItems: "center",
                      gap: "12px",
                      color: "var(--color-warning-900)",
                      fontSize: "12.5px",
                      lineHeight: "1.4",
                    }}
                  >
                    <div style={{ color: "var(--color-warning-600)", display: "flex", alignItems: "center" }}>
                      <ShieldIcon size={22} />
                    </div>
                    <div>
                      <strong>Protección de Rol Maestro:</strong> Para garantizar la continuidad operativa y evitar bloqueos accidentales de acceso, el rol <strong>Administrador</strong> mantiene protegidos los permisos críticos de administración (Dashboard, Roles y Usuarios).
                    </div>
                  </div>
                )}

                <div style={{ display: "flex", flexDirection: "column", gap: "18px" }}>
                  {Object.entries(permisosPorModulo).map(([modulo, lista]) => {
                    const idsModulo = lista.map((p) => p.id);
                    const seleccionadosEnModulo = idsModulo.filter((id) => permisosSeleccionados.includes(id)).length;

                    return (
                      <div
                        key={modulo}
                        style={{
                          background: "#ffffff",
                          border: "1px solid var(--border-default)",
                          borderRadius: "var(--radius-lg)",
                          padding: "16px 18px",
                          boxShadow: "var(--shadow-sm)",
                        }}
                      >
                        {/* Cabecera del Módulo */}
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "14px", borderBottom: "1px solid var(--border-subtle)", paddingBottom: "10px" }}>
                          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                            <div style={{ width: "28px", height: "28px", borderRadius: "var(--radius-sm)", background: "var(--color-primary-50)", color: "var(--color-primary-700)", display: "flex", alignItems: "center", justifyContent: "center" }}>
                              {getModuloIcon(modulo)}
                            </div>
                            <div>
                              <strong style={{ fontSize: "13.5px", color: "var(--text-primary)" }}>
                                {modulo}
                              </strong>
                              <span style={{ marginLeft: "8px", fontSize: "11px", padding: "2px 7px", borderRadius: "var(--radius-pill)", background: seleccionadosEnModulo > 0 ? "var(--color-primary-50)" : "var(--color-slate-100)", color: seleccionadosEnModulo > 0 ? "var(--color-primary-700)" : "var(--text-muted)", fontWeight: 700 }}>
                                {seleccionadosEnModulo} / {idsModulo.length}
                              </span>
                            </div>
                          </div>

                          <div style={{ display: "flex", gap: "6px" }}>
                            <button
                              type="button"
                              className="btn btn-ghost btn-sm"
                              style={{ fontSize: "11.5px", padding: "3px 8px" }}
                              onClick={() => seleccionarModuloCompleto(modulo, true)}
                            >
                              Marcar todos
                            </button>
                            <button
                              type="button"
                              className="btn btn-ghost btn-sm"
                              style={{ fontSize: "11.5px", padding: "3px 8px" }}
                              onClick={() => seleccionarModuloCompleto(modulo, false)}
                            >
                              Ninguno
                            </button>
                          </div>
                        </div>

                        {/* Grid de Checkboxes con Acciones Claras */}
                        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(210px, 1fr))", gap: "10px" }}>
                          {lista.map((p) => {
                            const checked = permisosSeleccionados.includes(p.id);
                            const info = formatPermission(p);
                            const esAdmin = rolPermisosSeleccionado?.nombre.toLowerCase().includes("admin");
                            const esCritico = esAdmin && CRITICAL_ADMIN_PERMISSIONS.includes(p.nombre);

                            return (
                              <label
                                key={p.id}
                                style={{
                                  display: "flex",
                                  alignItems: "flex-start",
                                  gap: "10px",
                                  padding: "10px 12px",
                                  background: checked ? (esCritico ? "var(--color-warning-50)" : "var(--color-primary-50)") : "#ffffff",
                                  border: `1.5px solid ${checked ? (esCritico ? "var(--color-warning-400)" : "var(--color-primary-500)") : "var(--border-default)"}`,
                                  borderRadius: "var(--radius-md)",
                                  cursor: esCritico ? "not-allowed" : "pointer",
                                  transition: "all var(--transition-fast)",
                                  userSelect: "none",
                                  opacity: esCritico ? 0.95 : 1,
                                }}
                              >
                                <input
                                  type="checkbox"
                                  checked={checked}
                                  disabled={esCritico}
                                  onChange={() => togglePermiso(p.id)}
                                  style={{
                                    accentColor: esCritico ? "var(--color-warning-600)" : "var(--color-primary-600)",
                                    marginTop: "2px",
                                    width: "15px",
                                    height: "15px",
                                    cursor: esCritico ? "not-allowed" : "pointer",
                                  }}
                                />
                                <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
                                  <div style={{ display: "flex", alignItems: "center", gap: "6px", flexWrap: "wrap" }}>
                                    <span
                                      style={{
                                       fontSize: "13px",
                                       fontWeight: checked ? 700 : 600,
                                       color: checked ? "var(--color-primary-900)" : "var(--text-primary)",
                                      }}
                                    >
                                      {info.label}
                                    </span>
                                    {esCritico && (
                                      <span
                                        style={{
                                          fontSize: "10px",
                                          padding: "1px 5px",
                                          borderRadius: "var(--radius-pill)",
                                          background: "var(--color-warning-100)",
                                          color: "var(--color-warning-900)",
                                          fontWeight: 700,
                                          border: "1px solid var(--color-warning-300)",
                                        }}
                                        title="Permiso protegido para el rol Administrador"
                                      >
                                        Protegido
                                      </span>
                                    )}
                                  </div>
                                  <span
                                    style={{
                                      fontSize: "11px",
                                      color: "var(--text-muted)",
                                      lineHeight: 1.25,
                                    }}
                                  >
                                    {info.desc}
                                  </span>
                                </div>
                              </label>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              <div className="modal-footer" style={{ borderTop: "1px solid var(--border-default)", background: "var(--bg-surface)", padding: "16px 24px" }}>
                <button type="button" className="btn btn-ghost" onClick={() => setModalPermisosAbierto(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={guardandoPermisos}>
                  {guardandoPermisos ? "Guardando permisos..." : "Guardar Matriz de Permisos"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
