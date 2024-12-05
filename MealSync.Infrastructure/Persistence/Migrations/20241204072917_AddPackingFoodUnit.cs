using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackingFoodUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "max_carry_weight",
                table: "shop",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "food_packing_unit_id",
                table: "food",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "food_packing_unit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    shop_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    weight = table.Column<double>(type: "double", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_food_packing_unit", x => x.id);
                    table.ForeignKey(
                        name: "FK_FoodPackingUnit_Shop",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_food_food_packing_unit_id",
                table: "food",
                column: "food_packing_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_food_packing_unit_shop_id",
                table: "food_packing_unit",
                column: "shop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Food_FoodPackingUnit",
                table: "food",
                column: "food_packing_unit_id",
                principalTable: "food_packing_unit",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Food_FoodPackingUnit",
                table: "food");

            migrationBuilder.DropTable(
                name: "food_packing_unit");

            migrationBuilder.DropIndex(
                name: "ix_food_food_packing_unit_id",
                table: "food");

            migrationBuilder.DropColumn(
                name: "max_carry_weight",
                table: "shop");

            migrationBuilder.DropColumn(
                name: "food_packing_unit_id",
                table: "food");
        }
    }
}
