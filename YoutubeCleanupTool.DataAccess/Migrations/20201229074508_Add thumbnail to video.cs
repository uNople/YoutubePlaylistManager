using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    public partial class Addthumbnailtovideo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailBytes",
                table: "Videos",
                type: "BLOB",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailBytes",
                table: "Videos");
        }
    }
}
