using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddIndexOnList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                columns: new[] { "Complete", "Removed" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");
        }
    }
}
