namespace ParcelRegistry.Projections.Integration.ParcelVersion
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public sealed class ParcelVersionAddress
    {
        public long Position { get; set; }
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public string CaPaKey { get; set; }
        public int Count { get; set; }

        public ParcelVersionAddress(
            long position,
            Guid parcelId,
            int addressPersistentLocalId,
            string caPaKey)
        {
            Position = position;
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            CaPaKey = caPaKey;
            Count = 1;
        }

        public ParcelVersionAddress CloneAndApplyEventInfo(long newPosition)
        {
            var newItem = new ParcelVersionAddress
            {
                Position = newPosition,
                ParcelId = ParcelId,
                AddressPersistentLocalId = AddressPersistentLocalId,
                CaPaKey = CaPaKey,
                Count = Count
            };

            return newItem;
        }

        // Needed for EF
        protected ParcelVersionAddress()
        { }
    }

    public sealed class ParcelVersionAddressConfiguration : IEntityTypeConfiguration<ParcelVersionAddress>
    {
        public void Configure(EntityTypeBuilder<ParcelVersionAddress> builder)
        {
            builder.ToTable("parcel_version_addresses", Schema.Integration)
                .HasKey(e => new { e.Position, e.ParcelId, e.AddressPersistentLocalId });

            builder.Property(parcel => parcel.Position).ValueGeneratedNever();
            builder.Property(parcel => parcel.Position).HasColumnName("position");

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
                .HasColumnName("count")
                .HasDefaultValue(1)
                .IsRequired();

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.ParcelId);
            builder.HasIndex(x => x.AddressPersistentLocalId);
            builder.HasIndex(x => x.CaPaKey);
        }
    }
}
