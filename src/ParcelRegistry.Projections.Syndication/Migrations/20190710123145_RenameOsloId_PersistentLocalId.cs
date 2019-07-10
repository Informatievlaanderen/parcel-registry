using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Syndication.Migrations
{
    public partial class RenameOsloId_PersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressOsloIdSyndication",
                schema: "ParcelRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressPersistentLocalIdSyndication", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressPersistentLocalIdSyndication_IsComplete_IsRemoved",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "AddressOsloIdSyndication",
                schema: "ParcelRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    Version = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressOsloIdSyndication", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressOsloIdSyndication_IsComplete_IsRemoved",
                schema: "ParcelRegistrySyndication",
                table: "AddressOsloIdSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });
        }
    }
}
