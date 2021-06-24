using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddNullableUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_PersistentLocalId_1",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_PersistentLocalId_1",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");
        }
    }
}
