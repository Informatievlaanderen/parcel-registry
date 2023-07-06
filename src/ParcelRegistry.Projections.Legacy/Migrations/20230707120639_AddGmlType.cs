using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddGmlType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GmlType",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GmlType",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetailsV2");
        }
    }
}
