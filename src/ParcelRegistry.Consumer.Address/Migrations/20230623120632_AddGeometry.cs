using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    public partial class AddGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeometryMethod",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GeometrySpecification",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Point>(
                name: "Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                type: "sys.geometry",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeometryMethod",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "GeometrySpecification",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");
        }
    }
}
