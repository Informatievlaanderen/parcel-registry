namespace ParcelRegistry.Consumer.Address
{
    using System;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.EntityFrameworkCore.EntityTypeConfiguration;
    using Microsoft.EntityFrameworkCore;

    public abstract class ConsumerDbContext<TContext> : DbContext where TContext : DbContext
    {
        public const string ProcessedMessageTable = "ProcessedMessages";

        public abstract string ProcessedMessagesSchema { get; }
        public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

        protected ConsumerDbContext()
        { }

        protected ConsumerDbContext(DbContextOptions<TContext> dbContextOptions)
            : base(dbContextOptions)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
                OnConfiguringOptionsBuilder(optionsBuilder);
        }

        protected virtual void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<ProcessedMessage>()
                .ToTable(ProcessedMessageTable, ProcessedMessagesSchema)
                .HasKey(p => p.IdempotenceKey)
                .IsClustered();

            modelBuilder
                .Entity<ProcessedMessage>()
                .Property(p => p.IdempotenceKey)
                .HasMaxLength(128); //SHA512 length

            modelBuilder
                .Entity<ProcessedMessage>()
                .Property(p => p.DateProcessed);

            modelBuilder.AddEntityConfigurationsFromAssembly(typeof(TContext).GetTypeInfo().Assembly);
        }
    }

    public class ProcessedMessage
    {
        public string IdempotenceKey { get; set; }
        public DateTimeOffset DateProcessed { get; set; }

        public ProcessedMessage(
            string idempotenceKey,
            DateTimeOffset dateProcessed)
        {
            IdempotenceKey = idempotenceKey;
            DateProcessed = dateProcessed;
        }
    }
}
