using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    using ParcelDetail;

    /// <inheritdoc />
    public partial class DeleteOldRenameV2WithCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]");

            migrationBuilder.DropTable(
                name: "ParcelAddressesV2",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ParcelDetailsV2",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropForeignKey(
                name: "FK_ParcelAddressesWithCountV2_ParcelDetailsWithCountV2_ParcelId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesWithCountV2");

            migrationBuilder.DropUniqueConstraint(
                name:"AK_ParcelDetailsWithCountV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelAddressesWithCountV2",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesWithCountV2");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelDetailsWithCountV2",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2");

            migrationBuilder.RenameTable(
                name: "ParcelDetailsWithCountV2",
                schema: "ParcelRegistryLegacy",
                newName:"ParcelDetails",
                newSchema:"ParcelRegistryLegacy");

            migrationBuilder.RenameTable(
                name: "ParcelAddressesWithCountV2",
                schema: "ParcelRegistryLegacy",
                newName:"ParcelDetailAddresses",
                newSchema:"ParcelRegistryLegacy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelAddresses",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses",
                columns: new[] { "ParcelId", "AddressPersistentLocalId" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelDetails",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                columns: new[] { "ParcelId" })
            .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name:"AK_ParcelDetails_CaPaKey",
                schema:"ParcelRegistryLegacy",
                table:"ParcelDetails",
                column:"CaPaKey");

            migrationBuilder.AddForeignKey(
                name: "FK_ParcelDetailAddresses_ParcelDetails_ParcelId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses",
                column: "ParcelId",
                principalSchema: "ParcelRegistryLegacy",
                principalTable: "ParcelDetails",
                principalColumn: "ParcelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameIndex(
                name: "IX_ParcelAddressesWithCountV2_AddressPersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses",
                newName: "IX_ParcelDetailAddresses_AddressPersistentLocalId");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetailsWithCountV2_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetails_CaPaKey");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetailsWithCountV2_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetails_Removed");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetailsWithCountV2_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetails_Status");

            migrationBuilder.Sql($@"
CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]
WITH SCHEMABINDING
AS
SELECT COUNT_BIG(*) as Count
FROM [{Infrastructure.Schema.Legacy}].[{ParcelDetailConfiguration.TableName}]
WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailV2ListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}] (Count)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]");

            migrationBuilder.CreateTable(
                name: "ParcelDetailsV2",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Gml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GmlType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetailsV2", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                    table.UniqueConstraint("AK_ParcelDetailsV2_CaPaKey", x => x.CaPaKey);
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

            // Rename indexes back to original names
            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetails_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetailsWithCountV2_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetails_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetailsWithCountV2_Removed");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetails_CaPaKey",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                newName: "IX_ParcelDetailsWithCountV2_CaPaKey");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelDetailAddresses_AddressPersistentLocalId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses",
                newName: "IX_ParcelAddressesWithCountV2_AddressPersistentLocalId");

            // Drop foreign key, unique constraint, and primary keys
            migrationBuilder.DropForeignKey(
                name: "FK_ParcelDetailAddresses_ParcelDetails_ParcelId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses");

            migrationBuilder.DropUniqueConstraint(
                name:"AK_ParcelDetails_CaPaKey",
                schema:"ParcelRegistryLegacy",
                table:"ParcelDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelDetails",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelAddresses",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailAddresses");

            // Rename tables back to original names
            migrationBuilder.RenameTable(
                name: "ParcelDetailAddresses",
                schema: "ParcelRegistryLegacy",
                newName: "ParcelAddressesWithCountV2",
                newSchema: "ParcelRegistryLegacy");

            migrationBuilder.RenameTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryLegacy",
                newName: "ParcelDetailsWithCountV2",
                newSchema: "ParcelRegistryLegacy");

            // Add back the primary keys
            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelDetailsWithCountV2",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsWithCountV2",
                column: "ParcelId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelAddressesWithCountV2",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesWithCountV2",
                columns: new[] { "ParcelId", "AddressPersistentLocalId" })
                .Annotation("SqlServer:Clustered", true);

            // Add back the unique constraint
            migrationBuilder.AddUniqueConstraint(
                name:"AK_ParcelDetailsWithCountV2_CaPaKey",
                schema:"ParcelRegistryLegacy",
                table:"ParcelDetailsWithCountV2",
                column:"CaPaKey");

            // Add back the foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_ParcelAddressesWithCountV2_ParcelDetailsWithCountV2_ParcelId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelAddressesWithCountV2",
                column: "ParcelId",
                principalSchema: "ParcelRegistryLegacy",
                principalTable: "ParcelDetailsWithCountV2",
                principalColumn: "ParcelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql($@"
CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]
WITH SCHEMABINDING
AS
SELECT COUNT_BIG(*) as Count
FROM [{Infrastructure.Schema.Legacy}].[{ParcelDetailConfiguration.TableName}]
WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailV2ListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}] (Count)");
        }
    }
}
