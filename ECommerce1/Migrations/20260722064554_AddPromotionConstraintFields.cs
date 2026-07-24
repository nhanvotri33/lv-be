using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddPromotionConstraintFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Brands_BrandId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Categories_CategoryId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Products_ProductId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Brands_BrandId",
                table: "CampaignMainProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Categories_CategoryId",
                table: "CampaignMainProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Products_ProductId",
                table: "CampaignMainProductRules");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "Promotions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPerUser",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderAmount",
                table: "Promotions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "PromotionCampaigns",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BrandId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BrandId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Brands_BrandId",
                table: "CampaignAddonProductRules",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Categories_CategoryId",
                table: "CampaignAddonProductRules",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Products_ProductId",
                table: "CampaignAddonProductRules",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Brands_BrandId",
                table: "CampaignMainProductRules",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Categories_CategoryId",
                table: "CampaignMainProductRules",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Products_ProductId",
                table: "CampaignMainProductRules",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Brands_BrandId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Categories_CategoryId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignAddonProductRules_Products_ProductId",
                table: "CampaignAddonProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Brands_BrandId",
                table: "CampaignMainProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Categories_CategoryId",
                table: "CampaignMainProductRules");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignMainProductRules_Products_ProductId",
                table: "CampaignMainProductRules");

            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxPerUser",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MinOrderAmount",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "PromotionCampaigns");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BrandId",
                table: "CampaignMainProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BrandId",
                table: "CampaignAddonProductRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Brands_BrandId",
                table: "CampaignAddonProductRules",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Categories_CategoryId",
                table: "CampaignAddonProductRules",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignAddonProductRules_Products_ProductId",
                table: "CampaignAddonProductRules",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Brands_BrandId",
                table: "CampaignMainProductRules",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Categories_CategoryId",
                table: "CampaignMainProductRules",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignMainProductRules_Products_ProductId",
                table: "CampaignMainProductRules",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
