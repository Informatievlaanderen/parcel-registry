namespace ParcelRegistry.Projections.Extract.ParcelExtract
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelExtractItemV2
    {
        public Guid? ParcelId { get; set; }
        public string? CaPaKey { get; set; }
        public byte[]? DbaseRecord { get; set; }
    }

    public class ParcelExtractItemV2Configuration : IEntityTypeConfiguration<ParcelExtractItemV2>
    {
        private const string TableName = "ParcelV2";

        public void Configure(EntityTypeBuilder<ParcelExtractItemV2> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => p.ParcelId)
                .IsClustered(false);

            builder.Property(p => p.CaPaKey);
            builder.Property(p => p.DbaseRecord);

            builder.HasIndex(p => p.CaPaKey).IsClustered();
        }
    }
}
