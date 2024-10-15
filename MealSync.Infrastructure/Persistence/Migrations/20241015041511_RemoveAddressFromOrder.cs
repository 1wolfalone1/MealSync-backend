using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressFromOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "order");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "order");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "order",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "order",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "order",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
