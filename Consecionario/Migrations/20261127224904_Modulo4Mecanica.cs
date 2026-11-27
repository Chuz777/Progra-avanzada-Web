using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcesionarioN.Migrations
{
    /// <inheritdoc />
    public partial class Modulo4Mecanica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TallerCitas_Usuarios_UsuarioId",
                table: "TallerCitas");

            migrationBuilder.RenameColumn(
                name: "FechaCita",
                table: "TallerCitas",
                newName: "FechaIngreso");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteTelefono",
                table: "TallerCitas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteEmail",
                table: "TallerCitas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "EstadoVehiculo",
                table: "TallerCitas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCambioEstado",
                table: "TallerCitas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CitasIngreso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlacaVehiculo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitasIngreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitasIngreso_Usuarios_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitasIngreso_ClienteId",
                table: "CitasIngreso",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CitasIngreso_FechaHora",
                table: "CitasIngreso",
                column: "FechaHora",
                unique: true,
                filter: "[Estado] = N'Programada'");

            migrationBuilder.AddForeignKey(
                name: "FK_TallerCitas_Usuarios_UsuarioId",
                table: "TallerCitas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TallerCitas_Usuarios_UsuarioId",
                table: "TallerCitas");

            migrationBuilder.DropTable(
                name: "CitasIngreso");

            migrationBuilder.DropColumn(
                name: "EstadoVehiculo",
                table: "TallerCitas");

            migrationBuilder.DropColumn(
                name: "FechaCambioEstado",
                table: "TallerCitas");

            migrationBuilder.RenameColumn(
                name: "FechaIngreso",
                table: "TallerCitas",
                newName: "FechaCita");

            migrationBuilder.AlterColumn<string>(
                name: "ClienteTelefono",
                table: "TallerCitas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClienteEmail",
                table: "TallerCitas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TallerCitas_Usuarios_UsuarioId",
                table: "TallerCitas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
