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
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VamBooru.Models.Author", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("VamBooru.Models.Scene", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("AuthorId");

                    b.Property<string>("ImageUrl");

                    b.Property<bool>("Published");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Scenes");
                });

            modelBuilder.Entity("VamBooru.Models.SceneTag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("SceneId");

                    b.Property<Guid?>("TagId");

                    b.HasKey("Id");

                    b.HasIndex("SceneId");

                    b.HasIndex("TagId");

                    b.ToTable("SceneTag");
                });

            modelBuilder.Entity("VamBooru.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("VamBooru.Models.Scene", b =>
                {
                    b.HasOne("VamBooru.Models.Author", "Author")
                        .WithMany("Scenes")
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("VamBooru.Models.SceneTag", b =>
                {
                    b.HasOne("VamBooru.Models.Scene", "Scene")
                        .WithMany("Tags")
                        .HasForeignKey("SceneId");

                    b.HasOne("VamBooru.Models.Tag", "Tag")
                        .WithMany("Scenes")
                        .HasForeignKey("TagId");
                });
#pragma warning restore 612, 618
        }
    }
}
