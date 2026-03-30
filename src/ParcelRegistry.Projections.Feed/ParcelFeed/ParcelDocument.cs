namespace ParcelRegistry.Projections.Feed.ParcelFeed
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;

    public sealed class ParcelDocument
    {
        public string CaPaKey { get; set; } = null!;
        public bool IsRemoved { get; set; }
        public ParcelDocumentContent Document { get; set; }

        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }
        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToBelgianDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set
            {
                var belgianDateTimeOffset = value.ToBelgianDateTimeOffset();
                LastChangedOnAsDateTimeOffset = belgianDateTimeOffset;
                Document.VersionId = belgianDateTimeOffset;
            }
        }

        private ParcelDocument()
        {
            Document = new ParcelDocumentContent();
            IsRemoved = false;
        }

        public ParcelDocument(
            string caPaKey,
            string status,
            string gml,
            string gmlType,
            string extendedWkbGeometry,
            List<int> addressPersistentLocalIds,
            bool isRemoved,
            Instant createdTimestamp)
        {
            CaPaKey = caPaKey;
            IsRemoved = isRemoved;

            Document = new ParcelDocumentContent
            {
                CaPaKey = caPaKey,
                Status = status,
                GeometryAsGml = gml,
                GeometryGmlType = gmlType,
                ExtendedWkbGeometry = extendedWkbGeometry,
                AddressPersistentLocalIds = addressPersistentLocalIds,
            };

            RecordCreatedAt = createdTimestamp;
            LastChangedOn = createdTimestamp;
        }
    }

    public sealed class ParcelDocumentContent
    {
        public string CaPaKey { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string GeometryAsGml { get; set; } = string.Empty;
        public string GeometryGmlType { get; set; } = string.Empty;
        public string ExtendedWkbGeometry { get; set; } = string.Empty;
        public List<int> AddressPersistentLocalIds { get; set; } = new();

        public DateTimeOffset VersionId { get; set; }
    }

    public sealed class ParcelDocumentConfiguration : IEntityTypeConfiguration<ParcelDocument>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public ParcelDocumentConfiguration(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void Configure(EntityTypeBuilder<ParcelDocument> b)
        {
            b.ToTable("ParcelDocuments", Schema.Feed)
                .HasKey(x => x.CaPaKey)
                .IsClustered();

            b.Property(x => x.CaPaKey)
                .ValueGeneratedNever();

            b.Property(x => x.LastChangedOnAsDateTimeOffset)
                .HasColumnName("LastChangedOn");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset)
                .HasColumnName("RecordCreatedAt");

            b.Property(x => x.Document)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _serializerSettings),
                    v => JsonConvert.DeserializeObject<ParcelDocumentContent>(v, _serializerSettings)!);

            b.Ignore(x => x.LastChangedOn);
            b.Ignore(x => x.RecordCreatedAt);
        }
    }
}
