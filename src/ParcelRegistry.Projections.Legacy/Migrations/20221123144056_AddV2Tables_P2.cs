using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddV2Tables_P2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressPersistentLocalIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailV2ListCount]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[ParcelDetailsV2]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_ParcelDetailV2ListCount ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailV2ListCount] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressPersistentLocalIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.ParcelDetailListCountName}] ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailV2ListCount]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailV2ListCount]");
        }
    }
}
