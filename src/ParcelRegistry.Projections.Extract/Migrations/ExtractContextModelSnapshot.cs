﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelRegistry.Projections.Extract;

#nullable disable

namespace ParcelRegistry.Projections.Extract.Migrations
{
    [DbContext(typeof(ExtractContext))]
    partial class ExtractContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name");

                    b.ToTable("ProjectionStates", "ParcelRegistryExtract");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Extract.ParcelExtract.ParcelExtractItem", b =>
                {
                    b.Property<Guid?>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"), false);

                    b.HasIndex("CaPaKey");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.ToTable("Parcel", "ParcelRegistryExtract");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Extract.ParcelExtract.ParcelExtractItemV2", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("DbaseRecord")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"), false);

                    b.HasIndex("CaPaKey");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.ToTable("ParcelV2", "ParcelRegistryExtract");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Extract.ParcelLinkExtract.ParcelLinkExtractItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"), false);

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("CaPaKey");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.HasIndex("ParcelId");

                    b.ToTable("ParcelLinks", "ParcelRegistryExtract");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Extract.ParcelLinkExtractWithCount.ParcelLinkExtractItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"), false);

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("CaPaKey");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.HasIndex("ParcelId");

                    b.ToTable("ParcelLinksWithCount", "ParcelRegistryExtract");
                });
#pragma warning restore 612, 618
        }
    }
}
