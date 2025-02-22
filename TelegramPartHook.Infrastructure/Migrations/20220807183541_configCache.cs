using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    public partial class configCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "nextdatetocacheclear",
                schema: "public",
                table: "config",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
            name: "nextdatesearchoninstagram",
            schema: "public",
            table: "config",
            type: "timestamp",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "lastsearchdate",
                schema: "public",
                table: "client",
                type: "text",
                nullable: true,
                defaultValue: "07/08/2022 20:35:41",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "04/08/2022 22:01:17");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextDateToCacheClear",
                schema: "public",
                table: "config");

            migrationBuilder.AlterColumn<string>(
                name: "lastsearchdate",
                schema: "public",
                table: "client",
                type: "text",
                nullable: true,
                defaultValue: "04/08/2022 22:01:17",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "07/08/2022 20:35:41");
        }
    }
}
