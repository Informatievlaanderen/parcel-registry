using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Syndication.Migrations
{
    public partial class AddClusteredIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressPersistentLocalIdSyndication",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                column: "AddressId")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
