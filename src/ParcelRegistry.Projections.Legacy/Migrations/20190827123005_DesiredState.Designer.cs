﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelRegistry.Projections.Legacy;

namespace ParcelRegistry.Projections.Legacy.Migrations
{
    [DbContext(typeof(LegacyContext))]
    [Migration("20190827123005_DesiredState")]
    partial class DesiredState
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DesiredState");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt");

                    b.Property<long>("Position");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetail", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Complete");

                    b.Property<string>("PersistentLocalId");

                    b.Property<bool>("Removed");

                    b.Property<string>("StatusAsString")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("ParcelId")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("Complete");

                    b.HasIndex("PersistentLocalId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Removed");

                    b.ToTable("ParcelDetails","ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetailAddress", b =>
                {
                    b.Property<Guid>("ParcelId");

                    b.Property<Guid>("AddressId");

                    b.HasKey("ParcelId", "AddressId")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ParcelAddresses","ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelSyndication.ParcelSyndicationItem", b =>
                {
                    b.Property<long>("Position");

                    b.Property<string>("AddressesAsString")
                        .HasColumnName("AddressPersistentLocalIds");

                    b.Property<int?>("Application");

                    b.Property<string>("CaPaKey");

                    b.Property<string>("ChangeType");

                    b.Property<string>("EventDataAsXml");

                    b.Property<bool>("IsComplete");

                    b.Property<DateTimeOffset>("LastChangedOnAsDateTimeOffset")
                        .HasColumnName("LastChangedOn");

                    b.Property<int?>("Modification");

                    b.Property<string>("Operator");

                    b.Property<int?>("Organisation");

                    b.Property<Guid?>("ParcelId")
                        .IsRequired();

                    b.Property<string>("Reason");

                    b.Property<DateTimeOffset>("RecordCreatedAtAsDateTimeOffset")
                        .HasColumnName("RecordCreatedAt");

                    b.Property<string>("StatusAsString")
                        .HasColumnName("Status");

                    b.HasKey("Position")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("ParcelId");

                    b.ToTable("ParcelSyndication","ParcelRegistryLegacy");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetailAddress", b =>
                {
                    b.HasOne("ParcelRegistry.Projections.Legacy.ParcelDetail.ParcelDetail")
                        .WithMany("Addresses")
                        .HasForeignKey("ParcelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}