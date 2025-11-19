using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buyly.Infrastructure.Data.Migrations
{
    public partial class AddProductIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_CreatedAt",
                table: "Products",
                columns: new[] { "CategoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Price",
                table: "Products",
                columns: new[] { "CategoryId", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");
        }
    }
}
