namespace ParcelRegistry.Projections.Legacy.ParcelDetailV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using Parcel;

    public class ParcelDetailV2
    {
        private ParcelDetailV2()
        {
            Addresses = new List<ParcelDetailAddressV2>();
        }

        public ParcelDetailV2(
            Guid parcelId,
            string caPaKey,
            ParcelStatus status,
            IEnumerable<ParcelDetailAddressV2> addresses,
            bool removed,
            Instant versionTimeStamp)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            Status = status;
            Addresses = addresses.ToList();
            Removed = removed;
            VersionTimestamp = versionTimeStamp;
        }

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public ParcelStatus Status { get; set; }

        public virtual List<ParcelDetailAddressV2> Addresses { get; set; }

        public bool Removed { get; set; }
        public string LastEventHash { get; set; } = string.Empty;
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class ParcelDetailV2Configuration : IEntityTypeConfiguration<ParcelDetailV2>
    {
        internal const string TableName = "ParcelDetailsV2";

        public void Configure(EntityTypeBuilder<ParcelDetailV2> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.ParcelId)
                .IsClustered(false);

            builder.Property(p => p.CaPaKey);
            builder.Property(x => x.Status)
                .HasConversion(
                    value => value.Status,
                    value => ParcelStatus.Parse(value))
                .HasColumnName("Status");

            builder.Property(p => p.Removed);
            builder.Property(x => x.LastEventHash);

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");
            builder.Ignore(x => x.VersionTimestamp);

            builder.HasMany(x => x.Addresses).WithOne()
                .HasForeignKey(x => x.ParcelId);

            builder.HasIndex(x => x.CaPaKey)
                .IsUnique()
                .IsClustered();
            builder.HasIndex(x => x.Removed);
            builder.HasIndex(x => x.Status);
        }
    }
}