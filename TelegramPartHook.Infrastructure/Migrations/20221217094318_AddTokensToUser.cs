using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    public partial class AddTokensToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatetomonitorrun",
                schema: "public",
                table: "config",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatetocacheclear",
                schema: "public",
                table: "config",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatesearchoninstagram",
                schema: "public",
                table: "config",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<string>(
                name: "lastsearchdate",
                schema: "public",
                table: "client",
                type: "text",
                nullable: true,
                defaultValue: "17/12/2022 10:43:18",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "25/08/2022 21:57:47");

            migrationBuilder.AddColumn<string>(
                name: "activetokens",
                schema: "public",
                table: "client",
                type: "text",
                nullable: true,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activetokens",
                schema: "public",
                table: "client");

            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatetomonitorrun",
                schema: "public",
                table: "config",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatetocacheclear",
                schema: "public",
                table: "config",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "nextdatesearchoninstagram",
                schema: "public",
                table: "config",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "lastsearchdate",
                schema: "public",
                table: "client",
                type: "text",
                nullable: true,
                defaultValue: "25/08/2022 21:57:47",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "17/12/2022 10:43:18");
        }
    }
}
