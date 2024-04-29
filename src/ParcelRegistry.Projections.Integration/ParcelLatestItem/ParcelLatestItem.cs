namespace ParcelRegistry.Projections.Integration.ParcelLatestItem
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using ParcelRegistry.Infrastructure;

    public sealed class ParcelLatestItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public string Status { get; set; }
        public string OsloStatus { get; set; }
        public Geometry Geometry { get; set; }

        public string Puri { get; set; }
        public string Namespace { get; set; }

        public bool IsRemoved { get; set; }

        public string VersionAsString { get; set; }
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public ParcelLatestItem(
            Guid parcelId,
            string caPaKey,
            string status,
            string osloStatus,
            Geometry geometry,
            string puri,
            string ns,
            bool isRemoved,
            Instant versionTimestamp)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            Status = status;
            OsloStatus = osloStatus;
            Geometry = geometry;
            Puri = puri;
            Namespace = ns;
            IsRemoved = isRemoved;
            VersionTimestamp = versionTimestamp;
        }

        // This needs to be here to please EF
        protected ParcelLatestItem()
        { }
    }

    public sealed class ParcelLatestItemConfiguration : IEntityTypeConfiguration<ParcelLatestItem>
    {
        public void Configure(EntityTypeBuilder<ParcelLatestItem> builder)
        {
            builder
                .ToTable("parcel_latest_items", Schema.Integration)
                .HasKey(parcel => parcel.ParcelId);

            builder
                .Property(parcel => parcel.ParcelId)
                .HasColumnName("parcel_id");

            builder
                .Property(parcel => parcel.CaPaKey)
                .HasColumnName("capakey")
                .HasColumnType("varchar")
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(parcel => parcel.Status)
                .HasColumnName("status")
                .IsRequired();

            builder
                .Property(parcel => parcel.OsloStatus)
                .HasColumnName("oslo_status")
                .IsRequired();

            builder
                .Property(parcel => parcel.Puri)
                .HasColumnName("puri")
                .IsRequired();

            builder
                .Property(parcel => parcel.Namespace)
                .HasColumnName("namespace")
                .IsRequired();

            builder
                .Property(parcel => parcel.IsRemoved)
                .HasColumnName("is_removed")
                .HasColumnType("boolean")
                .IsRequired();

            builder
                .Property(parcel => parcel.Geometry)
                .HasColumnName("geometry")
                .IsRequired();

            builder.Property(parcel => parcel.VersionAsString).HasColumnName("version_as_string");
            builder.Property(ParcelLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");

            builder.Ignore(parcel => parcel.VersionTimestamp);

            builder.HasIndex(parcel => parcel.CaPaKey);
            builder.HasIndex(parcel => parcel.Status);
            builder.HasIndex(parcel => parcel.OsloStatus);
            builder.HasIndex(parcel => parcel.IsRemoved);
            builder.HasIndex(x => new { x.IsRemoved, x.Status });
            builder
                .HasIndex(parcel => parcel.Geometry)
                .HasMethod("GIST");
        }
    }
}
