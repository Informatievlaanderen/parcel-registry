using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    using ParcelDetail;

    /// <inheritdoc />
    public partial class UpdateListCountViewToWithCountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            ALTER VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[{ParcelDetailConfiguration.TableName}]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailV2ListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}] (Count)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
            ALTER VIEW [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}]
            WITH SCHEMABINDING
            AS
            SELECT COUNT_BIG(*) as Count
            FROM [{Infrastructure.Schema.Legacy}].[ParcelDetailsV2]
            WHERE [Removed] = 0");

            migrationBuilder.Sql($@"CREATE UNIQUE CLUSTERED INDEX IX_{LegacyContext.ParcelDetailV2ListCountName} ON [{Infrastructure.Schema.Legacy}].[{LegacyContext.ParcelDetailV2ListCountName}] (Count)");
        }
    }
}
