using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Core.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondDigikalaPanelTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DigikalaId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Packages2",
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
                    table.PrimaryKey("PK_Packages2", x => x.Serial);
                });

            migrationBuilder.CreateTable(
                name: "InvoicesProduct2",
                columns: table => new
                {
                    DigikalaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DKPC = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    desc = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Serial = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    PayMethod = table.Column<string>(type: "text", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesProduct2", x => x.DigikalaId);
                    table.ForeignKey(
                        name: "FK_InvoicesProduct2_Invoices2_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices2_DigikalaId",
                table: "Invoices2",
                column: "DigikalaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX2_CreatedAt_BRIN",
                table: "Invoices2",
                column: "CreatedAt")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesProduct2_InvoiceId",
                table: "InvoicesProduct2",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX2_btree_dkpc_paymethod",
                table: "InvoicesProduct2",
                columns: new[] { "DKPC", "PayMethod" })
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "IX2_CreatedAt_BRIN_product",
                table: "InvoicesProduct2",
                column: "CreatedAt")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "IX2_CreatedAt_BRIN_Packages",
                table: "Packages2",
                column: "RecievedAt")
                .Annotation("Npgsql:IndexMethod", "brin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoicesProduct2");

            migrationBuilder.DropTable(
                name: "Packages2");

            migrationBuilder.DropTable(
                name: "Invoices2");
        }
    }
}
