using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddParcelDetailCountView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            CREATE VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[ParcelDetails]
            WHERE [Complete] = 1 AND [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_vw_ParcelDetailListCount ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount] (Count)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP INDEX [IX_vw_ParcelDetailListCount] ON [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");
            migrationBuilder.Sql($@"DROP VIEW [{Infrastructure.Schema.Legacy}].[vw_ParcelDetailListCount]");
        }
    }
}
