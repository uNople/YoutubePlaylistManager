﻿// <auto-generated />

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    [DbContext(typeof(YouTubeCleanupToolDbContext))]
    partial class YouTubeCleanupToolDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("YouTubeCleanupTool.Domain.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CategoryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoDataId")
                        .HasColumnType("TEXT");

                    b.HasKey("CategoryId");

                    b.HasIndex("VideoDataId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.PlaylistData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Kind")
                        .HasColumnType("TEXT");

                    b.Property<string>("PrivacyStatus")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.PlaylistItemData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AddedToPlaylist")
                        .HasColumnType("TEXT");

                    b.Property<string>("Kind")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistDataId")
                        .HasColumnType("TEXT");

                    b.Property<long?>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PrivacyStatus")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoKind")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoPublishedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistDataId");

                    b.ToTable("PlaylistItems");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.VideoData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ChannelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChannelTitle")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeletedFromYouTube")
                        .HasColumnType("INTEGER");

                    b.Property<string>("License")
                        .HasColumnType("TEXT");

                    b.Property<string>("RecordingDate")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("ThumbnailBytes")
                        .HasColumnType("BLOB");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.Category", b =>
                {
                    b.HasOne("YouTubeCleanupTool.Domain.VideoData", null)
                        .WithMany("Categories")
                        .HasForeignKey("VideoDataId");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.PlaylistItemData", b =>
                {
                    b.HasOne("YouTubeCleanupTool.Domain.PlaylistData", null)
                        .WithMany("PlaylistItems")
                        .HasForeignKey("PlaylistDataId");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.PlaylistData", b =>
                {
                    b.Navigation("PlaylistItems");
                });

            modelBuilder.Entity("YouTubeCleanupTool.Domain.VideoData", b =>
                {
                    b.Navigation("Categories");
                });
#pragma warning restore 612, 618
        }
    }
}
