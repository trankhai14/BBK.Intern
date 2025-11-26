using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class addproductspecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "TechnicalSpecifications",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "Warranty",
                table: "AppProducts");

            migrationBuilder.CreateTable(
                name: "AppProductSpecifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModelNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Chipset = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Ram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Storage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Screen = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Battery = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Camera = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FrontCamera = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Sim = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Connectivity = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Security = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Charging = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChargingPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Warranty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TechnicalSpecifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProductSpecifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductSpecifications_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProductSpecifications_ProductId",
                table: "AppProductSpecifications",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProductSpecifications");

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
        }
    }
}
