using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buyly.Infrastructure.Data.Migrations
{
    public partial class AddPasswordResetRateLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeLastSentAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordResetCodeRequestCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeRequestWindowStart",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetCodeLastSentAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeRequestCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeRequestWindowStart",
                table: "AspNetUsers");
        }
    }
}
