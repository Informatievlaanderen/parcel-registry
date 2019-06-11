using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RenamePlanToReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                nullable: true);
        }
    }
}
