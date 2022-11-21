using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Api.BackOffice.Abstractions.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryBackOffice");

            migrationBuilder.CreateTable(
                name: "ParcelAddressRelation",
                schema: "ParcelRegistryBackOffice",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddressRelation", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressRelation_AddressPersistentLocalId",
                schema: "ParcelRegistryBackOffice",
                table: "ParcelAddressRelation",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelAddressRelation_ParcelId",
                schema: "ParcelRegistryBackOffice",
                table: "ParcelAddressRelation",
                column: "ParcelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddressRelation",
                schema: "ParcelRegistryBackOffice");
        }
    }
}
