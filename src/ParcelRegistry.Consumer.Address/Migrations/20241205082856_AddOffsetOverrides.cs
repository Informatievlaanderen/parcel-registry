using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    /// <inheritdoc />
    public partial class AddOffsetOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OffsetOverrides",
                schema: "ParcelRegistryConsumerAddress",
                columns: table => new
                {
                    ConsumerGroupId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Offset = table.Column<long>(type: "bigint", nullable: false),
                    Configured = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffsetOverrides", x => x.ConsumerGroupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OffsetOverrides",
                schema: "ParcelRegistryConsumerAddress");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");
        }
    }
}
