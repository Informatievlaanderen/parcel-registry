using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    public partial class AddIndexOnPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses",
                column: "Position");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_Position",
                schema: "ParcelRegistryConsumerAddress",
                table: "Addresses");
        }
    }
}
