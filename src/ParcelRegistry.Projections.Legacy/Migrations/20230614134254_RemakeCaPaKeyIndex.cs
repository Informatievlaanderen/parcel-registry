using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RemakeCaPaKeyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "CaPaKey");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

            migrationBuilder.DropIndex(
                name: "IX_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "CaPaKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
