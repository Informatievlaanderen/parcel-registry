using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Producer.Ldes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryProducerLdes");

            migrationBuilder.CreateTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryProducerLdes",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetails", x => x.ParcelId)
                        .Annotation("SqlServer:Clustered", false);
                    table.UniqueConstraint("AK_ParcelDetails_CaPaKey", x => x.CaPaKey);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryProducerLdes",
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

            migrationBuilder.CreateTable(
                name: "ParcelDetailAddresses",
                schema: "ParcelRegistryProducerLdes",
                columns: table => new
                {
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDetailAddresses", x => new { x.ParcelId, x.AddressPersistentLocalId })
                        .Annotation("SqlServer:Clustered", true);
                    table.ForeignKey(
                        name: "FK_ParcelDetailAddresses_ParcelDetails_ParcelId",
                        column: x => x.ParcelId,
                        principalSchema: "ParcelRegistryProducerLdes",
                        principalTable: "ParcelDetails",
                        principalColumn: "ParcelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetailAddresses_AddressPersistentLocalId",
                schema: "ParcelRegistryProducerLdes",
                table: "ParcelDetailAddresses",
                column: "AddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_CaPaKey",
                schema: "ParcelRegistryProducerLdes",
                table: "ParcelDetails",
                column: "CaPaKey")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_IsRemoved",
                schema: "ParcelRegistryProducerLdes",
                table: "ParcelDetails",
                column: "IsRemoved");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelDetails_Status",
                schema: "ParcelRegistryProducerLdes",
                table: "ParcelDetails",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelDetailAddresses",
                schema: "ParcelRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryProducerLdes");

            migrationBuilder.DropTable(
                name: "ParcelDetails",
                schema: "ParcelRegistryProducerLdes");
        }
    }
}
