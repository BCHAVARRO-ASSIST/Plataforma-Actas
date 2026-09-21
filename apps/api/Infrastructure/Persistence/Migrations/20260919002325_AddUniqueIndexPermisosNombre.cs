using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaActas.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexPermisosNombre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_permisos_Nombre",
                table: "permisos",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_permisos_Nombre",
                table: "permisos");
        }
    }
}
