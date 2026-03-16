using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManager.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefonoInSocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "Socios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Telefono",
                table: "Socios",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "Planes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "Pagos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "MetodosPago",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "IntentosAccesos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "DocumentosSocio",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "Asistencias",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Planes");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "IntentosAccesos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "DocumentosSocio");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Asistencias");
        }
    }
}
