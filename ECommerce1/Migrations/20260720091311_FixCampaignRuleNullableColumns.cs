using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class FixCampaignRuleNullableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs before altering columns (required by SQL Server)
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Brands_BrandId",       "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Categories_CategoryId", "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Products_ProductId",    "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Brands_BrandId",         "CampaignMainProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Categories_CategoryId",  "CampaignMainProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Products_ProductId",     "CampaignMainProductRules");

            // Drop indexes before altering
            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_ProductId",  "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_CategoryId", "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_BrandId",    "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_ProductId",   "CampaignMainProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_CategoryId",  "CampaignMainProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_BrandId",     "CampaignMainProductRules");

            // ALTER columns to nullable
            migrationBuilder.AlterColumn<int>("ProductId",  "CampaignAddonProductRules", nullable: true, oldClrType: typeof(int), oldNullable: false);
            migrationBuilder.AlterColumn<int>("CategoryId", "CampaignAddonProductRules", nullable: true, oldClrType: typeof(int), oldNullable: false);
            migrationBuilder.AlterColumn<int>("BrandId",    "CampaignAddonProductRules", nullable: true, oldClrType: typeof(int), oldNullable: false);
            migrationBuilder.AlterColumn<int>("ProductId",  "CampaignMainProductRules",  nullable: true, oldClrType: typeof(int), oldNullable: false);
            migrationBuilder.AlterColumn<int>("CategoryId", "CampaignMainProductRules",  nullable: true, oldClrType: typeof(int), oldNullable: false);
            migrationBuilder.AlterColumn<int>("BrandId",    "CampaignMainProductRules",  nullable: true, oldClrType: typeof(int), oldNullable: false);

            // Re-create indexes
            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_ProductId",  "CampaignAddonProductRules", "ProductId");
            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_CategoryId", "CampaignAddonProductRules", "CategoryId");
            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_BrandId",    "CampaignAddonProductRules", "BrandId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_ProductId",   "CampaignMainProductRules",  "ProductId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_CategoryId",  "CampaignMainProductRules",  "CategoryId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_BrandId",     "CampaignMainProductRules",  "BrandId");

            // Re-add FKs (nullable -> no cascade, just restrict)
            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Brands_BrandId",       "CampaignAddonProductRules", "BrandId",    "Brands",     principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Categories_CategoryId","CampaignAddonProductRules", "CategoryId", "Categories", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Products_ProductId",   "CampaignAddonProductRules", "ProductId",  "Products",   principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Brands_BrandId",        "CampaignMainProductRules",  "BrandId",    "Brands",     principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Categories_CategoryId", "CampaignMainProductRules",  "CategoryId", "Categories", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Products_ProductId",    "CampaignMainProductRules",  "ProductId",  "Products",   principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Brands_BrandId",       "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Categories_CategoryId", "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignAddonProductRules_Products_ProductId",    "CampaignAddonProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Brands_BrandId",         "CampaignMainProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Categories_CategoryId",  "CampaignMainProductRules");
            migrationBuilder.DropForeignKey("FK_CampaignMainProductRules_Products_ProductId",     "CampaignMainProductRules");

            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_ProductId",  "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_CategoryId", "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignAddonProductRules_BrandId",    "CampaignAddonProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_ProductId",   "CampaignMainProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_CategoryId",  "CampaignMainProductRules");
            migrationBuilder.DropIndex("IX_CampaignMainProductRules_BrandId",     "CampaignMainProductRules");

            migrationBuilder.AlterColumn<int>("ProductId",  "CampaignAddonProductRules", nullable: false, oldClrType: typeof(int), oldNullable: true);
            migrationBuilder.AlterColumn<int>("CategoryId", "CampaignAddonProductRules", nullable: false, oldClrType: typeof(int), oldNullable: true);
            migrationBuilder.AlterColumn<int>("BrandId",    "CampaignAddonProductRules", nullable: false, oldClrType: typeof(int), oldNullable: true);
            migrationBuilder.AlterColumn<int>("ProductId",  "CampaignMainProductRules",  nullable: false, oldClrType: typeof(int), oldNullable: true);
            migrationBuilder.AlterColumn<int>("CategoryId", "CampaignMainProductRules",  nullable: false, oldClrType: typeof(int), oldNullable: true);
            migrationBuilder.AlterColumn<int>("BrandId",    "CampaignMainProductRules",  nullable: false, oldClrType: typeof(int), oldNullable: true);

            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_ProductId",  "CampaignAddonProductRules", "ProductId");
            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_CategoryId", "CampaignAddonProductRules", "CategoryId");
            migrationBuilder.CreateIndex("IX_CampaignAddonProductRules_BrandId",    "CampaignAddonProductRules", "BrandId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_ProductId",   "CampaignMainProductRules",  "ProductId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_CategoryId",  "CampaignMainProductRules",  "CategoryId");
            migrationBuilder.CreateIndex("IX_CampaignMainProductRules_BrandId",     "CampaignMainProductRules",  "BrandId");

            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Brands_BrandId",       "CampaignAddonProductRules", "BrandId",    "Brands",     principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Categories_CategoryId","CampaignAddonProductRules", "CategoryId", "Categories", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_CampaignAddonProductRules_Products_ProductId",   "CampaignAddonProductRules", "ProductId",  "Products",   principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Brands_BrandId",        "CampaignMainProductRules",  "BrandId",    "Brands",     principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Categories_CategoryId", "CampaignMainProductRules",  "CategoryId", "Categories", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_CampaignMainProductRules_Products_ProductId",    "CampaignMainProductRules",  "ProductId",  "Products",   principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }
    }
}
