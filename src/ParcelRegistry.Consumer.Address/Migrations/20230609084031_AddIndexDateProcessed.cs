using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Consumer.Address.Migrations
{
    public partial class AddIndexDateProcessed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "ParcelRegistryConsumerAddress",
                table: "ProcessedMessages",
                column: "DateProcessed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "ParcelRegistryConsumerAddress",
                table: "ProcessedMessages");
        }
    }
}
