using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    /// <inheritdoc />
    public partial class DeleteV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddresses",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryLegacy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersistentLocalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetails", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ParcelAddresses",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddresses", x => new { x.ParcelId, x.AddressId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ParcelAddresses_ParcelDetails_ParcelId",
                        column: x => x.ParcelId,
                        principalSchema: "ParcelRegistryLegacy",
                        principalTable: "ParcelDetails",
                        principalColumn: "ParcelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddresses_AddressId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddresses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_PersistentLocalId_1",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "PersistentLocalId",
                unique: true,
                filter: "([PersistentLocalId] IS NOT NULL)")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Status");
        }
    }
}
