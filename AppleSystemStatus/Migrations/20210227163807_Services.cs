using Microsoft.EntityFrameworkCore.Migrations;

namespace AppleSystemStatus.Migrations
{
    public partial class Services : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Services_Name_CountryId",
                table: "Services",
                columns: new[] { "Name", "CountryId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Name_CountryId",
                table: "Services");
        }
    }
}
