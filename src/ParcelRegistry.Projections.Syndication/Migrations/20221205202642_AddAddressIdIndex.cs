using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Syndication.Migrations
{
    public partial class AddAddressIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AddressPersistentLocalIdSyndication_PersistentLocalId",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressPersistentLocalIdSyndication_PersistentLocalId",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AlterColumn<string>(
                name: "PersistentLocalId",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
