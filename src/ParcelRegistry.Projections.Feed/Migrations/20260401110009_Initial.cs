using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelRegistry.Projections.Feed.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ParcelRegistryFeed");

            migrationBuilder.CreateSequence(
                name: "ParcelFeedSequence",
                schema: "ParcelRegistryFeed");

            migrationBuilder.CreateTable(
                name: "ParcelDocuments",
                schema: "ParcelRegistryFeed",
                columns: table => new
                {
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RecordCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelDocuments", x => x.CaPaKey)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ParcelFeed",
                schema: "ParcelRegistryFeed",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaPaKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: true),
                    Modification = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudEvent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelFeed", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryFeed",
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
                name: "IX_ParcelFeed_CaPaKey",
                schema: "ParcelRegistryFeed",
                table: "ParcelFeed",
                column: "CaPaKey");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelFeed_Page",
                schema: "ParcelRegistryFeed",
                table: "ParcelFeed",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelFeed_ParcelId",
                schema: "ParcelRegistryFeed",
                table: "ParcelFeed",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelFeed_Position",
                schema: "ParcelRegistryFeed",
                table: "ParcelFeed",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParcelDocuments",
                schema: "ParcelRegistryFeed");

            migrationBuilder.DropTable(
                name: "ParcelFeed",
                schema: "ParcelRegistryFeed");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "ParcelRegistryFeed");

            migrationBuilder.DropSequence(
                name: "ParcelFeedSequence",
                schema: "ParcelRegistryFeed");
        }
    }
}
