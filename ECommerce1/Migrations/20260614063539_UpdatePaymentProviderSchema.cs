using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce1.Migrations
{
    public partial class UpdatePaymentProviderSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripeSessionId",
                table: "Payments",
                newName: "ProviderTransactionId");

            migrationBuilder.RenameColumn(
                name: "StripePaymentIntentId",
                table: "Payments",
                newName: "ProviderSessionId");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ProviderTransactionId",
                table: "Payments",
                newName: "StripeSessionId");

            migrationBuilder.RenameColumn(
                name: "ProviderSessionId",
                table: "Payments",
                newName: "StripePaymentIntentId");
        }
    }
}
