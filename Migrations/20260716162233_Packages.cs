using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.API.Migrations
{
    /// <inheritdoc />
    public partial class Packages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Serial = table.Column<string>(type: "text", nullable: false),
                    PackageId = table.Column<long>(type: "bigint", nullable: false),
                    dkpc = table.Column<long>(type: "bigint", nullable: false),
                    RecievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Warehouse = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Serial);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreatedAt_BRIN_Packages",
                table: "Packages",
                column: "RecievedAt")
                .Annotation("Npgsql:IndexMethod", "brin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
