namespace ParcelRegistry.Importer.Grb
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public sealed class RunHistory
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

    public sealed class RunHistoryConfiguration : IEntityTypeConfiguration<RunHistory>
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
}
