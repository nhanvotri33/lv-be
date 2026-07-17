using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class AddShippingCarrierToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingCarrier",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCarrier",
                table: "Orders");
        }
    }
}
