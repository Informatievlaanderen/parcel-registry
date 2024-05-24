using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Extract.Migrations
{
    /// <inheritdoc />
    public partial class DeleteV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parcel",
                schema: "ParcelRegistryExtract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parcel",
                schema: "ParcelRegistryExtract",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcel", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parcel_CaPaKey",
                schema: "ParcelRegistryExtract",
                table: "Parcel",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
