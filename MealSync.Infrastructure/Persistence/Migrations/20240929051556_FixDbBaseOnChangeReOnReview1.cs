using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDbBaseOnChangeReOnReview1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourite_ShopOwner",
                table: "favourtite");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_DeliveryOrderCombination",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_ShopOwner",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_ShopOwner",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotion_ShopOwner",
                table: "promotion");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_ShopOwner",
                table: "report");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopDormitory_ShopOwner",
                table: "shop_dormitory");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffDelivery_ShopOwner",
                table: "staff_delivery");

            migrationBuilder.DropTable(
                name: "delivery_order_combination");

            migrationBuilder.DropTable(
                name: "operating_frame");

            migrationBuilder.DropTable(
                name: "order_detail_option");

            migrationBuilder.DropTable(
                name: "order_transaction");

            migrationBuilder.DropTable(
                name: "order_transaction_history");

            migrationBuilder.DropTable(
                name: "product_category");

            migrationBuilder.DropTable(
                name: "product_operating_hours");

            migrationBuilder.DropTable(
                name: "topping_option");

            migrationBuilder.DropTable(
                name: "operating_day");

            migrationBuilder.DropTable(
                name: "topping_question");

            migrationBuilder.DropTable(
                name: "shop_owner");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "staff_delivery",
                newName: "shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_staff_delivery_shop_owner_id",
                table: "staff_delivery",
                newName: "ix_staff_delivery_shop_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "shop_dormitory",
                newName: "shop_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "report",
                newName: "shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_report_shop_owner_id",
                table: "report",
                newName: "ix_report_shop_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "promotion",
                newName: "shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_promotion_shop_owner_id",
                table: "promotion",
                newName: "ix_promotion_shop_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "product",
                newName: "shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_product_shop_owner_id",
                table: "product",
                newName: "ix_product_shop_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "order",
                newName: "shop_id");

            migrationBuilder.RenameColumn(
                name: "delivery_order_combination_id",
                table: "order",
                newName: "delivery_package_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_shop_owner_id",
                table: "order",
                newName: "ix_order_shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_delivery_order_combination_id",
                table: "order",
                newName: "ix_order_delivery_package_id");

            migrationBuilder.RenameColumn(
                name: "shop_owner_id",
                table: "favourtite",
                newName: "shop_id");

            migrationBuilder.RenameIndex(
                name: "ix_favourtite_shop_owner_id",
                table: "favourtite",
                newName: "ix_favourtite_shop_id");

            migrationBuilder.AddColumn<long>(
                name: "payment_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "withdrawal_request_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_flags_before_ban",
                table: "system_config",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "time_frame_duration",
                table: "system_config",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "category_id",
                table: "product",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_topping",
                table: "product",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "parent_id",
                table: "product",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "shop_category_id",
                table: "product",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "parent_order_detail_id",
                table: "order_detail",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "category",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    payment_methods = table.Column<int>(type: "int", nullable: false),
                    payment_third_party_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_third_party_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payment_Order",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payment_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    payment_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    payment_methods = table.Column<int>(type: "int", nullable: false),
                    payment_third_party_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_third_party_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_history", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_variant",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_variant", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductVariant_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logo_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    banner_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_short_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_account_number = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_order = table.Column<int>(type: "int", nullable: false),
                    total_product = table.Column<int>(type: "int", nullable: false),
                    total_review = table.Column<int>(type: "int", nullable: false),
                    total_rating = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    is_accepting_order_next_day = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_receiving_order_paused = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    min_value_order_free_ship = table.Column<double>(type: "double", nullable: false),
                    additional_ship_fee = table.Column<double>(type: "double", nullable: false),
                    is_auto_order_confirmation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    max_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    min_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    num_of_warning = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop", x => x.id);
                    table.ForeignKey(
                        name: "FK_Shop_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shop_Location",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shop_Wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_variant_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_variant_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductVariantOption_ProductVariant",
                        column: x => x.product_variant_id,
                        principalTable: "product_variant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_package",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    staff_delivery_id = table.Column<long>(type: "bigint", nullable: true),
                    shop_id = table.Column<long>(type: "bigint", nullable: true),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_package", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeliveryPackage_StaffDelivery",
                        column: x => x.staff_delivery_id,
                        principalTable: "staff_delivery",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_delivery_package_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operating_slot",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operating_slot", x => x.id);
                    table.ForeignKey(
                        name: "FK_OperatingSlot_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_ShopCategory_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_detail_product_variant",
                columns: table => new
                {
                    order_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    p_variant_option_id = table.Column<long>(type: "bigint", nullable: false),
                    p_variant_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    p_variant_option_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    p_variant_option_image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_detail_product_variant", x => new { x.order_detail_id, x.p_variant_option_id });
                    table.ForeignKey(
                        name: "FK_OrderDetailProductVariant_OrderDetail",
                        column: x => x.order_detail_id,
                        principalTable: "order_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetailProductVariant_ProductVariantOption",
                        column: x => x.p_variant_option_id,
                        principalTable: "product_variant_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_operating_slot",
                columns: table => new
                {
                    operating_slot_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_operating_slot", x => new { x.operating_slot_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_ProductOperatingSlot_OperatingSlot",
                        column: x => x.operating_slot_id,
                        principalTable: "operating_slot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOperatingSlot_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_withdrawal_request_id",
                table: "wallet_transaction",
                column: "withdrawal_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_category_id",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_parent_id",
                table: "product",
                column: "parent_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_shop_category_id",
                table: "product",
                column: "shop_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_parent_order_detail_id",
                table: "order_detail",
                column: "parent_order_detail_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_delivery_package_shop_id",
                table: "delivery_package",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_package_staff_delivery_id",
                table: "delivery_package",
                column: "staff_delivery_id");

            migrationBuilder.CreateIndex(
                name: "ix_operating_slot_shop_id",
                table: "operating_slot",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_product_variant_p_variant_option_id",
                table: "order_detail_product_variant",
                column: "p_variant_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_order_id",
                table: "payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_operating_slot_product_id",
                table: "product_operating_slot",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_variant_product_id",
                table: "product_variant",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_variant_option_product_variant_id",
                table: "product_variant_option",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_location_id",
                table: "shop",
                column: "location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_wallet_id",
                table: "shop",
                column: "wallet_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_category_shop_id",
                table: "shop_category",
                column: "shop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourite_Shop",
                table: "favourtite",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_DeliveryPackage",
                table: "order",
                column: "delivery_package_id",
                principalTable: "delivery_package",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Shop",
                table: "order",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ParentOrderDetail",
                table: "order_detail",
                column: "parent_order_detail_id",
                principalTable: "order_detail",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category",
                table: "product",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ParentProduct",
                table: "product",
                column: "parent_id",
                principalTable: "product",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Shop",
                table: "product",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ShopCategory",
                table: "product",
                column: "shop_category_id",
                principalTable: "shop_category",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotion_Shop",
                table: "promotion",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Shop",
                table: "report",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopDormitory_Shop",
                table: "shop_dormitory",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffDelivery_Shop",
                table: "staff_delivery",
                column: "shop_id",
                principalTable: "shop",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Payment",
                table: "wallet_transaction",
                column: "payment_id",
                principalTable: "payment",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_WithdrawalRequest",
                table: "wallet_transaction",
                column: "withdrawal_request_id",
                principalTable: "withdrawal_request",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favourite_Shop",
                table: "favourtite");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_DeliveryPackage",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Shop",
                table: "order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ParentOrderDetail",
                table: "order_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_ParentProduct",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Shop",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_ShopCategory",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotion_Shop",
                table: "promotion");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_Shop",
                table: "report");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopDormitory_Shop",
                table: "shop_dormitory");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffDelivery_Shop",
                table: "staff_delivery");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Payment",
                table: "wallet_transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_WithdrawalRequest",
                table: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "delivery_package");

            migrationBuilder.DropTable(
                name: "order_detail_product_variant");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "payment_history");

            migrationBuilder.DropTable(
                name: "product_operating_slot");

            migrationBuilder.DropTable(
                name: "shop_category");

            migrationBuilder.DropTable(
                name: "product_variant_option");

            migrationBuilder.DropTable(
                name: "operating_slot");

            migrationBuilder.DropTable(
                name: "product_variant");

            migrationBuilder.DropTable(
                name: "shop");

            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_payment_id",
                table: "wallet_transaction");

            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_withdrawal_request_id",
                table: "wallet_transaction");

            migrationBuilder.DropIndex(
                name: "ix_product_category_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "ix_product_parent_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "ix_product_shop_category_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "ix_order_detail_parent_order_detail_id",
                table: "order_detail");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "withdrawal_request_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "max_flags_before_ban",
                table: "system_config");

            migrationBuilder.DropColumn(
                name: "time_frame_duration",
                table: "system_config");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "is_topping",
                table: "product");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "shop_category_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "parent_order_detail_id",
                table: "order_detail");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "staff_delivery",
                newName: "shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_staff_delivery_shop_id",
                table: "staff_delivery",
                newName: "ix_staff_delivery_shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "shop_dormitory",
                newName: "shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "report",
                newName: "shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_report_shop_id",
                table: "report",
                newName: "ix_report_shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "promotion",
                newName: "shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_promotion_shop_id",
                table: "promotion",
                newName: "ix_promotion_shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "product",
                newName: "shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_product_shop_id",
                table: "product",
                newName: "ix_product_shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "order",
                newName: "shop_owner_id");

            migrationBuilder.RenameColumn(
                name: "delivery_package_id",
                table: "order",
                newName: "delivery_order_combination_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_shop_id",
                table: "order",
                newName: "ix_order_shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_delivery_package_id",
                table: "order",
                newName: "ix_order_delivery_order_combination_id");

            migrationBuilder.RenameColumn(
                name: "shop_id",
                table: "favourtite",
                newName: "shop_owner_id");

            migrationBuilder.RenameIndex(
                name: "ix_favourtite_shop_id",
                table: "favourtite",
                newName: "ix_favourtite_shop_owner_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "category",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_transaction",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    payment_methods = table.Column<int>(type: "int", nullable: false),
                    payment_third_party_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_third_party_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderTransaction_Order",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_transaction_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    amount = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    order_transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_methods = table.Column<int>(type: "int", nullable: false),
                    payment_third_party_content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_third_party_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_transaction_history", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_category", x => new { x.category_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_ProductCategory_Category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategory_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_owner",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    additional_ship_fee = table.Column<double>(type: "double", nullable: false),
                    average_order_handle_in_frame = table.Column<int>(type: "int", nullable: false),
                    average_total_order_handle_in_day = table.Column<int>(type: "int", nullable: false),
                    bank_account_number = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_short_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    banner_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_accepting_order_next_day = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    logo_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    max_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    min_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    min_value_order_free_ship = table.Column<double>(type: "double", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_order = table.Column<int>(type: "int", nullable: false),
                    total_product = table.Column<int>(type: "int", nullable: false),
                    total_rating = table.Column<int>(type: "int", nullable: false),
                    total_review = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_owner", x => x.id);
                    table.ForeignKey(
                        name: "FK_ShopOwner_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopOwner_Location",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopOwner_Wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "topping_question",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topping_question", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductQuestion_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_order_combination",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: true),
                    staff_delivery_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_order_combination", x => x.id);
                    table.ForeignKey(
                        name: "FK_DeliveryOrderCombination_StaffDelivery",
                        column: x => x.staff_delivery_id,
                        principalTable: "staff_delivery",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_delivery_order_combination_shop_owner_shop_owner_id",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operating_day",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    day_of_week = table.Column<int>(type: "int", nullable: false),
                    is_close = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operating_day", x => x.id);
                    table.ForeignKey(
                        name: "FK_OperatingDay_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "topping_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    topping_question_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_pricing = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    price = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topping_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_ToppingOption_ToppingQuestion",
                        column: x => x.topping_question_id,
                        principalTable: "topping_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operating_frame",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    operating_day_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operating_frame", x => x.id);
                    table.ForeignKey(
                        name: "FK_OperatingFrame_OperatingDay",
                        column: x => x.operating_day_id,
                        principalTable: "operating_day",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_operating_hours",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    operating_day_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_operating_hours", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductOperatingHour_OperatingDay",
                        column: x => x.operating_day_id,
                        principalTable: "operating_day",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOperatingHour_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_detail_option",
                columns: table => new
                {
                    order_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    topping_option_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    price = table.Column<double>(type: "double", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_detail_option", x => new { x.order_detail_id, x.topping_option_id });
                    table.ForeignKey(
                        name: "FK_OrderDetailOption_OrderDetail",
                        column: x => x.order_detail_id,
                        principalTable: "order_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetailOption_ToppingOption",
                        column: x => x.topping_option_id,
                        principalTable: "topping_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_order_combination_shop_owner_id",
                table: "delivery_order_combination",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_order_combination_staff_delivery_id",
                table: "delivery_order_combination",
                column: "staff_delivery_id");

            migrationBuilder.CreateIndex(
                name: "ix_operating_day_shop_owner_id",
                table: "operating_day",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_operating_frame_operating_day_id",
                table: "operating_frame",
                column: "operating_day_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_option_topping_option_id",
                table: "order_detail_option",
                column: "topping_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_transaction_order_id",
                table: "order_transaction",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_category_product_id",
                table: "product_category",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_operating_hours_operating_day_id",
                table: "product_operating_hours",
                column: "operating_day_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_operating_hours_product_id",
                table: "product_operating_hours",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_owner_location_id",
                table: "shop_owner",
                column: "location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_owner_wallet_id",
                table: "shop_owner",
                column: "wallet_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_topping_option_topping_question_id",
                table: "topping_option",
                column: "topping_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_topping_question_product_id",
                table: "topping_question",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Favourite_ShopOwner",
                table: "favourtite",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_DeliveryOrderCombination",
                table: "order",
                column: "delivery_order_combination_id",
                principalTable: "delivery_order_combination",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ShopOwner",
                table: "order",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ShopOwner",
                table: "product",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotion_ShopOwner",
                table: "promotion",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_ShopOwner",
                table: "report",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopDormitory_ShopOwner",
                table: "shop_dormitory",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffDelivery_ShopOwner",
                table: "staff_delivery",
                column: "shop_owner_id",
                principalTable: "shop_owner",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
