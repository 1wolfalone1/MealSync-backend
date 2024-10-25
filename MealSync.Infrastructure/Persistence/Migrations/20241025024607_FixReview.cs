using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Customer",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Order",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction");

            migrationBuilder.AlterColumn<long>(
                name: "wallet_from_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "review",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "customer_id",
                table: "review",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "entity",
                table: "review",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "shop_id",
                table: "review",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_review_shop_id",
                table: "review",
                column: "shop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Customer",
                table: "review",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction",
                column: "wallet_from_id",
                principalTable: "wallet",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Customer",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Order",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Shop",
                table: "review");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction");

            migrationBuilder.DropIndex(
                name: "ix_review_shop_id",
                table: "review");

            migrationBuilder.DropColumn(
                name: "entity",
                table: "review");

            migrationBuilder.DropColumn(
                name: "shop_id",
                table: "review");

            migrationBuilder.AlterColumn<long>(
                name: "wallet_from_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "review",
                keyColumn: "image_url",
                keyValue: null,
                column: "image_url",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "review",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "customer_id",
                table: "review",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Customer",
                table: "review",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Order",
                table: "review",
                column: "order_id",
                principalTable: "order",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction",
                column: "wallet_from_id",
                principalTable: "wallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
