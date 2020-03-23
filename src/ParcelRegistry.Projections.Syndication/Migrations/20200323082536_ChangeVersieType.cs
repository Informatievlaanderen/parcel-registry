using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ParcelRegistry.Projections.Syndication.Migrations
{
    public partial class ChangeVersieType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Version",
                schema: "ParcelRegistrySyndication",
                table: "AddressPersistentLocalIdSyndication",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
