using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foodsave.Web.Migrations
{
    /// <inheritdoc />
    public partial class MejorasModeloDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comercios_Titulares_TitularId",
                table: "Comercios");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Comercios_ComercioId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ComercioId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Titulares",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Titulares",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Titulares",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Titulares",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Comercios",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Rubro",
                table: "Comercios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Comercios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Comercios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Titulares_Email",
                table: "Titulares",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_Estado",
                table: "Suscripciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesComercio_EmailTitular",
                table: "SolicitudesComercio",
                column: "EmailTitular");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ComercioId_SuscripcionId_FechaPago",
                table: "Pagos",
                columns: new[] { "ComercioId", "SuscripcionId", "FechaPago" });

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Nombre",
                table: "Comercios",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Rubro",
                table: "Comercios",
                column: "Rubro");

            migrationBuilder.AddForeignKey(
                name: "FK_Comercios_Titulares_TitularId",
                table: "Comercios",
                column: "TitularId",
                principalTable: "Titulares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Comercios_ComercioId",
                table: "Pagos",
                column: "ComercioId",
                principalTable: "Comercios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comercios_Titulares_TitularId",
                table: "Comercios");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Comercios_ComercioId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Titulares_Email",
                table: "Titulares");

            migrationBuilder.DropIndex(
                name: "IX_Suscripciones_Estado",
                table: "Suscripciones");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesComercio_EmailTitular",
                table: "SolicitudesComercio");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_ComercioId_SuscripcionId_FechaPago",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Comercios_Nombre",
                table: "Comercios");

            migrationBuilder.DropIndex(
                name: "IX_Comercios_Rubro",
                table: "Comercios");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Titulares",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Titulares",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Titulares",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Apellido",
                table: "Titulares",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Comercios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Rubro",
                table: "Comercios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Comercios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Comercios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ComercioId",
                table: "Pagos",
                column: "ComercioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FechaPago",
                table: "Pagos",
                column: "FechaPago");

            migrationBuilder.AddForeignKey(
                name: "FK_Comercios_Titulares_TitularId",
                table: "Comercios",
                column: "TitularId",
                principalTable: "Titulares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Comercios_ComercioId",
                table: "Pagos",
                column: "ComercioId",
                principalTable: "Comercios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
