using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodsave.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanComercios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Comercios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Estandar");

            migrationBuilder.Sql(
                """
                UPDATE "Comercios" AS c
                SET "Plan" = COALESCE(
                    (
                        SELECT s."Plan"
                        FROM "Suscripciones" AS s
                        WHERE s."ComercioId" = c."Id"
                        ORDER BY
                            CASE WHEN s."Estado" = 'Activa' THEN 0 ELSE 1 END,
                            s."FechaInicio" DESC,
                            s."Id" DESC
                        LIMIT 1
                    ),
                    'Estandar'
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Plan",
                table: "Comercios",
                column: "Plan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comercios_Plan",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Comercios");
        }
    }
}
