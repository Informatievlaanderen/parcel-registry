using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2Tables_P1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressPersistentLocalIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                newName: "AddressIds");

            migrationBuilder.AddColumn<decimal>(
                name: "XCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "YCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParcelDetailsV2",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetailsV2", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ParcelAddressesV2",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressesV2", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ParcelAddressesV2_ParcelDetailsV2_ParcelId",
                        column: x => x.ParcelId,
                        principalSchema: "ParcelRegistryLegacy",
                        principalTable: "ParcelDetailsV2",
                        principalColumn: "ParcelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "CaPaKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsV2_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressesV2",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ParcelDetailsV2",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropColumn(
                name: "XCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.DropColumn(
                name: "YCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.RenameColumn(
                name: "AddressIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                newName: "AddressPersistentLocalIds");
        }
    }
}
