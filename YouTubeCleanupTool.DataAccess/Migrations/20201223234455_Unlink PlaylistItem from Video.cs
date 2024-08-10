using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    public partial class UnlinkPlaylistItemfromVideo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistItems_Videos_VideoId",
                table: "PlaylistItems");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistItems_Videos_VideoId",
                table: "PlaylistItems",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
