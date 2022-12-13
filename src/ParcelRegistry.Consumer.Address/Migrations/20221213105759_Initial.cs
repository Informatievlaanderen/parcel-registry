using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryConsumerAddress");

            migrationBuilder.CreateTable(
                name: "Addresses",
                schema: "ParcelRegistryConsumerAddress",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                schema: "ParcelRegistryConsumerAddress",
                columns: table => new
                {
                    IdempotenceKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateProcessed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.IdempotenceKey)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AddressId",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_IsRemoved",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                column: "IsRemoved");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses",
                schema: "ParcelRegistryConsumerAddress");

            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "ParcelRegistryConsumerAddress");
        }
    }
}
