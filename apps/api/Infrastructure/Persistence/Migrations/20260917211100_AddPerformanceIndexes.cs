using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlataformaActas.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_acta_status_history_v2_acta",
                table: "acta_status_history_v2");

            migrationBuilder.DropForeignKey(
                name: "fk_acta_status_history_v2_usuario",
                table: "acta_status_history_v2");

            migrationBuilder.DropForeignKey(
                name: "FK_password_reset_tokens_usuarios",
                table: "password_reset_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_rol_permisos_permiso",
                table: "rol_permisos");

            migrationBuilder.DropForeignKey(
                name: "FK_rol_permisos_rol",
                table: "rol_permisos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_roles_rol",
                table: "usuario_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_roles_usuario",
                table: "usuario_roles");

            migrationBuilder.DropIndex(
                name: "UQ_permisos_nombre",
                table: "permisos");

            migrationBuilder.DropIndex(
                name: "IX_password_reset_tokens_FechaExpiracion",
                table: "password_reset_tokens");

            migrationBuilder.DropIndex(
                name: "ix_actas_v2_numero_acta",
                table: "actas_v2");

            migrationBuilder.DropIndex(
                name: "ix_actas_v2_tipo_acta",
                table: "actas_v2");

            migrationBuilder.DropColumn(
                name: "UltimoAcceso",
                table: "usuarios");

            migrationBuilder.RenameIndex(
                name: "UQ_roles_nombre",
                table: "roles",
                newName: "IX_roles_Nombre");

            migrationBuilder.RenameIndex(
                name: "ix_equipment_estado",
                table: "equipment",
                newName: "ix_equipment_estatus");

            migrationBuilder.RenameIndex(
                name: "ix_actas_v2_acta_origen_id",
                table: "actas_v2",
                newName: "IX_actas_v2_acta_origen_id");

            migrationBuilder.RenameIndex(
                name: "ix_acta_status_history_v2_usuario_id",
                table: "acta_status_history_v2",
                newName: "ix_acta_status_history_usuario_id");

            migrationBuilder.RenameIndex(
                name: "ix_acta_status_history_v2_acta_id",
                table: "acta_status_history_v2",
                newName: "ix_acta_status_history_acta_id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "password_reset_tokens",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_mantenimiento",
                table: "equipment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "observaciones",
                table: "equipment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "estatus_equipo",
                table: "equipment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Activo",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "equipment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "departamento",
                table: "equipment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_acta",
                table: "actas_v2",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Entrega",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Entrega");

            migrationBuilder.AlterColumn<string>(
                name: "ruta_documento",
                table: "actas_v2",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "consent_accepted",
                table: "acta_signatures_v2",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "texto",
                table: "acta_observations_v2",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_fecha_creacion",
                table: "actas_v2",
                column: "fecha_creacion");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_numero_acta",
                table: "actas_v2",
                column: "numero_acta",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_acta_status_history_acta",
                table: "acta_status_history_v2",
                column: "acta_id",
                principalTable: "actas_v2",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_acta_status_history_usuario",
                table: "acta_status_history_v2",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_password_reset_tokens_usuarios_UsuarioId",
                table: "password_reset_tokens",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_permisos_permisos_PermisoId",
                table: "rol_permisos",
                column: "PermisoId",
                principalTable: "permisos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_permisos_roles_RolId",
                table: "rol_permisos",
                column: "RolId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_roles_roles_RolId",
                table: "usuario_roles",
                column: "RolId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_roles_usuarios_UsuarioId",
                table: "usuario_roles",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_acta_status_history_acta",
                table: "acta_status_history_v2");

            migrationBuilder.DropForeignKey(
                name: "fk_acta_status_history_usuario",
                table: "acta_status_history_v2");

            migrationBuilder.DropForeignKey(
                name: "FK_password_reset_tokens_usuarios_UsuarioId",
                table: "password_reset_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_rol_permisos_permisos_PermisoId",
                table: "rol_permisos");

            migrationBuilder.DropForeignKey(
                name: "FK_rol_permisos_roles_RolId",
                table: "rol_permisos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_roles_roles_RolId",
                table: "usuario_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_roles_usuarios_UsuarioId",
                table: "usuario_roles");

            migrationBuilder.DropIndex(
                name: "ix_actas_v2_fecha_creacion",
                table: "actas_v2");

            migrationBuilder.DropIndex(
                name: "ix_actas_v2_numero_acta",
                table: "actas_v2");

            migrationBuilder.RenameIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                newName: "UQ_roles_nombre");

            migrationBuilder.RenameIndex(
                name: "ix_equipment_estatus",
                table: "equipment",
                newName: "ix_equipment_estado");

            migrationBuilder.RenameIndex(
                name: "IX_actas_v2_acta_origen_id",
                table: "actas_v2",
                newName: "ix_actas_v2_acta_origen_id");

            migrationBuilder.RenameIndex(
                name: "ix_acta_status_history_usuario_id",
                table: "acta_status_history_v2",
                newName: "ix_acta_status_history_v2_usuario_id");

            migrationBuilder.RenameIndex(
                name: "ix_acta_status_history_acta_id",
                table: "acta_status_history_v2",
                newName: "ix_acta_status_history_v2_acta_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoAcceso",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "password_reset_tokens",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_mantenimiento",
                table: "equipment",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "observaciones",
                table: "equipment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "estatus_equipo",
                table: "equipment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Activo");

            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                table: "equipment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "departamento",
                table: "equipment",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_acta",
                table: "actas_v2",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Entrega",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Entrega");

            migrationBuilder.AlterColumn<string>(
                name: "ruta_documento",
                table: "actas_v2",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "consent_accepted",
                table: "acta_signatures_v2",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "texto",
                table: "acta_observations_v2",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "UQ_permisos_nombre",
                table: "permisos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_FechaExpiracion",
                table: "password_reset_tokens",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_numero_acta",
                table: "actas_v2",
                column: "numero_acta");

            migrationBuilder.CreateIndex(
                name: "ix_actas_v2_tipo_acta",
                table: "actas_v2",
                column: "tipo_acta");

            migrationBuilder.AddForeignKey(
                name: "fk_acta_status_history_v2_acta",
                table: "acta_status_history_v2",
                column: "acta_id",
                principalTable: "actas_v2",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_acta_status_history_v2_usuario",
                table: "acta_status_history_v2",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_password_reset_tokens_usuarios",
                table: "password_reset_tokens",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_permisos_permiso",
                table: "rol_permisos",
                column: "PermisoId",
                principalTable: "permisos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rol_permisos_rol",
                table: "rol_permisos",
                column: "RolId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_roles_rol",
                table: "usuario_roles",
                column: "RolId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_roles_usuario",
                table: "usuario_roles",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
