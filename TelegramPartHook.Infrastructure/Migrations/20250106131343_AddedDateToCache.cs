using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedDateToCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedDate",
                schema: "public",
                table: "search",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedDate",
                schema: "public",
                table: "search");
        }
    }
}
