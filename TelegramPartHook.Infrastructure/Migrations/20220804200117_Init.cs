using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

#nullable disable

namespace TelegramPartHook.Domain.Aggregations.CacheAggregation
{
    [DbContext(typeof(BotContext))]
    [Migration("20220804200117_Init")]
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "client",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    telegramid = table.Column<string>(type: "text", nullable: false),
                    fullname = table.Column<string>(type: "text", nullable: false),
                    culture = table.Column<string>(type: "text", nullable: true, defaultValue: "en"),
                    isvip = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    unsubscribe = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    searchesok = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    searchesfailed = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    searchesscheduled = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    searchescount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    lastsearchdate = table.Column<string>(type: "text", nullable: true, defaultValue: "04/08/2022 22:01:17"),
                    vipinformation = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    repertoire = table.Column<Repertoire>(type: "jsonb", nullable: true, defaultValue: default)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "config",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    minutestoclean = table.Column<int>(type: "integer", nullable: false),
                    nextdatetomonitorrun = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "search",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    term = table.Column<string>(type: "text", nullable: false),
                    results = table.Column<SheetSearchResult[]>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "instacache",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pagename = table.Column<string>(type: "text", nullable: false),
                    endcursor = table.Column<string>(type: "text", nullable: false),
                    items = table.Column<InstagramItem[]>(type: "jsonb", nullable: true),
                    graphqlid = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instacache", x => x.id);
                });                
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client",
                schema: "public");

            migrationBuilder.DropTable(
                name: "config",
                schema: "public");

            migrationBuilder.DropTable(
                name: "search",
                schema: "public");
        }
    }
}
