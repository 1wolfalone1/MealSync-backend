using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPaidToShopAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction");

            migrationBuilder.AddColumn<bool>(
                name: "is_paid_to_shop",
                table: "order",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "is_paid_to_shop",
                table: "order");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction",
                column: "payment_id",
                unique: true);
        }
    }
}
