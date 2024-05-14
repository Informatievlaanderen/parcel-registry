namespace ParcelRegistry.Projections.Integration.ParcelLatestItem
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public sealed class ParcelLatestItemAddress
    {
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public string CaPaKey { get; set; }
        public int Count { get; set; }

        public ParcelLatestItemAddress(Guid parcelId, int addressPersistentLocalId, string caPaKey)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            CaPaKey = caPaKey;
            Count = 1;
        }

        // Needed for EF
        protected ParcelLatestItemAddress()
        { }
    }

    public sealed class ParcelLatestItemAddressConfiguration : IEntityTypeConfiguration<ParcelLatestItemAddress>
    {
        public void Configure(EntityTypeBuilder<ParcelLatestItemAddress> builder)
        {
            builder
                .ToTable("parcel_latest_item_addresses", Schema.Integration)
                .HasKey(e => new { e.ParcelId, e.AddressPersistentLocalId });

            builder.Property(e => e.ParcelId)
                .HasColumnName("parcel_id")
                .IsRequired();

            builder.Property(e => e.AddressPersistentLocalId)
                .HasColumnName("address_persistent_local_id")
                .IsRequired();

            builder.Property(e => e.CaPaKey)
                .HasColumnName("capakey")
                .IsRequired();

            builder.Property(e => e.Count)
                .HasDefaultValue(1);

            builder.HasIndex(x => x.ParcelId);
            builder.HasIndex(x => x.AddressPersistentLocalId);
            builder.HasIndex(x => x.CaPaKey);
        }
    }
}
