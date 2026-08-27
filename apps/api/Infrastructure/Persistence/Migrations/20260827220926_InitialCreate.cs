using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlataformaActas.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    inventory_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    empresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    asignado_a = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    acta_entregada = table.Column<bool>(type: "boolean", nullable: true),
                    proveedor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nombre_equipo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    hoja_de_vida = table.Column<string>(type: "text", nullable: true),
                    marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    procesador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    disco_duro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    memoria_ram = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    departamento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    estatus_equipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mantenimiento_realizado = table.Column<bool>(type: "boolean", nullable: true),
                    tipo_mantenimiento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fecha_mantenimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    source_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "P8-FR-019"),
                    last_sync_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("equipment_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsuarioRed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Proceso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rol_permisos",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    PermisoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_rol_permisos_permiso",
                        column: x => x.PermisoId,
                        principalTable: "permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rol_permisos_rol",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "actas_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    numero_acta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    destinatario_usuario_id = table.Column<int>(type: "integer", nullable: true),
                    mensajero_usuario_id = table.Column<int>(type: "integer", nullable: true),
                    acta_entregada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Borrador"),
                    tipo_acta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Entrega"),
                    acta_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ruta_documento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_firma = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    fecha_anulacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_finalizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    device_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    form_data_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    hash_sha256 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    signature_data_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("actas_v2_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_actas_v2_acta_origen",
                        column: x => x.acta_origen_id,
                        principalTable: "actas_v2",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actas_v2_destinatario",
                        column: x => x.destinatario_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actas_v2_equipment",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actas_v2_mensajero",
                        column: x => x.mensajero_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actas_v2_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_usuarios",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_usuario_roles_rol",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuario",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acta_observations_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    acta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    texto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("acta_observations_v2_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_acta_observations_v2_acta",
                        column: x => x.acta_id,
                        principalTable: "actas_v2",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_acta_observations_v2_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "acta_signatures_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    acta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    signature_base64 = table.Column<string>(type: "text", nullable: false),
                    consent_accepted = table.Column<bool>(type: "boolean", nullable: false),
                    legal_clause_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("acta_signatures_v2_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_acta_signatures_v2_acta",
                        column: x => x.acta_id,
                        principalTable: "actas_v2",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "acta_status_history_v2",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    acta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_anterior = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estado_nuevo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("acta_status_history_v2_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_acta_status_history_v2_acta",
                        column: x => x.acta_id,
                        principalTable: "actas_v2",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_acta_status_history_v2_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acta_observations_v2_acta_id",
                table: "acta_observations_v2",
                column: "acta_id");

            migrationBuilder.CreateIndex(
                name: "ix_acta_observations_v2_usuario_id",
                table: "acta_observations_v2",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_acta_signatures_v2_acta_id",
                table: "acta_signatures_v2",
                column: "acta_id");

            migrationBuilder.CreateIndex(
                name: "ix_acta_status_history_v2_acta_id",
                table: "acta_status_history_v2",
                column: "acta_id");

            migrationBuilder.CreateIndex(
                name: "ix_acta_status_history_v2_usuario_id",
                table: "acta_status_history_v2",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_acta_origen_id",
                table: "actas_v2",
                column: "acta_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_destinatario_usuario_id",
                table: "actas_v2",
                column: "destinatario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_equipment_id",
                table: "actas_v2",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_estado",
                table: "actas_v2",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_mensajero_usuario_id",
                table: "actas_v2",
                column: "mensajero_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_numero_acta",
                table: "actas_v2",
                column: "numero_acta");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_tipo_acta",
                table: "actas_v2",
                column: "tipo_acta");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_usuario_id",
                table: "actas_v2",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "equipment_inventory_key_key",
                table: "equipment",
                column: "inventory_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_equipment_estado",
                table: "equipment",
                column: "estatus_equipo");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_nombre",
                table: "equipment",
                column: "nombre_equipo");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_FechaExpiracion",
                table: "password_reset_tokens",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_TokenHash",
                table: "password_reset_tokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_UsuarioId",
                table: "password_reset_tokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ_permisos_nombre",
                table: "permisos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_permisos_PermisoId",
                table: "rol_permisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "UQ_roles_nombre",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_RolId",
                table: "usuario_roles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Correo",
                table: "usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_UsuarioRed",
                table: "usuarios",
                column: "UsuarioRed",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acta_observations_v2");

            migrationBuilder.DropTable(
                name: "acta_signatures_v2");

            migrationBuilder.DropTable(
                name: "acta_status_history_v2");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "rol_permisos");

            migrationBuilder.DropTable(
                name: "usuario_roles");

            migrationBuilder.DropTable(
                name: "actas_v2");

            migrationBuilder.DropTable(
                name: "permisos");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "equipment");
        }
    }
}
