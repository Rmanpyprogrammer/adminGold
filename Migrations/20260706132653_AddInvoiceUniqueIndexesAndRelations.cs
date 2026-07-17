using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceUniqueIndexesAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Orders_CreatedAt_BRIN_product",
                table: "InvoicesProduct",
                newName: "IX_CreatedAt_BRIN_product");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CreatedAt_BRIN",
                table: "Invoices",
                newName: "IX_CreatedAt_BRIN");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DigikalaId",
                table: "Invoices",
                column: "DigikalaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_DigikalaId",
                table: "Invoices");

            migrationBuilder.RenameIndex(
                name: "IX_CreatedAt_BRIN_product",
                table: "InvoicesProduct",
                newName: "IX_Orders_CreatedAt_BRIN_product");

            migrationBuilder.RenameIndex(
                name: "IX_CreatedAt_BRIN",
                table: "Invoices",
                newName: "IX_Orders_CreatedAt_BRIN");
        }
    }
}
