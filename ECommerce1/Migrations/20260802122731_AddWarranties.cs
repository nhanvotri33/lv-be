using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddWarranties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerDeviceId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionStatus",
                table: "OrderItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WarrantyId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WarrantyPrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImeiOrSerial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDevices_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiresInspection = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyPackageRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarrantyId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    MinPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyPackageRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyPackageRules_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyPackageRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyPackageRules_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyPackageRules_Warranties_WarrantyId",
                        column: x => x.WarrantyId,
                        principalTable: "Warranties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CustomerDeviceId",
                table: "OrderItems",
                column: "CustomerDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_WarrantyId",
                table: "OrderItems",
                column: "WarrantyId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_WarrantyId",
                table: "CartItems",
                column: "WarrantyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDevices_UserId",
                table: "CustomerDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDevices_VariantId",
                table: "CustomerDevices",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPackageRules_BrandId",
                table: "WarrantyPackageRules",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPackageRules_CategoryId",
                table: "WarrantyPackageRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPackageRules_ProductId",
                table: "WarrantyPackageRules",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyPackageRules_WarrantyId",
                table: "WarrantyPackageRules",
                column: "WarrantyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Warranties_WarrantyId",
                table: "CartItems",
                column: "WarrantyId",
                principalTable: "Warranties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_CustomerDevices_CustomerDeviceId",
                table: "OrderItems",
                column: "CustomerDeviceId",
                principalTable: "CustomerDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Warranties_WarrantyId",
                table: "OrderItems",
                column: "WarrantyId",
                principalTable: "Warranties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Warranties_WarrantyId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_CustomerDevices_CustomerDeviceId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Warranties_WarrantyId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "CustomerDevices");

            migrationBuilder.DropTable(
                name: "WarrantyPackageRules");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_CustomerDeviceId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_WarrantyId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_WarrantyId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CustomerDeviceId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "InspectionStatus",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "WarrantyId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "WarrantyPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "WarrantyId",
                table: "CartItems");
        }
    }
}
