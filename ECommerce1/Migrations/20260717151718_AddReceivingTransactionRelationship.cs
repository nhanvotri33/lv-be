using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddReceivingTransactionRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetail_ReceivingDetailId",
                table: "InventoryDetail",
                column: "ReceivingDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryDetail_InventoryTransactions_ReceivingDetailId",
                table: "InventoryDetail",
                column: "ReceivingDetailId",
                principalTable: "InventoryTransactions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryDetail_InventoryTransactions_ReceivingDetailId",
                table: "InventoryDetail");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDetail_ReceivingDetailId",
                table: "InventoryDetail");
        }
    }
}
