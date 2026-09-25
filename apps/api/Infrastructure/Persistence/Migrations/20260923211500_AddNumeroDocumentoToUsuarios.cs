using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaActas.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroDocumentoToUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "numero_documento",
                table: "usuarios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numero_documento",
                table: "usuarios");
        }
    }
}
