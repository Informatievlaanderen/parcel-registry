using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddParcelDetailCountView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailListCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{ParcelDetail.ParcelDetailConfiguration.TableName}]
            WHERE [Complete] = 1 AND [Removed] = 0 AND [PersistentLocalId] <> 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailListCountName}] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_{LegacyContext.ParcelDetailListCountName}] ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailListCountName}]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailListCountName}]");
        }
    }
}
