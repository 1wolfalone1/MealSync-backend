using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPromotionOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "intended_receive_at",
                table: "order");

            migrationBuilder.AddColumn<double>(
                name: "max_apply_value",
                table: "promotion",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "intended_receive_date",
                table: "order",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_apply_value",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "intended_receive_date",
                table: "order");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "intended_receive_at",
                table: "order",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
