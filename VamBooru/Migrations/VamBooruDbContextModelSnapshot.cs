﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using VamBooru.Models;

namespace VamBooru.Migrations
{
    [DbContext(typeof(VamBooruDbContext))]
    partial class VamBooruDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("VamBooru.Models.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("AuthorId");

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<DateTimeOffset>("DatePublished");

                    b.Property<string>("ImageUrl");

                    b.Property<bool>("Published");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("VamBooru.Models.PostTag", b =>
                {
                    b.Property<Guid>("PostId");

                    b.Property<Guid>("TagId");

                    b.HasKey("PostId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("PostTags");
                });

            modelBuilder.Entity("VamBooru.Models.Scene", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("PostId");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Scenes");
                });

            modelBuilder.Entity("VamBooru.Models.SceneFile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Bytes")
                        .IsRequired();

                    b.Property<string>("Extension")
                        .IsRequired();

                    b.Property<string>("Filename")
                        .IsRequired();

                    b.Property<Guid>("SceneId");

                    b.HasKey("Id");

                    b.HasIndex("SceneId");

                    b.ToTable("SceneFiles");
                });

            modelBuilder.Entity("VamBooru.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("VamBooru.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("DateSubscribed");

                    b.Property<string>("Username")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("VamBooru.Models.UserLogin", b =>
                {
                    b.Property<string>("Scheme");

                    b.Property<string>("NameIdentifier");

                    b.Property<Guid>("UserId");

                    b.HasKey("Scheme", "NameIdentifier");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("VamBooru.Models.Post", b =>
                {
                    b.HasOne("VamBooru.Models.User", "Author")
                        .WithMany("Scenes")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VamBooru.Models.PostTag", b =>
                {
                    b.HasOne("VamBooru.Models.Post", "Post")
                        .WithMany("Tags")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("VamBooru.Models.Tag", "Tag")
                        .WithMany("Scenes")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VamBooru.Models.Scene", b =>
                {
                    b.HasOne("VamBooru.Models.Post", "Post")
                        .WithMany("Scenes")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VamBooru.Models.SceneFile", b =>
                {
                    b.HasOne("VamBooru.Models.Scene", "Scene")
                        .WithMany("Files")
                        .HasForeignKey("SceneId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VamBooru.Models.UserLogin", b =>
                {
                    b.HasOne("VamBooru.Models.User", "User")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
