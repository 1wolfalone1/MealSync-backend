using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameTableShopDeliveryStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPackage_StaffDelivery",
                table: "delivery_package");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_StaffDelivery",
                table: "report");

            migrationBuilder.DropTable(
                name: "staff_delivery");

            migrationBuilder.RenameColumn(
                name: "staff_delivery_id",
                table: "report",
                newName: "shop_delivery_staff_id");

            migrationBuilder.RenameIndex(
                name: "ix_report_staff_delivery_id",
                table: "report",
                newName: "ix_report_shop_delivery_staff_id");

            migrationBuilder.RenameColumn(
                name: "staff_delivery_id",
                table: "delivery_package",
                newName: "shop_delivery_staff_id");

            migrationBuilder.RenameIndex(
                name: "ix_delivery_package_staff_delivery_id",
                table: "delivery_package",
                newName: "ix_delivery_package_shop_delivery_staff_id");

            migrationBuilder.CreateTable(
                name: "shop_delivery_staff",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_delivery_staff", x => x.id);
                    table.ForeignKey(
                        name: "FK_ShopDeliveryStaff_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopDeliveryStaff_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_shop_delivery_staff_shop_id",
                table: "shop_delivery_staff",
                column: "shop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPackage_ShopDeliveryStaff",
                table: "delivery_package",
                column: "shop_delivery_staff_id",
                principalTable: "shop_delivery_staff",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_ShopDeliveryStaff",
                table: "report",
                column: "shop_delivery_staff_id",
                principalTable: "shop_delivery_staff",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryPackage_ShopDeliveryStaff",
                table: "delivery_package");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_ShopDeliveryStaff",
                table: "report");

            migrationBuilder.DropTable(
                name: "shop_delivery_staff");

            migrationBuilder.RenameColumn(
                name: "shop_delivery_staff_id",
                table: "report",
                newName: "staff_delivery_id");

            migrationBuilder.RenameIndex(
                name: "ix_report_shop_delivery_staff_id",
                table: "report",
                newName: "ix_report_staff_delivery_id");

            migrationBuilder.RenameColumn(
                name: "shop_delivery_staff_id",
                table: "delivery_package",
                newName: "staff_delivery_id");

            migrationBuilder.RenameIndex(
                name: "ix_delivery_package_shop_delivery_staff_id",
                table: "delivery_package",
                newName: "ix_delivery_package_staff_delivery_id");

            migrationBuilder.CreateTable(
                name: "staff_delivery",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_delivery", x => x.id);
                    table.ForeignKey(
                        name: "FK_StaffDelivery_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffDelivery_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_staff_delivery_shop_id",
                table: "staff_delivery",
                column: "shop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryPackage_StaffDelivery",
                table: "delivery_package",
                column: "staff_delivery_id",
                principalTable: "staff_delivery",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_StaffDelivery",
                table: "report",
                column: "staff_delivery_id",
                principalTable: "staff_delivery",
                principalColumn: "id");
        }
    }
}
