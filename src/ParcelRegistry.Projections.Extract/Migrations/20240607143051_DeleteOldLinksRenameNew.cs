using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class DeleteOldLinksRenameNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelLinks",
                schema: "ParcelRegistryExtract");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelLinksWithCount",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinksWithCount");

            migrationBuilder.RenameTable(
                name:"ParcelLinksWithCount",
                schema: "ParcelRegistryExtract",
                newName: "ParcelLinks",
                newSchema: "ParcelRegistryExtract");

            migrationBuilder.AddPrimaryKey(
                name:"PK_ParcelLinks",
                schema:"ParcelRegistryExtract",
                table:"ParcelLinks",
                columns: new[] { "ParcelId", "AddressPersistentLocalId" })
            .Annotation("SqlServer:Clustered", false);

            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinksWithCount_CaPaKey",
                newName: "IX_ParcelLinks_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks")
            .Annotation("SqlServer:Clustered", true);

            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinksWithCount_AddressPersistentLocalId",
                newName: "IX_ParcelLinks_AddressPersistentLocalId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinksWithCount_ParcelId",
                newName: "IX_ParcelLinks_ParcelId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelV2",
                table: "ParcelV2",
                schema: "ParcelRegistryExtract");

            migrationBuilder.RenameTable(
                name: "ParcelV2",
                schema: "ParcelRegistryExtract",
                newName: "Parcels",
                newSchema: "ParcelRegistryExtract");

            migrationBuilder
                .AddPrimaryKey(
                    name: "PK_Parcels",
                    schema: "ParcelRegistryExtract",
                    table: "Parcels",
                    column: "ParcelId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.RenameIndex(
                name:"IX_ParcelV2_CaPaKey",
                newName: "IX_Parcels_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "Parcels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename index back to original name
            migrationBuilder.RenameIndex(
                name: "IX_Parcels_CaPaKey",
                newName: "IX_ParcelV2_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "Parcels");

            // Drop primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Parcels",
                schema: "ParcelRegistryExtract",
                table: "Parcels");

            // Rename table back to original name
            migrationBuilder.RenameTable(
                name: "Parcels",
                schema: "ParcelRegistryExtract",
                newName: "ParcelV2",
                newSchema: "ParcelRegistryExtract");

            // Add back the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelV2",
                schema: "ParcelRegistryExtract",
                table: "ParcelV2",
                column: "ParcelId")
                .Annotation("SqlServer:Clustered", false);

            // Rename indexes back to original names
            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinks_ParcelId",
                newName: "IX_ParcelLinksWithCount_ParcelId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinks_AddressPersistentLocalId",
                newName: "IX_ParcelLinksWithCount_AddressPersistentLocalId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks");

            migrationBuilder.RenameIndex(
                name: "IX_ParcelLinks_CaPaKey",
                newName: "IX_ParcelLinksWithCount_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks")
                .Annotation("SqlServer:Clustered", true);

            // Drop primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_ParcelLinks",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks");

            // Rename table back to original name
            migrationBuilder.RenameTable(
                name: "ParcelLinks",
                schema: "ParcelRegistryExtract",
                newName: "ParcelLinksWithCount",
                newSchema: "ParcelRegistryExtract");

            // Add back the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_ParcelLinksWithCount",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinksWithCount",
                columns: new[] { "ParcelId", "AddressPersistentLocalId" })
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateTable(
                name: "ParcelLinks",
                schema: "ParcelRegistryExtract",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelLinks", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelLinks_AddressPersistentLocalId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelLinks_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelLinks_ParcelId",
                schema: "ParcelRegistryExtract",
                table: "ParcelLinks",
                column: "ParcelId");
        }
    }
}
