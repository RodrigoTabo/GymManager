using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManager.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEliminadoEnToSocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentoId",
                table: "Socios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EliminadoEn",
                table: "Socios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoId",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "EliminadoEn",
                table: "Socios");
        }
    }
}
