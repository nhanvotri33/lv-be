using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class RenameInventoryDetailToStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "InventoryDetail",
                newName: "Stock");

            migrationBuilder.RenameColumn(
                table: "Stock",
                name: "InventoryDetailId",
                newName: "StockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Stock",
                newName: "InventoryDetail");

            migrationBuilder.RenameColumn(
                table: "InventoryDetail",
                name: "StockId",
                newName: "InventoryDetailId");
        }
    }
}
