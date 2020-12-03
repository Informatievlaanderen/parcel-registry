using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveIsComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.DropColumn(
                name: "Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                columns: new[] { "Complete", "Removed" });
        }
    }
}
