namespace ParcelRegistry.Projections.Legacy.ParcelDetail
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelDetailAddress
    {
        public Guid ParcelId { get; set; }
        public Guid AddressId { get; set; }
    }

    public class ParcelDetailAddressmConfiguration : IEntityTypeConfiguration<ParcelDetailAddress>
    {
        public const string TableName = "ParcelAddresses";

        public void Configure(EntityTypeBuilder<ParcelDetailAddress> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.ParcelId, p.AddressId })
                .ForSqlServerIsClustered();
        }
    }
}
