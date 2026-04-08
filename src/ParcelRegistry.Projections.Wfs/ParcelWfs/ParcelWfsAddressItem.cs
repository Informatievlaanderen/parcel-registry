namespace ParcelRegistry.Projections.Wfs.ParcelWfs
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelWfsAddressItem
    {
        private ParcelWfsAddressItem()
        { }

        public ParcelWfsAddressItem(Guid parcelId, int addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
        }

        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
    }

    public class ParcelWfsAddressItemConfiguration : IEntityTypeConfiguration<ParcelWfsAddressItem>
    {
        internal const string TableName = "ParcelWfsAddresses";

        public void Configure(EntityTypeBuilder<ParcelWfsAddressItem> builder)
        {
            builder.ToTable(TableName, Schema.Wfs)
                .HasKey(p => new { p.ParcelId, p.AddressPersistentLocalId })
                .IsClustered();

            builder.HasIndex(x => x.AddressPersistentLocalId);
            builder.HasIndex(x => x.ParcelId);
        }
    }
}
