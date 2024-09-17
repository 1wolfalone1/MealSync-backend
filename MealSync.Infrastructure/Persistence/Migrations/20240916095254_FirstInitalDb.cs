using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FirstInitalDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wallet_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    available_amount_before = table.Column<double>(type: "double", nullable: false),
                    available_amount_after = table.Column<double>(type: "double", nullable: false),
                    in_coming_amount_before = table.Column<double>(type: "double", nullable: false),
                    in_coming_amount_after = table.Column<double>(type: "double", nullable: false),
                    next_transfer_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_history", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "commission_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    commission_rate = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commission_config", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "location",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<double>(type: "double", nullable: false),
                    longitude = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_location", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operating_day",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    day_of_week = table.Column<int>(type: "int", nullable: false),
                    able_total_order = table.Column<int>(type: "int", nullable: false),
                    is_close = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operating_day", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_transaction_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    order_transaction_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_order_transaction_history", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_config", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system_resource",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    resource_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resource_content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_resource", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wallet",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    available_amount = table.Column<double>(type: "double", nullable: false),
                    in_coming_amount = table.Column<double>(type: "double", nullable: false),
                    next_transfer_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dormitory",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dormitory", x => x.id);
                    table.ForeignKey(
                        name: "FK_Dormitory_Location",
                        column: x => x.location_id,
                        principalTable: "location",
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
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    able_total_order_handle = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                name: "account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar_url = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    genders = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    f_user_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    num_of_flag = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account", x => x.id);
                    table.ForeignKey(
                        name: "FK_Account_Role",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wallet_transaction",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_WalletTransaction_Wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "withdrawal_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<double>(type: "double", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    bank_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_short_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_account_number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdrawal_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequest_Wallet",
                        column: x => x.wallet_id,
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "building",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    domitory_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_building", x => x.id);
                    table.ForeignKey(
                        name: "FK_Building_Dormitory",
                        column: x => x.domitory_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Building_Location",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "account_flag",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    action_type = table.Column<int>(type: "int", nullable: false),
                    target_type = table.Column<int>(type: "int", nullable: false),
                    target_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_flag", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_flag_account_account_id",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    dormitory_id = table.Column<long>(type: "bigint", nullable: false),
                    wallet_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer", x => x.id);
                    table.ForeignKey(
                        name: "FK_Customer_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Customer_Dormitory",
                        column: x => x.dormitory_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "moderator",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moderator", x => x.id);
                    table.ForeignKey(
                        name: "FK_Moderator_Account",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "moderator_permission",
                columns: table => new
                {
                    permission_id = table.Column<long>(type: "bigint", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    endpoint = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    method = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moderator_permission", x => new { x.permission_id, x.account_id });
                    table.ForeignKey(
                        name: "FK_AccountPermission_Account",
                        column: x => x.account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountPermission_Permission",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_type = table.Column<int>(type: "int", nullable: false),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notification_Account",
                        column: x => x.account_id,
                        principalTable: "account",
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
                    min_value_order_free_ship = table.Column<double>(type: "double", nullable: false),
                    additional_ship_fee = table.Column<double>(type: "double", nullable: false),
                    max_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    min_order_hours_in_advance = table.Column<int>(type: "int", nullable: false),
                    average_order_handle_in_frame = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                name: "verification_code",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expired_tịme = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "customer_building",
                columns: table => new
                {
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_building", x => new { x.building_id, x.customer_id });
                    table.ForeignKey(
                        name: "FK_CustomerBuilding_Building",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerBuilding_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "moderator_activity_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    moderator_id = table.Column<long>(type: "bigint", nullable: false),
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
                name: "moderator_dormitory",
                columns: table => new
                {
                    moderator_id = table.Column<long>(type: "bigint", nullable: false),
                    dormitory_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moderator_dormitory", x => new { x.moderator_id, x.dormitory_id });
                    table.ForeignKey(
                        name: "FK_ModeratorDormitory_Dormitory",
                        column: x => x.dormitory_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModeratorDormitory_Moderator",
                        column: x => x.moderator_id,
                        principalTable: "moderator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "favourtite",
                columns: table => new
                {
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favourtite", x => new { x.customer_id, x.shop_owner_id });
                    table.ForeignKey(
                        name: "FK_Favourite_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favourite_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("pk_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_Product_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "promotion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    decription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    banner_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<int>(type: "int", nullable: false),
                    amount_rate = table.Column<double>(type: "double", nullable: true),
                    amount_value = table.Column<double>(type: "double", nullable: true),
                    min_ordervalue = table.Column<double>(type: "double", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    usage_limit = table.Column<int>(type: "int", nullable: false),
                    number_of_used = table.Column<int>(type: "int", nullable: false),
                    apply_type = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion", x => x.id);
                    table.ForeignKey(
                        name: "FK_Promotion_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Promotion_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_dormitory",
                columns: table => new
                {
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    dormitory_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_dormitory", x => new { x.shop_owner_id, x.dormitory_id });
                    table.ForeignKey(
                        name: "FK_ShopDormitory_Dormitory",
                        column: x => x.dormitory_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopDormitory_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "staff_delivery",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                        name: "FK_StaffDelivery_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
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
                name: "product_operating_hours",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    operating_day_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                name: "topping_question",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                    staff_delivery_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                    is_pricing = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                name: "order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    promotion_id = table.Column<long>(type: "bigint", nullable: false),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    delivery_order_combination_id = table.Column<long>(type: "bigint", nullable: true),
                    shop_location_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_location_id = table.Column<long>(type: "bigint", nullable: false),
                    building_id = table.Column<long>(type: "bigint", nullable: false),
                    building_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shipping_fee = table.Column<double>(type: "double", nullable: false),
                    total_price = table.Column<double>(type: "double", nullable: false),
                    total_promotion = table.Column<double>(type: "double", nullable: false),
                    charge_fee = table.Column<double>(type: "double", nullable: false),
                    full_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<double>(type: "double", nullable: false),
                    longitude = table.Column<double>(type: "double", nullable: false),
                    order_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    intended_receive_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    receive_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    start_time = table.Column<int>(type: "int", nullable: false),
                    end_time = table.Column<int>(type: "int", nullable: false),
                    delivery_success_image_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_refund = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_report = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_Order_Building",
                        column: x => x.building_id,
                        principalTable: "building",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_CustomerLocation",
                        column: x => x.customer_location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_DeliveryOrderCombination",
                        column: x => x.delivery_order_combination_id,
                        principalTable: "delivery_order_combination",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Order_Promotion",
                        column: x => x.promotion_id,
                        principalTable: "promotion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_ShopLocation",
                        column: x => x.shop_location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_detail",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Product",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_transaction",
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
                name: "report",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_owner_id = table.Column<long>(type: "bigint", nullable: true),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    staff_delivery_id = table.Column<long>(type: "bigint", nullable: true),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_Report_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Report_Order",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Report_ShopOwner",
                        column: x => x.shop_owner_id,
                        principalTable: "shop_owner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Report_StaffDelivery",
                        column: x => x.staff_delivery_id,
                        principalTable: "staff_delivery",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "review",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_Review_Customer",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Review_Order",
                        column: x => x.order_id,
                        principalTable: "order",
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
                    price = table.Column<double>(type: "double", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
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
                name: "ix_account_email",
                table: "account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_account_phone_number",
                table: "account",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_account_role_id",
                table: "account",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_account_flag_account_id",
                table: "account_flag",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_building_domitory_id",
                table: "building",
                column: "domitory_id");

            migrationBuilder.CreateIndex(
                name: "ix_building_location_id",
                table: "building",
                column: "location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_dormitory_id",
                table: "customer",
                column: "dormitory_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_building_customer_id",
                table: "customer_building",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_delivery_order_combination_staff_delivery_id",
                table: "delivery_order_combination",
                column: "staff_delivery_id");

            migrationBuilder.CreateIndex(
                name: "ix_dormitory_location_id",
                table: "dormitory",
                column: "location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favourtite_shop_owner_id",
                table: "favourtite",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_moderator_activity_log_moderator_id",
                table: "moderator_activity_log",
                column: "moderator_id");

            migrationBuilder.CreateIndex(
                name: "ix_moderator_dormitory_dormitory_id",
                table: "moderator_dormitory",
                column: "dormitory_id");

            migrationBuilder.CreateIndex(
                name: "ix_moderator_permission_account_id",
                table: "moderator_permission",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_account_id",
                table: "notification",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_operating_frame_operating_day_id",
                table: "operating_frame",
                column: "operating_day_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_building_id",
                table: "order",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_customer_id",
                table: "order",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_customer_location_id",
                table: "order",
                column: "customer_location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_delivery_order_combination_id",
                table: "order",
                column: "delivery_order_combination_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_promotion_id",
                table: "order",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_shop_location_id",
                table: "order",
                column: "shop_location_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_shop_owner_id",
                table: "order",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_order_id",
                table: "order_detail",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_product_id",
                table: "order_detail",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_detail_option_topping_option_id",
                table: "order_detail_option",
                column: "topping_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_transaction_order_id",
                table: "order_transaction",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_shop_owner_id",
                table: "product",
                column: "shop_owner_id");

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
                name: "ix_promotion_customer_id",
                table: "promotion",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_shop_owner_id",
                table: "promotion",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_customer_id",
                table: "report",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_order_id",
                table: "report",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_shop_owner_id",
                table: "report",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_staff_delivery_id",
                table: "report",
                column: "staff_delivery_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_customer_id",
                table: "review",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_order_id",
                table: "review",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_dormitory_dormitory_id",
                table: "shop_dormitory",
                column: "dormitory_id");

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
                name: "ix_staff_delivery_shop_owner_id",
                table: "staff_delivery",
                column: "shop_owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_topping_option_topping_question_id",
                table: "topping_option",
                column: "topping_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_topping_question_product_id",
                table: "topping_question",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_verification_code_account_id",
                table: "verification_code",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transaction_wallet_id",
                table: "wallet_transaction",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_request_wallet_id",
                table: "withdrawal_request",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wallet_history");

            migrationBuilder.DropTable(
                name: "account_flag");

            migrationBuilder.DropTable(
                name: "commission_config");

            migrationBuilder.DropTable(
                name: "customer_building");

            migrationBuilder.DropTable(
                name: "favourtite");

            migrationBuilder.DropTable(
                name: "moderator_activity_log");

            migrationBuilder.DropTable(
                name: "moderator_dormitory");

            migrationBuilder.DropTable(
                name: "moderator_permission");

            migrationBuilder.DropTable(
                name: "notification");

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
                name: "report");

            migrationBuilder.DropTable(
                name: "review");

            migrationBuilder.DropTable(
                name: "shop_dormitory");

            migrationBuilder.DropTable(
                name: "system_config");

            migrationBuilder.DropTable(
                name: "system_resource");

            migrationBuilder.DropTable(
                name: "verification_code");

            migrationBuilder.DropTable(
                name: "wallet_transaction");

            migrationBuilder.DropTable(
                name: "withdrawal_request");

            migrationBuilder.DropTable(
                name: "moderator");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "order_detail");

            migrationBuilder.DropTable(
                name: "topping_option");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "operating_day");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "topping_question");

            migrationBuilder.DropTable(
                name: "building");

            migrationBuilder.DropTable(
                name: "delivery_order_combination");

            migrationBuilder.DropTable(
                name: "promotion");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "staff_delivery");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "shop_owner");

            migrationBuilder.DropTable(
                name: "dormitory");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "wallet");

            migrationBuilder.DropTable(
                name: "location");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
