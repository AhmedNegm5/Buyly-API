using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buyly.Infrastructure.Data.Migrations
{
    public partial class AddPasswordResetCodeFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiresAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCodeHash",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeHash",
                table: "AspNetUsers");
        }
    }
}
