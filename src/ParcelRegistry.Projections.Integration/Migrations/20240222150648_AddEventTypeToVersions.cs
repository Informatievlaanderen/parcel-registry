using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    public partial class AddEventTypeToVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "integration_parcel",
                table: "parcel_version",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_parcel_version_type",
                schema: "integration_parcel",
                table: "parcel_version",
                column: "type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_parcel_version_type",
                schema: "integration_parcel",
                table: "parcel_version");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "integration_parcel",
                table: "parcel_version");
        }
    }
}
