using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    /// <inheritdoc />
    public partial class FixParcelVersionTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_parcel_version",
                schema: "integration_parcel",
                table: "parcel_version");

            migrationBuilder.RenameTable(
                name: "parcel_version",
                schema: "integration_parcel",
                newName: "parcel_versions",
                newSchema: "integration_parcel");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_version_timestamp",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_version_timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_type",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_type");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_status",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_status");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_parcel_id",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_parcel_id");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_oslo_status",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_oslo_status");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_is_removed",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_is_removed");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_geometry",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_geometry");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_version_capakey",
                schema: "integration_parcel",
                table: "parcel_versions",
                newName: "IX_parcel_versions_capakey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_parcel_versions",
                schema: "integration_parcel",
                table: "parcel_versions",
                columns: new[] { "position", "parcel_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_parcel_versions",
                schema: "integration_parcel",
                table: "parcel_versions");

            migrationBuilder.RenameTable(
                name: "parcel_versions",
                schema: "integration_parcel",
                newName: "parcel_version",
                newSchema: "integration_parcel");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_version_timestamp",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_version_timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_type",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_type");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_status",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_status");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_parcel_id",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_parcel_id");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_oslo_status",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_oslo_status");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_is_removed",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_is_removed");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_geometry",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_geometry");

            migrationBuilder.RenameIndex(
                name: "IX_parcel_versions_capakey",
                schema: "integration_parcel",
                table: "parcel_version",
                newName: "IX_parcel_version_capakey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_parcel_version",
                schema: "integration_parcel",
                table: "parcel_version",
                columns: new[] { "position", "parcel_id" });
        }
    }
}
