using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.DropColumn(
                name: "YCoordinate",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.AddColumn<byte[]>(
                name: "ExtendedWkbGeometry",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gml",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedWkbGeometry",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.DropColumn(
                name: "Gml",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");

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
        }
    }
}
