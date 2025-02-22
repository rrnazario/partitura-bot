using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportImagesUrlIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_report_images_Url",
                schema: "public",
                table: "report_images",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_report_images_Url",
                schema: "public",
                table: "report_images");
        }
    }
}
