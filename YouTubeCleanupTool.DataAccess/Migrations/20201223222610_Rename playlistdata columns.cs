using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    public partial class Renameplaylistdatacolumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistItems",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ItemKind",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "PlaylistItems");

            migrationBuilder.RenameColumn(
                name: "PlaylistTitle",
                table: "PlaylistItems",
                newName: "PlaylistDataId");

            migrationBuilder.RenameColumn(
                name: "PlaylistPrivacyStatus",
                table: "PlaylistItems",
                newName: "Kind");

            migrationBuilder.AlterColumn<string>(
                name: "VideoId",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistItems",
                table: "PlaylistItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_PlaylistDataId",
                table: "PlaylistItems",
                column: "PlaylistDataId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistItems_Playlists_PlaylistDataId",
                table: "PlaylistItems",
                column: "PlaylistDataId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistItems_Videos_VideoId",
                table: "PlaylistItems",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistItems_Playlists_PlaylistDataId",
                table: "PlaylistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistItems_Videos_VideoId",
                table: "PlaylistItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistItems",
                table: "PlaylistItems");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistItems_PlaylistDataId",
                table: "PlaylistItems");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistItems_VideoId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlaylistItems");

            migrationBuilder.RenameColumn(
                name: "PlaylistDataId",
                table: "PlaylistItems",
                newName: "PlaylistTitle");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "PlaylistItems",
                newName: "PlaylistPrivacyStatus");

            migrationBuilder.AlterColumn<string>(
                name: "VideoId",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemKind",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaylistId",
                table: "PlaylistItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistItems",
                table: "PlaylistItems",
                column: "VideoId");
        }
    }
}
