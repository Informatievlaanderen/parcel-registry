using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddEventXmlToSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventDataAsXml",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDataAsXml",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");
        }
    }
}
