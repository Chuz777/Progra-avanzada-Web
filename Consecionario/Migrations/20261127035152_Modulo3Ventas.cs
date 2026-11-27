using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcesionarioN.Migrations
{
    /// <inheritdoc />
    public partial class Modulo3Ventas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Accesorios_AccesorioId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Vehiculos_VehiculoId",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "AccesorioId",
                table: "Ventas",
                newName: "VendedorId");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_AccesorioId",
                table: "Ventas",
                newName: "IX_Ventas_VendedorId");

            migrationBuilder.AddColumn<bool>(
                name: "Anulada",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Ventas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Ventas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Ventas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Ventas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VentaLineas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    AccesorioId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaLineas_Accesorios_AccesorioId",
                        column: x => x.AccesorioId,
                        principalTable: "Accesorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaLineas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaLineas_AccesorioId",
                table: "VentaLineas",
                column: "AccesorioId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaLineas_VentaId",
                table: "VentaLineas",
                column: "VentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas",
                column: "MotoId",
                principalTable: "Motos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_VendedorId",
                table: "Ventas",
                column: "VendedorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Vehiculos_VehiculoId",
                table: "Ventas",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_ClienteId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_VendedorId",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Vehiculos_VehiculoId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "VentaLineas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Anulada",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "VendedorId",
                table: "Ventas",
                newName: "AccesorioId");

            migrationBuilder.RenameIndex(
                name: "IX_Ventas_VendedorId",
                table: "Ventas",
                newName: "IX_Ventas_AccesorioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Accesorios_AccesorioId",
                table: "Ventas",
                column: "AccesorioId",
                principalTable: "Accesorios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Motos_MotoId",
                table: "Ventas",
                column: "MotoId",
                principalTable: "Motos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Vehiculos_VehiculoId",
                table: "Ventas",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id");
        }
    }
}
