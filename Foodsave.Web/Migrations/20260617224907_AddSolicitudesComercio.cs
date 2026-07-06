using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Foodsave.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudesComercio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesComercio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreComercio = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Rubro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TelefonoComercio = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    NombreTitular = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApellidoTitular = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelefonoTitular = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EmailTitular = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PlanInteres = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservacionAdmin = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesComercio", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesComercio_Estado",
                table: "SolicitudesComercio",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesComercio_FechaSolicitud",
                table: "SolicitudesComercio",
                column: "FechaSolicitud");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesComercio");
        }
    }
}
