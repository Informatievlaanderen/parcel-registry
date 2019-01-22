using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryLegacy");

            migrationBuilder.CreateTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(nullable: false),
                    OsloId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    Removed = table.Column<bool>(nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetails", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryLegacy",
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

            migrationBuilder.CreateTable(
                name: "ParcelAddresses",
                schema: "ParcelRegistryLegacy",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(nullable: false),
                    AddressId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelAddresses", x => new { x.ParcelId, x.AddressId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ParcelAddresses_ParcelDetails_ParcelId",
                        column: x => x.ParcelId,
                        principalSchema: "ParcelRegistryLegacy",
                        principalTable: "ParcelDetails",
                        principalColumn: "ParcelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Complete",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_OsloId",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "OsloId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Removed",
                schema: "ParcelRegistryLegacy",
                table: "ParcelDetails",
                column: "Removed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelAddresses",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryLegacy");

            migrationBuilder.DropTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryLegacy");
        }
    }
}
