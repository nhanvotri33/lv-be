using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddVariantIdToInventoryDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "InventoryDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetail_VariantId",
                table: "InventoryDetail",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryDetail_ProductVariants_VariantId",
                table: "InventoryDetail",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryDetail_ProductVariants_VariantId",
                table: "InventoryDetail");

            migrationBuilder.DropIndex(
                name: "IX_InventoryDetail_VariantId",
                table: "InventoryDetail");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "InventoryDetail");
        }
    }
}
