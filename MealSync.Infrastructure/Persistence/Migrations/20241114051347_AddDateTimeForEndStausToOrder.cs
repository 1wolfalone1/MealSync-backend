using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDateTimeForEndStausToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancel_at",
                table: "order",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "lastest_delivery_fail_at",
                table: "order",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "reject_at",
                table: "order",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "resolve_at",
                table: "order",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancel_at",
                table: "order");

            migrationBuilder.DropColumn(
                name: "lastest_delivery_fail_at",
                table: "order");

            migrationBuilder.DropColumn(
                name: "reject_at",
                table: "order");

            migrationBuilder.DropColumn(
                name: "resolve_at",
                table: "order");
        }
    }
}
