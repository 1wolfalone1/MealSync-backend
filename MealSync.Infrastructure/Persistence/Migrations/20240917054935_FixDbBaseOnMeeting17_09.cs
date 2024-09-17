using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDbBaseOnMeeting17_09 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Dormitory",
                table: "customer");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrderCombination_StaffDelivery",
                table: "delivery_order_combination");

            migrationBuilder.DropTable(
                name: "Wallet_history");

            migrationBuilder.DropTable(
                name: "moderator_activity_log");

            migrationBuilder.DropTable(
                name: "verification_code");

            migrationBuilder.DropIndex(
                name: "ix_customer_dormitory_id",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "dormitory_id",
                table: "customer");

            migrationBuilder.RenameColumn(
                name: "in_coming_amount",
                table: "wallet",
                newName: "incoming_amount");

            migrationBuilder.AddColumn<double>(
                name: "avaiable_amount_before",
                table: "wallet_transaction",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "incoming_amount_before",
                table: "wallet_transaction",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "reporting_amount_before",
                table: "wallet_transaction",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "reporting_amount",
                table: "wallet",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "resource_type",
                table: "system_resource",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "staff_delivery_id",
                table: "delivery_order_combination",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "shop_owner_id",
                table: "delivery_order_combination",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "activity_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    action_type = table.Column<int>(type: "int", nullable: false),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<long>(type: "bigint", nullable: true),
                    action_detail = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_ActivityLog_Account",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_order_combination_shop_owner_id",
                table: "delivery_order_combination",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_log_account_id",
                table: "activity_log",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrderCombination_StaffDelivery",
                table: "delivery_order_combination",
                column: "staff_delivery_id",
                principalTable: "staff_delivery",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_delivery_order_combination_shop_owner_shop_owner_id",
                table: "delivery_order_combination",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrderCombination_StaffDelivery",
                table: "delivery_order_combination");

            migrationBuilder.DropForeignKey(
                name: "fk_delivery_order_combination_shop_owner_shop_owner_id",
                table: "delivery_order_combination");

            migrationBuilder.DropTable(
                name: "activity_log");

            migrationBuilder.DropIndex(
                name: "ix_delivery_order_combination_shop_owner_id",
                table: "delivery_order_combination");

            migrationBuilder.DropColumn(
                name: "avaiable_amount_before",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "incoming_amount_before",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "reporting_amount_before",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "reporting_amount",
                table: "wallet");

            migrationBuilder.DropColumn(
                name: "resource_type",
                table: "system_resource");

            migrationBuilder.DropColumn(
                name: "shop_owner_id",
                table: "delivery_order_combination");

            migrationBuilder.RenameColumn(
                name: "incoming_amount",
                table: "wallet",
                newName: "in_coming_amount");

            migrationBuilder.AlterColumn<long>(
                name: "staff_delivery_id",
                table: "delivery_order_combination",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "dormitory_id",
                table: "customer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Wallet_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    available_amount_after = table.Column<double>(type: "double", nullable: false),
                    available_amount_before = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    in_coming_amount_after = table.Column<double>(type: "double", nullable: false),
                    in_coming_amount_before = table.Column<double>(type: "double", nullable: false),
                    next_transfer_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_history", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "moderator_activity_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    moderator_id = table.Column<long>(type: "bigint", nullable: false),
                    action_detail = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    action_type = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    is_success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    target_id = table.Column<long>(type: "bigint", nullable: true),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moderator_activity_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_ModeratorActivityLog_Moderator",
                        column: x => x.moderator_id,
                        principalTable: "moderator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "verification_code",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    expired_tịme = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verification_code", x => x.id);
                    table.ForeignKey(
                        name: "FK_Account_VerificationCode",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_customer_dormitory_id",
                table: "customer",
                column: "dormitory_id");

            migrationBuilder.CreateIndex(
                name: "ix_moderator_activity_log_moderator_id",
                table: "moderator_activity_log",
                column: "moderator_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_code_account_id",
                table: "verification_code",
                column: "account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Dormitory",
                table: "customer",
                column: "dormitory_id",
                principalTable: "dormitory",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrderCombination_StaffDelivery",
                table: "delivery_order_combination",
                column: "staff_delivery_id",
                principalTable: "staff_delivery",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
