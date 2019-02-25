using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Syndication.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "AddressOsloIdSyndication",
                schema: "ParcelRegistrySyndication",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    Version = table.Column<DateTimeOffset>(nullable: true),
                    Position = table.Column<long>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false),
                    IsRemoved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressOsloIdSyndication", x => x.AddressId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistrySyndication",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressOsloIdSyndication_IsComplete_IsRemoved",
                schema: "ParcelRegistrySyndication",
                table: "AddressOsloIdSyndication",
                columns: new[] { "IsComplete", "IsRemoved" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressOsloIdSyndication",
                schema: "ParcelRegistrySyndication");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistrySyndication");
        }
    }
}
