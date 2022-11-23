﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelRegistry.Projections.Legacy;

#nullable disable

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    [Migration("20221123144056_AddV2Tables_P2")]
    partial class AddV2Tables_P2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

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

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Name"));

                    b.ToTable("ProjectionStates", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetail", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PersistentLocalId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"), false);

                    b.HasIndex("PersistentLocalId")
                        .IsUnique()
                        .HasDatabaseName("IX_ParcelDetails_PersistentLocalId_1")
                        .HasFilter("([PersistentLocalId] IS NOT NULL)");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("PersistentLocalId"), false);

                    b.HasIndex("Removed");

                    b.HasIndex("Status");

                    b.ToTable("ParcelDetails", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetailAddress", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ParcelId", "AddressId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressId"));

                    b.ToTable("ParcelAddresses", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetailListViewCount", b =>
                {
                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.ToView("vw_ParcelDetailListCount", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailAddressV2", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"));

                    b.ToTable("ParcelAddressesV2", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailV2", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LastEventHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Removed")
                        .HasColumnType("bit");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"), false);

                    b.HasIndex("CaPaKey")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.HasIndex("Removed");

                    b.HasIndex("Status");

                    b.ToTable("ParcelDetailsV2", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailV2ListViewCount", b =>
                {
                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.ToView("vw_ParcelDetailV2ListCount", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelSyndication.ParcelSyndicationItem", b =>
                {
                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.Property<string>("AddressPersistentLocalIdsAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("AddressPersistentLocalIds");

                    b.Property<string>("AddressesAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("AddressIds");

                    b.Property<int?>("Application")
                        .HasColumnType("int");

                    b.Property<string>("CaPaKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ChangeType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventDataAsXml")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("LastChangedOn");

                    b.Property<int?>("Modification")
                        .HasColumnType("int");

                    b.Property<string>("Operator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Organisation")
                        .HasColumnType("int");

                    b.Property<Guid?>("ParcelId")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Reason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("RecordCreatedAt");

                    b.Property<string>("StatusAsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("SyndicationItemCreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<decimal?>("XCoordinate")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("YCoordinate")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Position");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Position"));

                    b.HasIndex("ParcelId");

                    b.HasIndex("Position")
                        .HasDatabaseName("CI_ParcelSyndication_Position")
                        .HasAnnotation("SqlServer:ColumnStoreIndex", "");

                    b.ToTable("ParcelSyndication", "ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetailAddress", b =>
                {
                    b.HasOne("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetail", null)
                        .WithMany("Addresses")
                        .HasForeignKey("ParcelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailAddressV2", b =>
                {
                    b.HasOne("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailV2", null)
                        .WithMany("Addresses")
                        .HasForeignKey("ParcelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetail", b =>
                {
                    b.Navigation("Addresses");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetailV2.ParcelDetailV2", b =>
                {
                    b.Navigation("Addresses");
                });
#pragma warning restore 612, 618
        }
    }
}
