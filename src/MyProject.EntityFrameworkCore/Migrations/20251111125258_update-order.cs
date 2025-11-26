using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Migrations
{
    /// <inheritdoc />
    public partial class updateorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "AppOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "AppOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidTime",
                table: "AppOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentExpiredAt",
                table: "AppOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "AppOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AppOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "AppOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "PaidTime",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "PaymentExpiredAt",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AppOrders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "AppOrders");
        }
    }
}
