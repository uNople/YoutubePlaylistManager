using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    public partial class FixupforeignkeyissuebetweenPlaylistDataandVideoData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistItems_Videos_VideoId",
                table: "PlaylistItems");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems");

            migrationBuilder.AddColumn<string>(
                name: "VideoDataId",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_VideoDataId",
                table: "PlaylistItems",
                column: "VideoDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistItems_Videos_VideoDataId",
                table: "PlaylistItems",
                column: "VideoDataId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistItems_Videos_VideoDataId",
                table: "PlaylistItems");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistItems_VideoDataId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "VideoDataId",
                table: "PlaylistItems");

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
