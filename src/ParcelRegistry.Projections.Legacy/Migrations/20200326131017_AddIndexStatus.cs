using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddIndexStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
