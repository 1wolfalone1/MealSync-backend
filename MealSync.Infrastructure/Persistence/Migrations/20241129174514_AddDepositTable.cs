using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "deposit_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "deposit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_thirdparty_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_thirdparty_content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deposit", x => x.id);
                    table.ForeignKey(
                        name: "FK_Deposit_Wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_deposit_id",
                table: "wallet_transaction",
                column: "deposit_id");

            migrationBuilder.CreateIndex(
                name: "ix_deposit_wallet_id",
                table: "deposit",
                column: "wallet_id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Deposit",
                table: "wallet_transaction",
                column: "deposit_id",
                principalTable: "deposit",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Deposit",
                table: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "deposit");

            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_deposit_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "deposit_id",
                table: "wallet_transaction");
        }
    }
}
