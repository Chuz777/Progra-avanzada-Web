using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcesionarioN.Migrations
{
    /// <inheritdoc />
    public partial class Modulo2Catalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MotoId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "Vehiculos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vin",
                table: "Vehiculos",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "Motos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vin",
                table: "Motos",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Accesorios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Accesorios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_MotoId",
                table: "Ventas",
                column: "MotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas",
                column: "MotoId",
                principalTable: "Motos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_MotoId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MotoId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Placa",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Vin",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Placa",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Vin",
                table: "Motos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Accesorios");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Accesorios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
