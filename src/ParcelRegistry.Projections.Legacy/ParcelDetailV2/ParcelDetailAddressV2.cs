namespace ParcelRegistry.Projections.Legacy.ParcelDetailV2
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
        }

        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
    }

    public class ParcelDetailAddressV2Configuration : IEntityTypeConfiguration<ParcelDetailAddressV2>
    {
        private const string TableName = "ParcelAddressesV2";

        public void Configure(EntityTypeBuilder<ParcelDetailAddressV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered();

            b.HasIndex(x => x.AddressPersistentLocalId);
        }
    }
}
