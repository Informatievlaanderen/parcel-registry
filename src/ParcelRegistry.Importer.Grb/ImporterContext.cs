namespace ParcelRegistry.Importer.Grb
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public class ImporterContext : DbContext
    {

        public DbSet<RunHistory> RunHistory { get; set; }
        public DbSet<ProcessedRequests> ProcessedRequests { get; set; }
    }

    public class RunHistory
    {
        public int Id { get; }
        public DateTimeOffset FromDate { get; }
        public DateTimeOffset ToDate { get; }
        public bool Completed { get; set; }

        public RunHistory()
        { }

        public RunHistory(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
        }

        public void SetComplete() => Completed = true;
    }

    public class RunHistoryConfiguration : IEntityTypeConfiguration<RunHistory>
    {
        public const string TableName = "RunHistory";

        public void Configure(EntityTypeBuilder<RunHistory> builder)
        {
            builder.ToTable(TableName, Schema.GrbImporter)
                .HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.FromDate);
            builder.Property(x => x.ToDate);
            builder.Property(x => x.Completed);
        }
    }

    public class ProcessedRequests
    {
        public string SHA256 { get; set; }

        public ProcessedRequests()
        { }
    }

    public class ProcessedRequestsConfiguration : IEntityTypeConfiguration<ProcessedRequests>
    {
        public const string TableName = "ProcessedRequests";

        public void Configure(EntityTypeBuilder<ProcessedRequests> builder)
        {
            builder.ToTable(TableName, Schema.GrbImporter)
                .HasKey(x => x.SHA256);

            builder.Property(x => x.SHA256);

            builder.HasIndex(x => x.SHA256).IsClustered();
        }
    }
}
