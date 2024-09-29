using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixBuilding_Order_Account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "able_total_order_handle",
                table: "operating_frame");

            migrationBuilder.DropColumn(
                name: "able_total_order",
                table: "operating_day");

            migrationBuilder.RenameColumn(
                name: "domitory_id",
                table: "building",
                newName: "dormitory_id");

            migrationBuilder.RenameIndex(
                name: "ix_building_domitory_id",
                table: "building",
                newName: "ix_building_dormitory_id");

            migrationBuilder.AddColumn<int>(
                name: "average_total_order_handle_in_day",
                table: "shop_owner",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "qr_scan_to_deliveried",
                table: "order",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "refresh_token",
                table: "account",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "average_total_order_handle_in_day",
                table: "shop_owner");

            migrationBuilder.DropColumn(
                name: "qr_scan_to_deliveried",
                table: "order");

            migrationBuilder.RenameColumn(
                name: "dormitory_id",
                table: "building",
                newName: "domitory_id");

            migrationBuilder.RenameIndex(
                name: "ix_building_dormitory_id",
                table: "building",
                newName: "ix_building_domitory_id");

            migrationBuilder.AddColumn<int>(
                name: "able_total_order_handle",
                table: "operating_frame",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "able_total_order",
                table: "operating_day",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "account",
                keyColumn: "refresh_token",
                keyValue: null,
                column: "refresh_token",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "refresh_token",
                table: "account",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
