namespace ParcelRegistry.Importer.Grb
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public class ProcessedRequests
    {
        public string SHA256 { get; set; }

        public ProcessedRequests()
        { }

        public ProcessedRequests(string sha256)
        {
            SHA256 = sha256;
        }
    }

    public class ProcessedRequestsConfiguration : IEntityTypeConfiguration<ProcessedRequests>
    {
        public const string TableName = "ProcessedRequests";

        public void Configure(EntityTypeBuilder<ProcessedRequests> builder)
        {
            builder.ToTable(TableName, Schema.GrbImporter)
                .HasKey(x => x.SHA256);

            builder.Property(x => x.SHA256)
                .HasMaxLength(64);

            builder.HasIndex(x => x.SHA256).IsClustered();
        }
    }
}
