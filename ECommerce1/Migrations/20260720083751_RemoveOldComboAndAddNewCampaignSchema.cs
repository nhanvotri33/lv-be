using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class RemoveOldComboAndAddNewCampaignSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "ComboDiscountAmount",
                table: "OrderItems",
                newName: "CampaignDiscountAmount");

            migrationBuilder.RenameColumn(
                name: "AppliedComboId",
                table: "OrderItems",
                newName: "ParentOrderItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_AppliedComboId",
                table: "OrderItems",
                newName: "IX_OrderItems_ParentOrderItemId");

            migrationBuilder.RenameColumn(
                name: "AppliedComboId",
                table: "CartItems",
                newName: "ParentCartItemId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_AppliedComboId",
                table: "CartItems",
                newName: "IX_CartItems_ParentCartItemId");

            migrationBuilder.AddColumn<decimal>(
                name: "AddonDiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AppliedCampaignId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddon",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AppliedCampaignId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddon",
                table: "CartItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PromotionCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxQuantityAllowed = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignAddonProductRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAddonProductRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAddonProductRules_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignAddonProductRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignAddonProductRules_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignAddonProductRules_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignMainProductRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMainProductRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignMainProductRules_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignMainProductRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignMainProductRules_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignMainProductRules_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_AppliedCampaignId",
                table: "OrderItems",
                column: "AppliedCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_AppliedCampaignId",
                table: "CartItems",
                column: "AppliedCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAddonProductRules_BrandId",
                table: "CampaignAddonProductRules",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAddonProductRules_CampaignId",
                table: "CampaignAddonProductRules",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAddonProductRules_CategoryId",
                table: "CampaignAddonProductRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAddonProductRules_ProductId",
                table: "CampaignAddonProductRules",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMainProductRules_BrandId",
                table: "CampaignMainProductRules",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMainProductRules_CampaignId",
                table: "CampaignMainProductRules",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMainProductRules_CategoryId",
                table: "CampaignMainProductRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMainProductRules_ProductId",
                table: "CampaignMainProductRules",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CartItems_ParentCartItemId",
                table: "CartItems",
                column: "ParentCartItemId",
                principalTable: "CartItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_PromotionCampaigns_AppliedCampaignId",
                table: "CartItems",
                column: "AppliedCampaignId",
                principalTable: "PromotionCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderItems_ParentOrderItemId",
                table: "OrderItems",
                column: "ParentOrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_PromotionCampaigns_AppliedCampaignId",
                table: "OrderItems",
                column: "AppliedCampaignId",
                principalTable: "PromotionCampaigns",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CartItems_ParentCartItemId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_PromotionCampaigns_AppliedCampaignId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderItems_ParentOrderItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_PromotionCampaigns_AppliedCampaignId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "CampaignAddonProductRules");

            migrationBuilder.DropTable(
                name: "CampaignMainProductRules");

            migrationBuilder.DropTable(
                name: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_AppliedCampaignId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_AppliedCampaignId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "AddonDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AppliedCampaignId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsAddon",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "AppliedCampaignId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "IsAddon",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "ParentOrderItemId",
                table: "OrderItems",
                newName: "AppliedComboId");

            migrationBuilder.RenameColumn(
                name: "CampaignDiscountAmount",
                table: "OrderItems",
                newName: "ComboDiscountAmount");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ParentOrderItemId",
                table: "OrderItems",
                newName: "IX_OrderItems_AppliedComboId");

            migrationBuilder.RenameColumn(
                name: "ParentCartItemId",
                table: "CartItems",
                newName: "AppliedComboId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ParentCartItemId",
                table: "CartItems",
                newName: "IX_CartItems_AppliedComboId");

            migrationBuilder.CreateTable(
                name: "ProductCombos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    DiscountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false)
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
    }
}
