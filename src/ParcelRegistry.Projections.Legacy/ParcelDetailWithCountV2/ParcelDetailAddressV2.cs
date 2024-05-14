namespace ParcelRegistry.Projections.Legacy.ParcelDetailWithCountV2
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelDetailAddressV2
    {
        private ParcelDetailAddressV2()
        { }

        public ParcelDetailAddressV2(Guid parcelId, int persistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = persistentLocalId;
            Count = 1;
        }

        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }
    }

    public class ParcelDetailAddressV2Configuration : IEntityTypeConfiguration<ParcelDetailAddressV2>
    {
        private const string TableName = "ParcelAddressesWithCountV2";

        public void Configure(EntityTypeBuilder<ParcelDetailAddressV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered();

            b.HasIndex(x => x.AddressPersistentLocalId);
            b.Property(x => x.Count).HasDefaultValue(1);
        }
    }
}
