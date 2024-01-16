using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_parcel");

            migrationBuilder.CreateTable(
                name: "parcel_latest_item_addresses",
                schema: "integration_parcel",
                columns: table => new
                {
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_persistent_local_id = table.Column<int>(type: "integer", nullable: false),
                    capakey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_latest_item_addresses", x => new { x.parcel_id, x.address_persistent_local_id });
                });

            migrationBuilder.CreateTable(
                name: "parcel_latest_items",
                schema: "integration_parcel",
                columns: table => new
                {
                    parcel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capakey = table.Column<string>(type: "varchar", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    oslo_status = table.Column<string>(type: "text", nullable: false),
                    geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    puri_id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    version_as_string = table.Column<string>(type: "text", nullable: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcel_latest_items", x => x.parcel_id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_parcel",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_address_persistent_local_id",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses",
                column: "address_persistent_local_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_capakey",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_item_addresses_parcel_id",
                schema: "integration_parcel",
                table: "parcel_latest_item_addresses",
                column: "parcel_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_capakey",
                schema: "integration_parcel",
                table: "parcel_latest_items",
                column: "capakey");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_geometry",
                schema: "integration_parcel",
                table: "parcel_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_is_removed",
                schema: "integration_parcel",
                table: "parcel_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_oslo_status",
                schema: "integration_parcel",
                table: "parcel_latest_items",
                column: "oslo_status");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_latest_items_status",
                schema: "integration_parcel",
                table: "parcel_latest_items",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parcel_latest_item_addresses",
                schema: "integration_parcel");

            migrationBuilder.DropTable(
                name: "parcel_latest_items",
                schema: "integration_parcel");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_parcel");
        }
    }
}
