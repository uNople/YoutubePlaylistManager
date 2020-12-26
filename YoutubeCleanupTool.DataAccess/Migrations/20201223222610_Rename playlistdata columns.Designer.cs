﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YouTubeCleanupTool.DataAccess;

namespace YouTubeCleanupTool.DataAccess.Migrations
{
    [DbContext(typeof(YoutubeCleanupToolDbContext))]
    [Migration("20201223222610_Rename playlistdata columns")]
    partial class Renameplaylistdatacolumns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("YoutubeCleanupTool.Domain.Category", b =>
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

            modelBuilder.Entity("YoutubeCleanupTool.Domain.PlaylistData", b =>
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

            modelBuilder.Entity("YoutubeCleanupTool.Domain.PlaylistItemData", b =>
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

                    b.HasIndex("VideoId");

                    b.ToTable("PlaylistItems");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Domain.VideoData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("License")
                        .HasColumnType("TEXT");

                    b.Property<string>("RecordingDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Domain.Category", b =>
                {
                    b.HasOne("YoutubeCleanupTool.Domain.VideoData", null)
                        .WithMany("Categories")
                        .HasForeignKey("VideoDataId");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Domain.PlaylistItemData", b =>
                {
                    b.HasOne("YoutubeCleanupTool.Domain.PlaylistData", null)
                        .WithMany("PlaylistItems")
                        .HasForeignKey("PlaylistDataId");

                    b.HasOne("YoutubeCleanupTool.Domain.VideoData", "Video")
                        .WithMany()
                        .HasForeignKey("VideoId");

                    b.Navigation("Video");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Domain.PlaylistData", b =>
                {
                    b.Navigation("PlaylistItems");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Domain.VideoData", b =>
                {
                    b.Navigation("Categories");
                });
#pragma warning restore 612, 618
        }
    }
}
