namespace ParcelRegistry.Projections.Integration.ParcelVersion
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using ParcelRegistry.Infrastructure;

    public sealed class ParcelVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

        public long Position { get; set; }

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public string? Status { get; set; }
        public string? OsloStatus { get; set; }
        public string Type { get; set; }
        public Geometry? Geometry { get; set; }

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

        public string CreatedOnAsString { get; set; }
        private DateTimeOffset CreatedOnTimestampAsDateTimeOffset { get; set; }

        public Instant CreatedOnTimestamp
        {
            get => Instant.FromDateTimeOffset(CreatedOnTimestampAsDateTimeOffset);
            set
            {
                CreatedOnTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                CreatedOnAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public ParcelVersion()
        { }

        public ParcelVersion CloneAndApplyEventInfo(long newPosition,
            string eventName,
            Instant lastChangedOn,
            Action<ParcelVersion> editFunc)
        {
            var newItem = new ParcelVersion
            {
                Position = newPosition,
                ParcelId = ParcelId,
                CaPaKey = CaPaKey,
                Status = Status,
                OsloStatus = OsloStatus,
                Type = eventName,
                Geometry = Geometry,
                Puri = Puri,
                Namespace = Namespace,
                IsRemoved = IsRemoved,
                VersionTimestamp = lastChangedOn,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public sealed class ParcelLatestItemConfiguration : IEntityTypeConfiguration<ParcelVersion>
    {
        public void Configure(EntityTypeBuilder<ParcelVersion> builder)
        {
            builder
                .ToTable("parcel_version", Schema.Integration)
                .HasKey(parcel => new { parcel.Position, parcel.ParcelId});

            builder.Property(parcel => parcel.Position).ValueGeneratedNever();

            builder.Property(parcel => parcel.Position).HasColumnName("position");

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
                .HasColumnName("status");

            builder
                .Property(parcel => parcel.OsloStatus)
                .HasColumnName("oslo_status");

            builder
                .Property(parcel => parcel.Type)
                .HasColumnName("type");

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
                .HasColumnName("geometry");

            builder.Property(parcel => parcel.VersionAsString).HasColumnName("version_as_string");
            builder.Property(parcel => parcel.CreatedOnAsString).HasColumnName("created_on_as_string");

            builder.Property(ParcelVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(ParcelVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

            builder.Ignore(parcel => parcel.VersionTimestamp);
            builder.Ignore(parcel => parcel.CreatedOnTimestamp);


            builder.HasIndex(parcel => parcel.CaPaKey);
            builder.HasIndex(parcel => parcel.Status);
            builder.HasIndex(parcel => parcel.OsloStatus);
            builder.HasIndex(parcel => parcel.Type);
            builder.HasIndex(parcel => parcel.IsRemoved);
            builder
                .HasIndex(parcel => parcel.Geometry)
                .HasMethod("GIST");
            builder.HasIndex(ParcelVersion.VersionTimestampBackingPropertyName);
        }
    }
}
