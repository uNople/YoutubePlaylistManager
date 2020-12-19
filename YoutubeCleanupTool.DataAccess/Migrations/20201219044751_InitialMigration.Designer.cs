﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YoutubeCleanupTool.DataAccess;

namespace YoutubeCleanupTool.DataAccess.Migrations
{
    [DbContext(typeof(YoutubeCleanupToolDbContext))]
    [Migration("20201219044751_InitialMigration")]
    partial class InitialMigration
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

            modelBuilder.Entity("YoutubeCleanupTool.Model.PlaylistData", b =>
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

            modelBuilder.Entity("YoutubeCleanupTool.Model.PlaylistItemData", b =>
                {
                    b.Property<string>("VideoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("AddedToPlaylist")
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemKind")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistId")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistPrivacyStatus")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistTitle")
                        .HasColumnType("TEXT");

                    b.Property<long?>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PrivacyStatus")
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoKind")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoPublishedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("VideoId");

                    b.ToTable("PlaylistItems");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Model.VideoData", b =>
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
                    b.HasOne("YoutubeCleanupTool.Model.VideoData", null)
                        .WithMany("Categories")
                        .HasForeignKey("VideoDataId");
                });

            modelBuilder.Entity("YoutubeCleanupTool.Model.VideoData", b =>
                {
                    b.Navigation("Categories");
                });
#pragma warning restore 612, 618
        }
    }
}
