using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buyly.Infrastructure.Data.Migrations
{
    public partial class AddReviewRatingCheckConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews",
                sql: "`Rating` BETWEEN 1 AND 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews");
        }
    }
}
