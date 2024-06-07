namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public class ParcelDetailAddress
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

    public class ParcelDetailAddressV2Configuration : IEntityTypeConfiguration<ParcelDetailAddress>
    {
        private const string TableName = "ParcelDetailAddresses";

        public void Configure(EntityTypeBuilder<ParcelDetailAddress> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered();

            b.HasIndex(x => x.AddressPersistentLocalId);
            b.Property(x => x.Count).HasDefaultValue(1);
        }
    }
}
