namespace ParcelRegistry.Projections.Integration.ParcelVersion
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using ParcelRegistry.Infrastructure;

    public sealed class ParcelVersionAddress
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public long Position { get; set; }
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public string CaPaKey { get; set; }

        public string VersionAsString { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public ParcelVersionAddress(long position, Guid parcelId, int addressPersistentLocalId, string caPaKey, Instant lastChangedOn)
        {
            Position = position;
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            CaPaKey = caPaKey;
            VersionTimestamp = lastChangedOn;
        }

        public ParcelVersionAddress CloneAndApplyEventInfo(
            long newPosition,
            Instant lastChangedOn)
        {
            var newItem = new ParcelVersionAddress()
            {
                Position = newPosition,
                ParcelId = ParcelId,
                AddressPersistentLocalId = AddressPersistentLocalId,
                CaPaKey = CaPaKey,
                VersionTimestamp = lastChangedOn
            };

            return newItem;
        }

        // Needed for EF
        protected ParcelVersionAddress()
        { }
    }

    public sealed class ParcelVersionAddressConfiguration : IEntityTypeConfiguration<ParcelVersionAddress>
    {
        public void Configure(EntityTypeBuilder<ParcelVersionAddress> builder)
        {
            builder.ToTable("parcel_version_addresses", Schema.Integration)
                .HasKey(e => new { e.Position, e.ParcelId, e.AddressPersistentLocalId });

            builder.Property(parcel => parcel.Position).ValueGeneratedNever();
            builder.Property(parcel => parcel.Position).HasColumnName("position");


            builder.Property(e => e.ParcelId)
                .HasColumnName("parcel_id")
                .IsRequired();

            builder.Property(e => e.AddressPersistentLocalId)
                .HasColumnName("address_persistent_local_id")
                .IsRequired();

            builder.Property(e => e.CaPaKey)
                .HasColumnName("capakey")
                .IsRequired();

            builder.Property(parcel => parcel.VersionAsString).HasColumnName("version_as_string");

            builder.Property(ParcelVersionAddress.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");

            builder.Ignore(parcel => parcel.VersionTimestamp);


            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.ParcelId);
            builder.HasIndex(x => x.AddressPersistentLocalId);
            builder.HasIndex(x => x.CaPaKey);
            builder.HasIndex(ParcelVersionAddress.VersionTimestampBackingPropertyName);
        }
    }
}
