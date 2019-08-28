using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class ColumnIndexSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CI_ParcelSyndication_Position",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                column: "Position")
                .Annotation("SqlServer:ColumnStoreIndex", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CI_ParcelSyndication_Position",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");
        }
    }
}
