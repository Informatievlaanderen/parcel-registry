namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelExtractItem
    {
        public Guid? ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public class ParcelExtractItemConfiguration : IEntityTypeConfiguration<ParcelExtractItem>
    {
        private const string TableName = "Parcel";

        public void Configure(EntityTypeBuilder<ParcelExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.ParcelId)
                .ForSqlServerIsClustered(false);

            builder.Property(p => p.CaPaKey);
            builder.Property(p => p.DbaseRecord);

            builder.HasIndex(p => p.CaPaKey).ForSqlServerIsClustered();
        }
    }
}
