using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Wfs.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryWfs");

            migrationBuilder.CreateTable(
                name: "ParcelWfs",
                schema: "ParcelRegistryWfs",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VbrCaPaKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelWfs", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                    table.UniqueConstraint("AK_ParcelWfs_CaPaKey", x => x.CaPaKey);
                });

            migrationBuilder.CreateTable(
                name: "ParcelWfsAddresses",
                schema: "ParcelRegistryWfs",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelWfsAddresses", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryWfs",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelWfs_CaPaKey",
                schema: "ParcelRegistryWfs",
                table: "ParcelWfs",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelWfs_Removed",
                schema: "ParcelRegistryWfs",
                table: "ParcelWfs",
                column: "Removed");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelWfs_Status",
                schema: "ParcelRegistryWfs",
                table: "ParcelWfs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelWfsAddresses_AddressPersistentLocalId",
                schema: "ParcelRegistryWfs",
                table: "ParcelWfsAddresses",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelWfsAddresses_ParcelId",
                schema: "ParcelRegistryWfs",
                table: "ParcelWfsAddresses",
                column: "ParcelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelWfs",
                schema: "ParcelRegistryWfs");

            migrationBuilder.DropTable(
                name: "ParcelWfsAddresses",
                schema: "ParcelRegistryWfs");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryWfs");
        }
    }
}
