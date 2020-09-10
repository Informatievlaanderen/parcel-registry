using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddSyndicationItemCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // set value to UtcNow for all existing records
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SyndicationItemCreatedAt",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                nullable: false,
                defaultValue: DateTimeOffset.Now);

            // remove the default value
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SyndicationItemCreatedAt",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                nullable: false,
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyndicationItemCreatedAt",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");
        }
    }
}
