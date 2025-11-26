using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class addsupplierandupdateproduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Battery",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera",
                table: "AppProducts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Charging",
                table: "AppProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChargingPort",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Chipset",
                table: "AppProducts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Connectivity",
                table: "AppProducts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontCamera",
                table: "AppProducts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "AppProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ram",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Screen",
                table: "AppProducts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Security",
                table: "AppProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sim",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "AppProducts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Storage",
                table: "AppProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "AppProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalSpecifications",
                table: "AppProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Warranty",
                table: "AppProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSuppliers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_SupplierId",
                table: "AppProducts",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppProducts_AppSuppliers_SupplierId",
                table: "AppProducts",
                column: "SupplierId",
                principalTable: "AppSuppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProducts_AppSuppliers_SupplierId",
                table: "AppProducts");

            migrationBuilder.DropTable(
                name: "AppSuppliers");

            migrationBuilder.DropIndex(
                name: "IX_AppProducts_SupplierId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Battery",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Camera",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Charging",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "ChargingPort",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Chipset",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Connectivity",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "FrontCamera",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Ram",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Screen",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Security",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Sim",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Storage",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "TechnicalSpecifications",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Warranty",
                table: "AppProducts");
        }
    }
}
