namespace ParcelRegistry.Projections.Syndication.Address
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class AddressOlsoIdItem
    {
        public Guid AddressId { get; set; }
        public string OsloId { get; set; }
        public DateTimeOffset? Version { get; set; }
        public long Position { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
    }

    public class AddressOsloIdItemConfiguration : IEntityTypeConfiguration<AddressOlsoIdItem>
    {
        private const string TableName = "AddressOsloIdSyndication";

        public void Configure(EntityTypeBuilder<AddressOlsoIdItem> builder)
        {
            builder.ToTable(TableName, Schema.Syndication)
                .HasKey(x => x.AddressId)
                .ForSqlServerIsClustered(false);

            builder.Property(x => x.OsloId);

            builder.Property(x => x.Version);
            builder.Property(x => x.Position);
            builder.Property(x => x.IsComplete);
            builder.Property(x => x.IsRemoved);

            builder.HasIndex(x => new { x.IsComplete, x.IsRemoved });
        }
    }
}
