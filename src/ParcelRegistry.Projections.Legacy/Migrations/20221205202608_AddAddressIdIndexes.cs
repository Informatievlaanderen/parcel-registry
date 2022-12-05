using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddAddressIdIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressesV2_AddressPersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesV2",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddresses_AddressId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddresses",
                column: "AddressId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelAddressesV2_AddressPersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesV2");

            migrationBuilder.DropIndex(
                name: "IX_ParcelAddresses_AddressId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddresses");
        }
    }
}
