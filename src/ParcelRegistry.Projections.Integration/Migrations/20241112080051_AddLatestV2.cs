using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class AddLatestV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parcel_latest_item_addresses_v2",
                schema: "integration_parcel",
                columns: table => new
                {
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    capakey = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_latest_item_addresses_v2", x => new { x.parcel_id, x.address_persistent_local_id });
                });

            migrationBuilder.CreateTable(
                name: "parcel_latest_items_v2",
                schema: "integration_parcel",
                columns: table => new
                {
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capakey = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    puri = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_latest_items_v2", x => x.parcel_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_v2_address_persistent_local_id",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses_v2",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_v2_capakey",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses_v2",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_v2_parcel_id",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses_v2",
                column: "parcel_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_capakey",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_geometry",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_is_removed",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_is_removed_status",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                columns: new[] { "is_removed", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_oslo_status",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_v2_status",
                schema: "integration_parcel",
                table: "parcel_latest_items_v2",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parcel_latest_item_addresses_v2",
                schema: "integration_parcel");

            migrationBuilder.DropTable(
                name: "parcel_latest_items_v2",
                schema: "integration_parcel");
        }
    }
}
