using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnBankAccountName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_account_name",
                table: "withdrawal_request",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "bank_account_name",
                table: "shop",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_account_name",
                table: "withdrawal_request");

            migrationBuilder.DropColumn(
                name: "bank_account_name",
                table: "shop");
        }
    }
}
