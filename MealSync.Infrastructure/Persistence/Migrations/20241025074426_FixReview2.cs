using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReview2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Order",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Shop",
                table: "review");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Order",
                table: "review",
                column: "order_id",
                principalTable: "order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Shop",
                table: "review",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Order",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Shop",
                table: "review");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Order",
                table: "review",
                column: "order_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Shop",
                table: "review",
                column: "shop_id",
                principalTable: "order",
                principalColumn: "id");
        }
    }
}
