using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddProductComboSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppliedComboId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ComboDiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AppliedComboId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCombos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCombos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductComboItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductComboId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComboItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductComboItems_ProductCombos_ProductComboId",
                        column: x => x.ProductComboId,
                        principalTable: "ProductCombos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductComboItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_AppliedComboId",
                table: "OrderItems",
                column: "AppliedComboId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_AppliedComboId",
                table: "CartItems",
                column: "AppliedComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComboItems_ProductComboId_ProductId",
                table: "ProductComboItems",
                columns: new[] { "ProductComboId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComboItems_ProductId",
                table: "ProductComboItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductCombos_AppliedComboId",
                table: "CartItems",
                column: "AppliedComboId",
                principalTable: "ProductCombos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductCombos_AppliedComboId",
                table: "OrderItems",
                column: "AppliedComboId",
                principalTable: "ProductCombos",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductCombos_AppliedComboId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductCombos_AppliedComboId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductComboItems");

            migrationBuilder.DropTable(
                name: "ProductCombos");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_AppliedComboId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_AppliedComboId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "AppliedComboId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ComboDiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AppliedComboId",
                table: "CartItems");
        }
    }
}
