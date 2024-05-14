namespace ParcelRegistry.Projections.Extract.ParcelLinkExtractWithCount
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class ParcelLinkExtractItem
    {
        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public sealed class ParcelLinkExtractItemConfiguration : IEntityTypeConfiguration<ParcelLinkExtractItem>
    {
        private const string TableName = "ParcelLinksWithCount";

        public void Configure(EntityTypeBuilder<ParcelLinkExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered(false);

            builder.Property(p => p.CaPaKey);
            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.AddressPersistentLocalId);
            builder.Property(p => p.Count).HasDefaultValue(1);

            builder.HasIndex(p => p.CaPaKey).IsClustered();
            builder.HasIndex(p => p.AddressPersistentLocalId);
            builder.HasIndex(p => p.ParcelId);
        }
    }
}
