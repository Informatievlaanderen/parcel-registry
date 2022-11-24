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
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{ParcelDetailV2.ParcelDetailV2Configuration.TableName}]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailV2ListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressPersistentLocalIds",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication");

            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.ParcelDetailV2ListCountName}] ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]");
        }
    }
}
