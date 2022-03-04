﻿// <auto-generated />
using System;
using Art.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Art.EF.Sqlite.Migrations
{
    [DbContext(typeof(ArtifactContext))]
    partial class ArtifactContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("Art.ArtifactInfoModel", b =>
                {
                    b.Property<string>("Tool")
                        .HasColumnType("TEXT");

                    b.Property<string>("Group")
                        .HasColumnType("TEXT");

                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Date")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Full")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("RetrievalDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("UpdateDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Tool", "Group", "Id");

                    b.ToTable("ArtifactInfoModels");
                });

            modelBuilder.Entity("Art.ArtifactResourceInfoModel", b =>
                {
                    b.Property<string>("ArtifactTool")
                        .HasColumnType("TEXT");

                    b.Property<string>("ArtifactGroup")
                        .HasColumnType("TEXT");

                    b.Property<string>("ArtifactId")
                        .HasColumnType("TEXT");

                    b.Property<string>("File")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChecksumId")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("ChecksumValue")
                        .HasColumnType("BLOB");

                    b.Property<string>("ContentType")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Retrieved")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Updated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .HasColumnType("TEXT");

                    b.HasKey("ArtifactTool", "ArtifactGroup", "ArtifactId", "File", "Path");

                    b.HasIndex("ArtifactTool", "ArtifactGroup", "ArtifactId");

                    b.ToTable("ArtifactResourceInfoModels");
                });

            modelBuilder.Entity("Art.ArtifactResourceInfoModel", b =>
                {
                    b.HasOne("Art.ArtifactInfoModel", "Artifact")
                        .WithMany("Resources")
                        .HasForeignKey("ArtifactTool", "ArtifactGroup", "ArtifactId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artifact");
                });

            modelBuilder.Entity("Art.ArtifactInfoModel", b =>
                {
                    b.Navigation("Resources");
                });
#pragma warning restore 612, 618
        }
    }
}
