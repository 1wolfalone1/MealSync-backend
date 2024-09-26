using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixShop_OperatingDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_operating_day_shop_owner_id",
                table: "operating_day",
                column: "shop_owner_id");

            migrationBuilder.AddForeignKey(
                name: "FK_OperatingDay_ShopOwner",
                table: "operating_day",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperatingDay_ShopOwner",
                table: "operating_day");

            migrationBuilder.DropIndex(
                name: "ix_operating_day_shop_owner_id",
                table: "operating_day");
        }
    }
}
