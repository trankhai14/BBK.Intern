using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class addmanagerwarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReferenceId",
                table: "AppInventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "AppInventoryTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppExportSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppExportSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExportSlips_AppOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "AppOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppExportSlips_AppSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "AppSuppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppImportSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppImportSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppImportSlips_AppSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "AppSuppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppStocktakings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StocktakingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    AssignedTo = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppStocktakings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStocktakings_AbpUsers_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppExportDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportSlipId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExportDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExportDetails_AppExportSlips_ExportSlipId",
                        column: x => x.ExportSlipId,
                        principalTable: "AppExportSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppExportDetails_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppImportDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportSlipId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppImportDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppImportDetails_AppImportSlips_ImportSlipId",
                        column: x => x.ImportSlipId,
                        principalTable: "AppImportSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppImportDetails_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStocktakingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StocktakingId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SystemQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualQuantity = table.Column<int>(type: "int", nullable: false),
                    Difference = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAdjusted = table.Column<bool>(type: "bit", nullable: false),
                    AdjustedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStocktakingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStocktakingDetails_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppStocktakingDetails_AppStocktakings_StocktakingId",
                        column: x => x.StocktakingId,
                        principalTable: "AppStocktakings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExportDetails_ExportSlipId",
                table: "AppExportDetails",
                column: "ExportSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExportDetails_ProductId",
                table: "AppExportDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExportSlips_OrderId",
                table: "AppExportSlips",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExportSlips_SupplierId",
                table: "AppExportSlips",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppImportDetails_ImportSlipId",
                table: "AppImportDetails",
                column: "ImportSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_AppImportDetails_ProductId",
                table: "AppImportDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppImportSlips_SupplierId",
                table: "AppImportSlips",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocktakingDetails_ProductId",
                table: "AppStocktakingDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocktakingDetails_StocktakingId",
                table: "AppStocktakingDetails",
                column: "StocktakingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocktakings_AssignedTo",
                table: "AppStocktakings",
                column: "AssignedTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExportDetails");

            migrationBuilder.DropTable(
                name: "AppImportDetails");

            migrationBuilder.DropTable(
                name: "AppStocktakingDetails");

            migrationBuilder.DropTable(
                name: "AppExportSlips");

            migrationBuilder.DropTable(
                name: "AppImportSlips");

            migrationBuilder.DropTable(
                name: "AppStocktakings");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "AppInventoryTransactions");
        }
    }
}
