using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToSearchCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_search_term",
                schema: "public",
                table: "search",
                column: "term");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_search_term",
                schema: "public",
                table: "search");
        }
    }
}
