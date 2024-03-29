﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ParcelRegistry.Projections.Integration;

#nullable disable

namespace ParcelRegistry.Projections.Integration.Migrations
{
    [DbContext(typeof(IntegrationContext))]
    [Migration("20240116143151_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("DesiredState")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name");

                    b.ToTable("ProjectionStates", "integration_parcel");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Integration.ParcelLatestItem.ParcelLatestItem", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("parcel_id");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar")
                        .HasColumnName("capakey");

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("geometry")
                        .HasColumnName("geometry");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean")
                        .HasColumnName("is_removed");

                    b.Property<string>("Namespace")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("namespace");

                    b.Property<string>("OsloStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("oslo_status");

                    b.Property<string>("Puri")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("puri");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("VersionAsString")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("version_as_string");

                    b.Property<DateTimeOffset>("VersionTimestampAsDateTimeOffset")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("version_timestamp");

                    b.HasKey("ParcelId");

                    b.HasIndex("CaPaKey");

                    b.HasIndex("Geometry");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Geometry"), "GIST");

                    b.HasIndex("IsRemoved");

                    b.HasIndex("OsloStatus");

                    b.HasIndex("Status");

                    b.ToTable("parcel_latest_items", "integration_parcel");
                });

            modelBuilder.Entity("ParcelRegistry.Projections.Integration.ParcelLatestItem.ParcelLatestItemAddress", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uuid")
                        .HasColumnName("parcel_id");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("integer")
                        .HasColumnName("address_persistent_local_id");

                    b.Property<string>("CaPaKey")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("capakey");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("CaPaKey");

                    b.HasIndex("ParcelId");

                    b.ToTable("parcel_latest_item_addresses", "integration_parcel");
                });
#pragma warning restore 612, 618
        }
    }
}
