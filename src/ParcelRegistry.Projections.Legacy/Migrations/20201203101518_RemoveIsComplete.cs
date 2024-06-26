using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class RemoveIsComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_vw_ParcelDetailListCount] ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");

            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.DropIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.DropColumn(
                name: "Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails");

            migrationBuilder.Sql($@"
                CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]
                WITH SCHEMABINDING
                AS
                SELECT COUNT_BIG(*) as Count
                FROM [{Infrastructure.Schema.Legacy}].[ParcelDetails]
                WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_ParcelDetailListCount ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_vw_ParcelDetailListCount] ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                columns: new[] { "Complete", "Removed" });

            migrationBuilder.Sql($@"
                CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]
                WITH SCHEMABINDING
                AS
                SELECT COUNT_BIG(*) as Count
                FROM [{Infrastructure.Schema.Legacy}].[ParcelDetails]
                WHERE [Complete] = 1 AND [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_ParcelDetailListCount ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount] (Count)");
        }
    }
}
