using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RemakeRemovedIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetailsV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "Removed")
                .Annotation("SqlServer:Include", new[] { "CaPaKey", "Status", "VersionTimestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetailsV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "Removed");
        }
    }
}
