using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldEvendence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "evidence_take_picture_datetime",
                table: "order");

            migrationBuilder.RenameColumn(
                name: "evidence_delivery_fail_image_urls",
                table: "order",
                newName: "evidence_delivery_fail_json");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "evidence_delivery_fail_json",
                table: "order",
                newName: "evidence_delivery_fail_image_urls");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "evidence_take_picture_datetime",
                table: "order",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
