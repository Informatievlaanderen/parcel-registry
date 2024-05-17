using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class AddCountOnParcelDetailAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelDetailsWithCountV2",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Gml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GmlType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetailsWithCountV2", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                    table.UniqueConstraint("AK_ParcelDetailsWithCountV2_CaPaKey", x => x.CaPaKey);
                });

            migrationBuilder.CreateTable(
                name: "ParcelAddressesWithCountV2",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressesWithCountV2", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ParcelAddressesWithCountV2_ParcelDetailsWithCountV2_ParcelId",
                        column: x => x.ParcelId,
                        principalSchema: "ParcelRegistryLegacy",
                        principalTable: "ParcelDetailsWithCountV2",
                        principalColumn: "ParcelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressesWithCountV2_AddressPersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesWithCountV2",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsWithCountV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsWithCountV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2",
                column: "Removed")
                .Annotation("SqlServer:Include", new[] { "CaPaKey", "Status", "VersionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailsWithCountV2_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressesWithCountV2",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ParcelDetailsWithCountV2",
                schema: "ParcelRegistryLegacy");
        }
    }
}
