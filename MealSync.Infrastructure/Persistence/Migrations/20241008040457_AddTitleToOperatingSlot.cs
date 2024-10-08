using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToOperatingSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Promotion",
                table: "order");

            migrationBuilder.RenameColumn(
                name: "toltal_price",
                table: "order_detail",
                newName: "total_price");

            migrationBuilder.AlterColumn<long>(
                name: "promotion_id",
                table: "order",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "operating_slot",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Promotion",
                table: "order",
                column: "promotion_id",
                principalTable: "promotion",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Promotion",
                table: "order");

            migrationBuilder.DropColumn(
                name: "title",
                table: "operating_slot");

            migrationBuilder.RenameColumn(
                name: "total_price",
                table: "order_detail",
                newName: "toltal_price");

            migrationBuilder.AlterColumn<long>(
                name: "promotion_id",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Promotion",
                table: "order",
                column: "promotion_id",
                principalTable: "promotion",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
