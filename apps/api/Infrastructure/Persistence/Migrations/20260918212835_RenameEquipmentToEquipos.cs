using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaActas.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameEquipmentToEquipos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Renombrado atómico de tabla en PostgreSQL sin pérdida de datos ni recreación
            migrationBuilder.RenameTable(
                name: "equipment",
                newName: "equipos");

            // 2. Renombrado controlado de índices asociados
            migrationBuilder.RenameIndex(
                name: "equipment_inventory_key_key",
                table: "equipos",
                newName: "equipos_inventory_key_key");

            migrationBuilder.RenameIndex(
                name: "ix_equipment_estatus",
                table: "equipos",
                newName: "ix_equipos_estatus");

            migrationBuilder.RenameIndex(
                name: "ix_equipment_nombre",
                table: "equipos",
                newName: "ix_equipos_nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Reversión atómica de tabla
            migrationBuilder.RenameTable(
                name: "equipos",
                newName: "equipment");

            // 2. Reversión de índices asociados
            migrationBuilder.RenameIndex(
                name: "equipos_inventory_key_key",
                table: "equipment",
                newName: "equipment_inventory_key_key");

            migrationBuilder.RenameIndex(
                name: "ix_equipos_estatus",
                table: "equipment",
                newName: "ix_equipment_estatus");

            migrationBuilder.RenameIndex(
                name: "ix_equipos_nombre",
                table: "equipment",
                newName: "ix_equipment_nombre");
        }
    }
}
