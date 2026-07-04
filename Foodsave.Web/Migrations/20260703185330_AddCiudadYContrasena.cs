using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodsave.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCiudadYContrasena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "SolicitudesComercio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contrasena",
                table: "SolicitudesComercio",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "SolicitudesComercio");

            migrationBuilder.DropColumn(
                name: "Contrasena",
                table: "SolicitudesComercio");
        }
    }
}
