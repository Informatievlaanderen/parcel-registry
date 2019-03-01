using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class AddSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelSyndication",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    Position = table.Column<long>(nullable: false),
                    ParcelId = table.Column<Guid>(nullable: false),
                    CaPaKey = table.Column<string>(nullable: true),
                    ChangeType = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    IsComplete = table.Column<bool>(nullable: false),
                    AddressOsloIds = table.Column<string>(nullable: true),
                    RecordCreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(nullable: false),
                    Application = table.Column<int>(nullable: true),
                    Modification = table.Column<int>(nullable: true),
                    Operator = table.Column<string>(nullable: true),
                    Organisation = table.Column<int>(nullable: true),
                    Plan = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelSyndication", x => x.Position)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelSyndication_ParcelId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelSyndication",
                column: "ParcelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelSyndication",
                schema: "ParcelRegistryLegacy");
        }
    }
}
