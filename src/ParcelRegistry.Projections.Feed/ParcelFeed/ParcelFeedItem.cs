namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; } = null!;

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private ParcelFeedItem() { }

        public ParcelFeedItem(long position, int page, Guid parcelId, string caPaKey) : this()
        {
            Page = page;
            Position = position;
            ParcelId = parcelId;
            CaPaKey = caPaKey;
        }
    }

    public class ParcelFeedConfiguration : IEntityTypeConfiguration<ParcelFeedItem>
    {
        private const string TableName = "ParcelFeed";

        public void Configure(EntityTypeBuilder<ParcelFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo("ParcelFeedSequence", Schema.Feed);

            b.Property(x => x.ParcelId)
                .IsRequired();

            b.Property(x => x.CaPaKey)
                .IsRequired();

            b.Property(x => x.CloudEventAsString)
                .HasColumnName("CloudEvent")
                .IsRequired();

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
            b.HasIndex(x => x.CaPaKey);
            b.HasIndex(x => x.ParcelId);
        }
    }
}
