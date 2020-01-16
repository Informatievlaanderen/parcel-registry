namespace ParcelRegistry.Projections.Syndication.Address
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class AddressPersistentLocalIdItem
    {
        public Guid AddressId { get; set; }
        public string? PersistentLocalId { get; set; }
        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class AddressPersistentLocalIdItemConfiguration : IEntityTypeConfiguration<AddressPersistentLocalIdItem>
    {
        private const string TableName = "AddressPersistentLocalIdSyndication";

        public void Configure(EntityTypeBuilder<AddressPersistentLocalIdItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.AddressId)
                .IsClustered(false);

            builder.Property(x => x.PersistentLocalId);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.IsComplete);
            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => new { x.IsComplete, x.IsRemoved });
        }
    }
}
