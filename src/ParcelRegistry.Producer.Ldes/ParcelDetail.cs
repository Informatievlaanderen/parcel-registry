namespace ParcelRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using Parcel;
    using ParcelRegistry.Infrastructure;

    public class ParcelDetail
    {
        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public ParcelStatus Status
        {
            get => ParcelStatus.Parse(StatusAsString);
            set => StatusAsString = value.Status;
        }

        public string StatusAsString { get; private set; }

        public virtual List<ParcelDetailAddress> Addresses { get; set; }

        public bool IsRemoved { get; set; }
        public string LastEventHash { get; set; } = string.Empty;
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; private set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        private ParcelDetail()
        {
            Addresses = new List<ParcelDetailAddress>();
        }

        public ParcelDetail(
            Guid parcelId,
            string caPaKey,
            ParcelStatus status,
            IEnumerable<ParcelDetailAddress> addresses,
            bool isRemoved,
            Instant versionTimeStamp)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            Status = status;
            Addresses = addresses.ToList();
            IsRemoved = isRemoved;
            VersionTimestamp = versionTimeStamp;
        }
    }

    public class ParcelDetailConfiguration : IEntityTypeConfiguration<ParcelDetail>
    {
        internal const string TableName = "ParcelDetails";

        public void Configure(EntityTypeBuilder<ParcelDetail> builder)
        {
            builder.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.ParcelId)
                .IsClustered(false);

            builder.Ignore(x => x.Status);

            builder.Property(p => p.CaPaKey);
            builder.Property(x => x.StatusAsString).HasMaxLength(450)
                .HasColumnName("Status");

            builder.Property(p => p.IsRemoved);
            builder.Property(x => x.LastEventHash);

            builder.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("VersionTimestamp");
            builder.Ignore(x => x.VersionTimestamp);

            builder.HasMany(x => x.Addresses).WithOne()
                .HasForeignKey(x => x.ParcelId);

            builder.HasIndex(x => x.CaPaKey).IsClustered();
            builder.HasAlternateKey(x => x.CaPaKey);

            builder.HasIndex(x => x.IsRemoved);
            builder.HasIndex(x => x.StatusAsString);
        }
    }

    public sealed class ParcelDetailAddress
    {
        private ParcelDetailAddress()
        { }

        public ParcelDetailAddress(Guid parcelId, int persistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = persistentLocalId;
            Count = 1;
        }

        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }
    }

    public sealed class ParcelDetailAddressV2Configuration : IEntityTypeConfiguration<ParcelDetailAddress>
    {
        private const string TableName = "ParcelDetailAddresses";

        public void Configure(EntityTypeBuilder<ParcelDetailAddress> b)
        {
            b.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered();

            b.HasIndex(x => x.AddressPersistentLocalId);
            b.Property(x => x.Count).HasDefaultValue(1);
        }
    }
}
