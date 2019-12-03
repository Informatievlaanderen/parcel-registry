namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using System;
    using System.Collections.ObjectModel;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class ParcelDetail
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public static string StatusBackingPropertyName = nameof(StatusAsString);

        public Guid ParcelId { get; set; }
        public string PersistentLocalId { get; set; }

        public ParcelStatus? Status
        {
            get => ParcelStatus.Parse(StatusAsString);
            set => StatusAsString = value?.Status;
        }

        private string StatusAsString { get; set; }

        public virtual Collection<ParcelDetailAddress> Addresses { get; set; }

        public bool Complete { get; set; }
        public bool Removed { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public ParcelDetail()
        {
            Addresses = new Collection<ParcelDetailAddress>();
        }
    }

    public class ParcelDetailConfiguration : IEntityTypeConfiguration<ParcelDetail>
    {
        internal const string TableName = "ParcelDetails";

        public void Configure(EntityTypeBuilder<ParcelDetail> builder)
        {
            builder.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.ParcelId)
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.PersistentLocalId);
            builder.Property(ParcelDetail.StatusBackingPropertyName)
                .HasColumnName("Status");

            builder.Property(p => p.Complete);
            builder.Property(p => p.Removed);
            builder.Property(ParcelDetail.VersionTimestampBackingPropertyName)
                .HasColumnName("VersionTimestamp");

            builder.Ignore(x => x.Status);
            builder.Ignore(x => x.VersionTimestamp);

            builder.HasMany(x => x.Addresses);

            builder.HasIndex(x => x.PersistentLocalId).ForSqlServerIsClustered();
            builder.HasIndex(x => x.Removed);
            builder.HasIndex(x => x.Complete);
        }
    }
}
