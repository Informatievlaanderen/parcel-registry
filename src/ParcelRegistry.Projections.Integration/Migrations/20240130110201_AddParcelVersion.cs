using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    public partial class AddParcelVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parcel_version",
                schema: "integration_parcel",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capakey = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    oslo_status = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: true),
                    puri = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_as_string = table.Column<string>(type: "text", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_version", x => new { x.position, x.parcel_id });
                });

            migrationBuilder.CreateTable(
                name: "parcel_version_addresses",
                schema: "integration_parcel",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    capakey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_version_addresses", x => new { x.position, x.parcel_id, x.address_persistent_local_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_capakey",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_geometry",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_is_removed",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_oslo_status",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_status",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_version_timestamp",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_addresses_address_persistent_local_id",
                schema: "integration_parcel",
                table: "parcel_version_addresses",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_addresses_capakey",
                schema: "integration_parcel",
                table: "parcel_version_addresses",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_addresses_parcel_id",
                schema: "integration_parcel",
                table: "parcel_version_addresses",
                column: "parcel_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_addresses_position",
                schema: "integration_parcel",
                table: "parcel_version_addresses",
                column: "position");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parcel_version",
                schema: "integration_parcel");

            migrationBuilder.DropTable(
                name: "parcel_version_addresses",
                schema: "integration_parcel");
        }
    }
}
