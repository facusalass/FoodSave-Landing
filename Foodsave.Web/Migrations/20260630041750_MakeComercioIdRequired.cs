using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodsave.Web.Migrations
{
    public partial class MakeComercioIdRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suscripciones_Comercios_ComercioId",
                table: "Suscripciones");

            migrationBuilder.AlterColumn<int>(
                name: "ComercioId",
                table: "Suscripciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Suscripciones_Comercios_ComercioId",
                table: "Suscripciones",
                column: "ComercioId",
                principalTable: "Comercios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suscripciones_Comercios_ComercioId",
                table: "Suscripciones");

            migrationBuilder.AlterColumn<int>(
                name: "ComercioId",
                table: "Suscripciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Suscripciones_Comercios_ComercioId",
                table: "Suscripciones",
                column: "ComercioId",
                principalTable: "Comercios",
                principalColumn: "Id");
        }
    }
}
