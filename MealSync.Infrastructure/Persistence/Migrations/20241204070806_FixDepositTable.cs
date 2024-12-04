using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDepositTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payment_thirdparty_id",
                table: "deposit",
                newName: "payment_third_party_id");

            migrationBuilder.RenameColumn(
                name: "payment_thirdparty_content",
                table: "deposit",
                newName: "payment_third_party_content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "payment_third_party_id",
                table: "deposit",
                newName: "payment_thirdparty_id");

            migrationBuilder.RenameColumn(
                name: "payment_third_party_content",
                table: "deposit",
                newName: "payment_thirdparty_content");
        }
    }
}
