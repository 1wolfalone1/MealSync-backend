using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "max_apply_value",
                table: "promotion",
                newName: "maximum_apply_value");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "wallet",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "wallet");

            migrationBuilder.RenameColumn(
                name: "maximum_apply_value",
                table: "promotion",
                newName: "max_apply_value");
        }
    }
}
