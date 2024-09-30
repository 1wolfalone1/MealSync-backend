using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDbAfterConsiderOnReview1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_ParentOrderDetail",
                table: "order_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Product",
                table: "order_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Wallet",
                table: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "order_detail_product_variant");

            migrationBuilder.DropTable(
                name: "product_operating_slot");

            migrationBuilder.DropTable(
                name: "product_variant_option");

            migrationBuilder.DropTable(
                name: "product_variant");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropIndex(
                name: "ix_order_detail_parent_order_detail_id",
                table: "order_detail");

            migrationBuilder.DropColumn(
                name: "parent_order_detail_id",
                table: "order_detail");

            migrationBuilder.RenameColumn(
                name: "wallet_id",
                table: "wallet_transaction",
                newName: "wallet_from_id");

            migrationBuilder.RenameIndex(
                name: "ix_wallet_transaction_wallet_id",
                table: "wallet_transaction",
                newName: "ix_wallet_transaction_wallet_from_id");

            migrationBuilder.RenameColumn(
                name: "total_product",
                table: "shop",
                newName: "total_food");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "order_detail",
                newName: "food_id");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "order_detail",
                newName: "toltal_price");

            migrationBuilder.RenameIndex(
                name: "ix_order_detail_product_id",
                table: "order_detail",
                newName: "ix_order_detail_food_id");

            migrationBuilder.AddColumn<long>(
                name: "wallet_to_id",
                table: "wallet_transaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "basic_price",
                table: "order_detail",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "order_detail",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "option_group",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_require = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_OptionGroup_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "platform_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("pk_platform_category", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    option_group_id = table.Column<long>(type: "bigint", nullable: false),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_calculate_price = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    price = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_Option_OptionGroup",
                        column: x => x.option_group_id,
                        principalTable: "option_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    platform_category_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_category_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_order = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    is_sold_out = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_food", x => x.id);
                    table.ForeignKey(
                        name: "FK_Food_PlatformCategory",
                        column: x => x.platform_category_id,
                        principalTable: "platform_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Food_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Food_ShopCategory",
                        column: x => x.shop_category_id,
                        principalTable: "shop_category",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_detail_option",
                columns: table => new
                {
                    order_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    option_id = table.Column<long>(type: "bigint", nullable: false),
                    option_group_title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_detail_option", x => new { x.order_detail_id, x.option_id });
                    table.ForeignKey(
                        name: "FK_OrderDetailOption_Option",
                        column: x => x.option_id,
                        principalTable: "option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetailOption_OrderDetail",
                        column: x => x.order_detail_id,
                        principalTable: "order_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_operating_slot",
                columns: table => new
                {
                    operating_slot_id = table.Column<long>(type: "bigint", nullable: false),
                    food_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_food_operating_slot", x => new { x.operating_slot_id, x.food_id });
                    table.ForeignKey(
                        name: "FK_FoodOperatingSlot_Food",
                        column: x => x.food_id,
                        principalTable: "food",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodOperatingSlot_OperatingSlot",
                        column: x => x.operating_slot_id,
                        principalTable: "operating_slot",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_option_group",
                columns: table => new
                {
                    food_id = table.Column<long>(type: "bigint", nullable: false),
                    option_group_id = table.Column<long>(type: "bigint", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_food_option_group", x => new { x.food_id, x.option_group_id });
                    table.ForeignKey(
                        name: "FK_FoodOptionGroup_Food",
                        column: x => x.food_id,
                        principalTable: "food",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodOptionGroup_OptionGroup",
                        column: x => x.option_group_id,
                        principalTable: "option_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_to_id",
                table: "wallet_transaction",
                column: "wallet_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_platform_category_id",
                table: "food",
                column: "platform_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_shop_category_id",
                table: "food",
                column: "shop_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_shop_id",
                table: "food",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_operating_slot_food_id",
                table: "food_operating_slot",
                column: "food_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_option_group_option_group_id",
                table: "food_option_group",
                column: "option_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_option_group_id",
                table: "option",
                column: "option_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_group_shop_id",
                table: "option_group",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_option_option_id",
                table: "order_detail_option",
                column: "option_id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Food",
                table: "order_detail",
                column: "food_id",
                principalTable: "food",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction",
                column: "wallet_from_id",
                principalTable: "wallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_WalletTo",
                table: "wallet_transaction",
                column: "wallet_to_id",
                principalTable: "wallet",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Food",
                table: "order_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_WalletFrom",
                table: "wallet_transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_WalletTo",
                table: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "food_operating_slot");

            migrationBuilder.DropTable(
                name: "food_option_group");

            migrationBuilder.DropTable(
                name: "order_detail_option");

            migrationBuilder.DropTable(
                name: "food");

            migrationBuilder.DropTable(
                name: "option");

            migrationBuilder.DropTable(
                name: "platform_category");

            migrationBuilder.DropTable(
                name: "option_group");

            migrationBuilder.DropIndex(
                name: "ix_wallet_transaction_wallet_to_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "wallet_to_id",
                table: "wallet_transaction");

            migrationBuilder.DropColumn(
                name: "basic_price",
                table: "order_detail");

            migrationBuilder.DropColumn(
                name: "description",
                table: "order_detail");

            migrationBuilder.RenameColumn(
                name: "wallet_from_id",
                table: "wallet_transaction",
                newName: "wallet_id");

            migrationBuilder.RenameIndex(
                name: "ix_wallet_transaction_wallet_from_id",
                table: "wallet_transaction",
                newName: "ix_wallet_transaction_wallet_id");

            migrationBuilder.RenameColumn(
                name: "total_food",
                table: "shop",
                newName: "total_product");

            migrationBuilder.RenameColumn(
                name: "toltal_price",
                table: "order_detail",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "food_id",
                table: "order_detail",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "ix_order_detail_food_id",
                table: "order_detail",
                newName: "ix_order_detail_product_id");

            migrationBuilder.AddColumn<long>(
                name: "parent_order_detail_id",
                table: "order_detail",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    shop_category_id = table.Column<long>(type: "bigint", nullable: true),
                    shop_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_sold_out = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_topping = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_order = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_Product_Category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_ParentProduct",
                        column: x => x.parent_id,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Product_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_ShopCategory",
                        column: x => x.shop_category_id,
                        principalTable: "shop_category",
                        principalColumn: "id");
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

            migrationBuilder.CreateTable(
                name: "product_variant",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
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
                name: "product_variant_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
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
                name: "order_detail_product_variant",
                columns: table => new
                {
                    order_detail_id = table.Column<long>(type: "bigint", nullable: false),
                    p_variant_option_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    p_variant_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    p_variant_option_image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    p_variant_option_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<double>(type: "double", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_parent_order_detail_id",
                table: "order_detail",
                column: "parent_order_detail_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_product_variant_p_variant_option_id",
                table: "order_detail_product_variant",
                column: "p_variant_option_id");

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
                name: "ix_product_shop_id",
                table: "product",
                column: "shop_id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_ParentOrderDetail",
                table: "order_detail",
                column: "parent_order_detail_id",
                principalTable: "order_detail",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product",
                table: "order_detail",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Wallet",
                table: "wallet_transaction",
                column: "wallet_id",
                principalTable: "wallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
