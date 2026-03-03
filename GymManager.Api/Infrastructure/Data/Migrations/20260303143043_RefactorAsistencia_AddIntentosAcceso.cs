using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManager.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAsistencia_AddIntentosAcceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "Resultado",
                table: "Asistencias");

            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Asistencias",
                newName: "FechaRegistro");

            migrationBuilder.CreateTable(
                name: "IntentosAccesos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DniIngresado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    SocioId = table.Column<int>(type: "int", nullable: true),
                    Resultado = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosAccesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntentosAccesos_Socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntentosAccesos_DniIngresado",
                table: "IntentosAccesos",
                column: "DniIngresado");

            migrationBuilder.CreateIndex(
                name: "IX_IntentosAccesos_FechaRegistro",
                table: "IntentosAccesos",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_IntentosAccesos_SocioId",
                table: "IntentosAccesos",
                column: "SocioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntentosAccesos");

            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "Asistencias",
                newName: "FechaHora");

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "Asistencias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resultado",
                table: "Asistencias",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
