using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    public partial class MoreThumbnails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Thumbnail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThumbnailBytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Width = table.Column<long>(type: "INTEGER", nullable: true),
                    Height = table.Column<long>(type: "INTEGER", nullable: true),
                    VideoDataId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Thumbnail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Thumbnail_Videos_VideoDataId",
                        column: x => x.VideoDataId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Thumbnail_VideoDataId",
                table: "Thumbnail",
                column: "VideoDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Thumbnail");
        }
    }
}
