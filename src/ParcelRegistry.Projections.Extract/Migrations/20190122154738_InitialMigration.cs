using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Extract.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryExtract");

            migrationBuilder.CreateTable(
                name: "Parcel",
                schema: "ParcelRegistryExtract",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(nullable: false),
                    CaPaKey = table.Column<string>(nullable: true),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcel", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryExtract",
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
                name: "IX_Parcel_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "Parcel",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parcel",
                schema: "ParcelRegistryExtract");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryExtract");
        }
    }
}
