using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buyly.Infrastructure.Data.Migrations
{
    public partial class AddOrderIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "PendingPayment",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldDefaultValue: "PendingPayment")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_OrderDate",
                table: "Orders",
                columns: new[] { "Status", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_OrderDate",
                table: "Orders",
                columns: new[] { "UserId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_Status",
                table: "Orders",
                columns: new[] { "UserId", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_Status",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "longtext",
                nullable: false,
                defaultValue: "PendingPayment",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldDefaultValue: "PendingPayment")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
