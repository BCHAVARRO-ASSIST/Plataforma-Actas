namespace PlataformaActas.Api.Authorization;

public static class Permissions
{
    // =====================================================
    // DASHBOARD
    // =====================================================

    public const string DashboardConsultar =
        "DASHBOARD_CONSULTAR";


    // =====================================================
    // USUARIOS
    // =====================================================


    public const string UsuariosConsultar =
        "USUARIOS_CONSULTAR";

    public const string UsuariosRegistrar =
        "USUARIOS_REGISTRAR";

    public const string UsuariosEditar =
        "USUARIOS_EDITAR";

    public const string UsuariosDesactivar =
        "USUARIOS_DESACTIVAR";


    // =====================================================
    // EQUIPOS
    // =====================================================

    public const string EquiposConsultar =
        "EQUIPOS_CONSULTAR";

    public const string EquiposRegistrar =
        "EQUIPOS_REGISTRAR";

    public const string EquiposEditar =
        "EQUIPOS_EDITAR";

    public const string EquiposDesactivar =
        "EQUIPOS_DESACTIVAR";


    // =====================================================
    // ACTAS
    // =====================================================

    public const string ActasConsultar =
        "ACTAS_CONSULTAR";

    public const string ActasRegistrar =
        "ACTAS_REGISTRAR";

    public const string ActasEditar =
        "ACTAS_EDITAR";

    public const string ActasFirmar =
        "ACTAS_FIRMAR";

    public const string ActasExportar =
        "ACTAS_EXPORTAR";


    // =====================================================
    // REPORTES
    // =====================================================

    public const string ReportesConsultar =
        "REPORTES_CONSULTAR";

    public const string ReportesExportar =
        "REPORTES_EXPORTAR";


    // =====================================================
    // ROLES
    // =====================================================

    public const string RolesConsultar =
        "ROLES_CONSULTAR";

    public const string RolesCrear =
        "ROLES_CREAR";

    public const string RolesEditar =
        "ROLES_EDITAR";

    public const string RolesEliminar =
        "ROLES_ELIMINAR";


    // =====================================================
    // MENSAJERO
    // =====================================================

    public const string MensajeroActasConsultar =
        "MENSAJERO_ACTAS_CONSULTAR";

    public const string MensajeroActasFirmar =
        "MENSAJERO_ACTAS_FIRMAR";

    public const string MensajeroActasEntregar =
        "MENSAJERO_ACTAS_ENTREGAR";

    public const string MensajeroActasDevolver =
        "MENSAJERO_ACTAS_DEVOLVER";
}