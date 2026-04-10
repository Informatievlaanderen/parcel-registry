namespace ParcelRegistry.Projections.Wfs.ParcelWfs
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using Parcel;

    public class ParcelWfsItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        private ParcelWfsItem()
        { }

        public ParcelWfsItem(
            Guid parcelId,
            string caPaKey,
            string vbrCaPaKey,
            ParcelStatus status,
            bool removed,
            Instant versionTimestamp)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            VbrCaPaKey = vbrCaPaKey;
            Status = status;
            Removed = removed;
            VersionTimestamp = versionTimestamp;
        }

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public string VbrCaPaKey { get; set; }

        public ParcelStatus Status
        {
            get => ParcelStatus.Parse(StatusAsString);
            set => StatusAsString = value.Status;
        }

        public string StatusAsString { get; private set; }

        public bool Removed { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class ParcelWfsItemConfiguration : IEntityTypeConfiguration<ParcelWfsItem>
    {
        internal const string TableName = "ParcelWfs";

        public void Configure(EntityTypeBuilder<ParcelWfsItem> builder)
        {
            builder.ToTable(TableName, Schema.Wfs)
                .HasKey(p => p.ParcelId)
                .IsClustered(false);

            builder.Ignore(x => x.Status);

            builder.Property(p => p.CaPaKey);
            builder.Property(p => p.VbrCaPaKey);

            builder.Property(x => x.StatusAsString)
                .HasMaxLength(450)
                .HasColumnName("Status");

            builder.Property(p => p.Removed);

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");
            builder.Ignore(x => x.VersionTimestamp);

            builder.HasIndex(x => x.CaPaKey).IsClustered();
            builder.HasAlternateKey(x => x.CaPaKey);

            builder.HasIndex(x => x.Removed);
            builder.HasIndex(x => x.StatusAsString);
        }
    }
}
