﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelRegistry.Producer.Ldes;

#nullable disable

namespace ParcelRegistry.Producer.Ldes.Migrations
{
    [DbContext(typeof(ProducerContext))]
    [Migration("20250321124400_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.ToTable("ProjectionStates", "ParcelRegistryProducerLdes");
                });

            modelBuilder.Entity("ParcelRegistry.Producer.Ldes.ParcelDetail", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("LastEventHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StatusAsString")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Status");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("VersionTimestamp");

                    b.HasKey("ParcelId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId"), false);

                    b.HasAlternateKey("CaPaKey");

                    b.HasIndex("CaPaKey");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CaPaKey"));

                    b.HasIndex("IsRemoved");

                    b.HasIndex("StatusAsString");

                    b.ToTable("ParcelDetails", "ParcelRegistryProducerLdes");
                });

            modelBuilder.Entity("ParcelRegistry.Producer.Ldes.ParcelDetailAddress", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"));

                    b.HasIndex("AddressPersistentLocalId");

                    b.ToTable("ParcelDetailAddresses", "ParcelRegistryProducerLdes");
                });

            modelBuilder.Entity("ParcelRegistry.Producer.Ldes.ParcelDetailAddress", b =>
                {
                    b.HasOne("ParcelRegistry.Producer.Ldes.ParcelDetail", null)
                        .WithMany("Addresses")
                        .HasForeignKey("ParcelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ParcelRegistry.Producer.Ldes.ParcelDetail", b =>
                {
                    b.Navigation("Addresses");
                });
#pragma warning restore 612, 618
        }
    }
}
