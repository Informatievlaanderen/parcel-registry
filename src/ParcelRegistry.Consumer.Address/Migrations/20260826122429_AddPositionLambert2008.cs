using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionLambert2008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "PositionLambert2008",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                type: "sys.geometry",
                nullable: true);

            // The Lambert 72 bounding box of SPATIAL_Addresses_Position expressed in Lambert 2008: all four
            // corners transformed, then the envelope padded out to the next 100 m. Lambert 2008 coordinates
            // fall entirely outside the Lambert 72 box, so the two indexes cannot share one. See ADR 0004.
            migrationBuilder.Sql(@"CREATE SPATIAL INDEX [SPATIAL_Addresses_PositionLambert2008] ON [ParcelRegistryConsumerAddress].[Addresses] ([PositionLambert2008])
	            USING GEOMETRY_GRID
	            WITH (
		            BOUNDING_BOX =(522200, 653000, 758900, 744100),
		            GRIDS =(
			            LEVEL_1 = MEDIUM,
			            LEVEL_2 = MEDIUM,
			            LEVEL_3 = MEDIUM,
			            LEVEL_4 = MEDIUM),
	            CELLS_PER_OBJECT = 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [SPATIAL_Addresses_PositionLambert2008] ON [ParcelRegistryConsumerAddress].[Addresses]");

            migrationBuilder.DropColumn(
                name: "PositionLambert2008",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");
        }
    }
}
