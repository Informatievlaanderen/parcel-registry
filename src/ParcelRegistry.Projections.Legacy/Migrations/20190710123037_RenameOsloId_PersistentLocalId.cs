using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressOsloIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                newName: "AddressPersistentLocalIds");

            migrationBuilder.RenameColumn(
                name: "OsloId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "PersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetails_OsloId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetails_PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressPersistentLocalIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                newName: "AddressOsloIds");

            migrationBuilder.RenameColumn(
                name: "PersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "OsloId");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetails_PersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetails_OsloId");
        }
    }
}
