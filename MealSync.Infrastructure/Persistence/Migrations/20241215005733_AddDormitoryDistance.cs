using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDormitoryDistance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dormitory_distance",
                columns: table => new
                {
                    dormitory_from_id = table.Column<long>(type: "bigint", nullable: false),
                    dormitory_to_id = table.Column<long>(type: "bigint", nullable: false),
                    distance = table.Column<double>(type: "double", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dormitory_distance", x => new { x.dormitory_from_id, x.dormitory_to_id });
                    table.ForeignKey(
                        name: "FK_DormitoryDistance_DormitoryFrom",
                        column: x => x.dormitory_from_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DormitoryDistance_DormitoryTo",
                        column: x => x.dormitory_to_id,
                        principalTable: "dormitory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_dormitory_distance_dormitory_to_id",
                table: "dormitory_distance",
                column: "dormitory_to_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dormitory_distance");
        }
    }
}
