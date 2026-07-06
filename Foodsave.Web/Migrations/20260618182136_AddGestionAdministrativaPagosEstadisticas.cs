using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Foodsave.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddGestionAdministrativaPagosEstadisticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Plan",
                table: "Suscripciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Suscripciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "EstadoPago",
                table: "Suscripciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProximoVencimiento",
                table: "Suscripciones",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoPago",
                table: "Suscripciones",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoMensual",
                table: "Suscripciones",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EstadoAdministrativo",
                table: "Comercios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Activo");

            migrationBuilder.Sql(
                """
                UPDATE "Suscripciones"
                SET "EstadoPago" = CASE
                        WHEN "Estado" = 'Activa' THEN 'AlDia'
                        WHEN "Estado" = 'Vencida' THEN 'Vencido'
                        ELSE 'Pendiente'
                    END,
                    "FechaProximoVencimiento" = "FechaFin",
                    "FechaUltimoPago" = CASE
                        WHEN "Estado" = 'Activa' THEN "FechaInicio"
                        ELSE NULL
                    END;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaProximoVencimiento",
                table: "Suscripciones",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComercioId = table.Column<int>(type: "integer", nullable: false),
                    SuscripcionId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Comercios_ComercioId",
                        column: x => x.ComercioId,
                        principalTable: "Comercios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Suscripciones_SuscripcionId",
                        column: x => x.SuscripcionId,
                        principalTable: "Suscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_EstadoPago",
                table: "Suscripciones",
                column: "EstadoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_FechaProximoVencimiento",
                table: "Suscripciones",
                column: "FechaProximoVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_EstadoAdministrativo",
                table: "Comercios",
                column: "EstadoAdministrativo");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ComercioId",
                table: "Pagos",
                column: "ComercioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_SuscripcionId",
                table: "Pagos",
                column: "SuscripcionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Suscripciones_EstadoPago",
                table: "Suscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Suscripciones_FechaProximoVencimiento",
                table: "Suscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Comercios_EstadoAdministrativo",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "FechaProximoVencimiento",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "FechaUltimoPago",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "MontoMensual",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "EstadoAdministrativo",
                table: "Comercios");

            migrationBuilder.AlterColumn<string>(
                name: "Plan",
                table: "Suscripciones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Suscripciones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
