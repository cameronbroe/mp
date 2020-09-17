﻿// <auto-generated />
using System;
using MP.Core.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MP.Core.Npgsql.Migrations
{
    [DbContext(typeof(MPContext))]
    [Migration("20200917171555_AddProcessedFormatToMediaFiles")]
    partial class AddProcessedFormatToMediaFiles
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("MP.Core.Models.Analysis", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("FileId")
                        .IsUnique();

                    b.ToTable("Analyses");
                });

            modelBuilder.Entity("MP.Core.Models.AudioStream", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AnalysisId")
                        .HasColumnType("uuid");

                    b.Property<int>("BitRate")
                        .HasColumnType("integer");

                    b.Property<string>("ChannelLayout")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Channels")
                        .HasColumnType("integer");

                    b.Property<string>("CodecLongName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CodecName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.Property<string>("Profile")
                        .HasColumnType("text");

                    b.Property<int>("SampleRateHz")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId");

                    b.ToTable("AudioStreams");
                });

            modelBuilder.Entity("MP.Core.Models.MediaFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("BytesPerSecond")
                        .HasColumnType("bigint");

                    b.Property<string>("ContentType")
                        .HasColumnType("text");

                    b.Property<string>("FileExt")
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .HasColumnType("character varying(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("FilenameData")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastProcessingUpdate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ProcessedFormat")
                        .HasColumnType("text");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("BytesPerSecond");

                    b.HasIndex("LastProcessingUpdate");

                    b.ToTable("MediaFiles");
                });

            modelBuilder.Entity("MP.Core.Models.MediaFormat", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AnalysisId")
                        .HasColumnType("uuid");

                    b.Property<double>("BitRate")
                        .HasColumnType("double precision");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<string>("FormatLongName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FormatName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("ProbeScore")
                        .HasColumnType("double precision");

                    b.Property<int>("StreamCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId")
                        .IsUnique();

                    b.ToTable("MediaFormats");
                });

            modelBuilder.Entity("MP.Core.Models.VideoStream", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AnalysisId")
                        .HasColumnType("uuid");

                    b.Property<double>("AvgFrameRate")
                        .HasColumnType("double precision");

                    b.Property<int>("BitRate")
                        .HasColumnType("integer");

                    b.Property<int>("BitsPerRawSample")
                        .HasColumnType("integer");

                    b.Property<string>("CodecLongName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CodecName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisplayAspectRatio")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval");

                    b.Property<double>("FrameRate")
                        .HasColumnType("double precision");

                    b.Property<int>("Height")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.Property<string>("PixelFormat")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Profile")
                        .HasColumnType("text");

                    b.Property<int>("Rotation")
                        .HasColumnType("integer");

                    b.Property<int>("Width")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisId");

                    b.ToTable("VideoStreams");
                });

            modelBuilder.Entity("MP.Core.Models.Analysis", b =>
                {
                    b.HasOne("MP.Core.Models.MediaFile", "File")
                        .WithOne("Analysis")
                        .HasForeignKey("MP.Core.Models.Analysis", "FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MP.Core.Models.AudioStream", b =>
                {
                    b.HasOne("MP.Core.Models.Analysis", "Analysis")
                        .WithMany("AudioStreams")
                        .HasForeignKey("AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MP.Core.Models.MediaFormat", b =>
                {
                    b.HasOne("MP.Core.Models.Analysis", "Analysis")
                        .WithOne("Format")
                        .HasForeignKey("MP.Core.Models.MediaFormat", "AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MP.Core.Models.VideoStream", b =>
                {
                    b.HasOne("MP.Core.Models.Analysis", "Analysis")
                        .WithMany("VideoStreams")
                        .HasForeignKey("AnalysisId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
