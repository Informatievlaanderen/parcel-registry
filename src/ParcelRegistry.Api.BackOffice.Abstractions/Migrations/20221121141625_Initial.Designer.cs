﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelRegistry.Api.BackOffice.Abstractions;

#nullable disable

namespace ParcelRegistry.Api.BackOffice.Abstractions.Migrations
{
    [DbContext(typeof(BackOfficeContext))]
    [Migration("20221121141625_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ParcelRegistry.Api.BackOffice.Abstractions.ParcelAddressRelation", b =>
                {
                    b.Property<Guid>("ParcelId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AddressPersistentLocalId")
                        .HasColumnType("int");

                    b.HasKey("ParcelId", "AddressPersistentLocalId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("ParcelId", "AddressPersistentLocalId"));

                    b.HasIndex("AddressPersistentLocalId");

                    b.HasIndex("ParcelId");

                    b.ToTable("ParcelAddressRelation", "ParcelRegistryBackOffice");
                });
#pragma warning restore 612, 618
        }
    }
}
