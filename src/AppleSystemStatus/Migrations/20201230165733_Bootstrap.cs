using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppleSystemStatus.Migrations
{
    public partial class Bootstrap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CountryId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    ServiceId = table.Column<Guid>(nullable: false),
                    EpochStartDate = table.Column<long>(nullable: false),
                    UsersAffected = table.Column<string>(nullable: false),
                    EpochEndDate = table.Column<long>(nullable: true),
                    MessageId = table.Column<long>(nullable: false),
                    StatusType = table.Column<int>(nullable: false),
                    DatePosted = table.Column<string>(nullable: false),
                    StartDate = table.Column<string>(nullable: false),
                    EndDate = table.Column<string>(nullable: true),
                    AffectedServices = table.Column<string>(nullable: true),
                    EventStatus = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.ServiceId, x.EpochStartDate });
                    table.ForeignKey(
                        name: "FK_Events_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EpochEndDate",
                table: "Events",
                column: "EpochEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CountryId",
                table: "Services",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name",
                table: "Services",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
