namespace ParcelRegistry.Projections.Legacy.ParcelSyndication
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Newtonsoft.Json;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using ParcelRegistry.Legacy;

    public class ParcelSyndicationItem
    {
        public long Position { get; set; }
        public Guid? ParcelId { get; set; }
        public string? CaPaKey { get; set; }
        public string? ChangeType { get; set; }
        public string? StatusAsString { get; set; }
        public byte[]? ExtendedWkbGeometry { get; set; }

        public ParcelStatus? Status
        {
            get => string.IsNullOrEmpty(StatusAsString) ? null : (ParcelStatus?)ParcelStatus.Parse(StatusAsString);
            set => StatusAsString = value.HasValue ? value.Value.Status : string.Empty;
        }

        public string? AddressesAsString { get; set; }

        public IReadOnlyCollection<Guid> AddressIds
        {
            get => GetDeserializedAddressIds();
            set => AddressesAsString = JsonConvert.SerializeObject(value);
        }

        public string? AddressPersistentLocalIdsAsString { get; set; }

        public IReadOnlyCollection<int> AddressPersistentLocalIds
        {
            get => GetDeserializedAddressPersistentLocalIds();
            set => AddressPersistentLocalIdsAsString = JsonConvert.SerializeObject(value);
        }

        public DateTimeOffset RecordCreatedAtAsDateTimeOffset { get; set; }
        public DateTimeOffset LastChangedOnAsDateTimeOffset { get; set; }

        public Instant RecordCreatedAt
        {
            get => Instant.FromDateTimeOffset(RecordCreatedAtAsDateTimeOffset);
            set => RecordCreatedAtAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Instant LastChangedOn
        {
            get => Instant.FromDateTimeOffset(LastChangedOnAsDateTimeOffset);
            set => LastChangedOnAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string? EventDataAsXml { get; set; }
        public DateTimeOffset SyndicationItemCreatedAt { get; set; }

        private List<Guid> GetDeserializedAddressIds()
        {
            return string.IsNullOrEmpty(AddressesAsString)
                ? new List<Guid>()
                : JsonConvert.DeserializeObject<List<Guid>>(AddressesAsString)!;
        }

        private List<int> GetDeserializedAddressPersistentLocalIds()
        {
            return string.IsNullOrEmpty(AddressPersistentLocalIdsAsString)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(AddressPersistentLocalIdsAsString)!;
        }

        public void AddAddressId(Guid addressId)
        {
            var addresses = GetDeserializedAddressIds();
            addresses.Add(addressId);
            AddressIds = addresses;
        }

        public void AddAddressPersistentLocalId(int addressPersistentLocalId)
        {
            var addressPersistentLocalIds = GetDeserializedAddressPersistentLocalIds();
            if (!addressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                addressPersistentLocalIds.Add(addressPersistentLocalId);
                AddressPersistentLocalIds = addressPersistentLocalIds;
            }
        }

        public void RemoveAllAddressPersistentLocalId(int addressPersistentLocalId)
        {
            var addressPersistentLocalIds = GetDeserializedAddressPersistentLocalIds();
            addressPersistentLocalIds.RemoveAll(x => x == addressPersistentLocalId);
            AddressPersistentLocalIds = addressPersistentLocalIds;
        }

        public void RemoveAddressPersistentLocalId(int addressPersistentLocalId)
        {
            var addressPersistentLocalIds = GetDeserializedAddressPersistentLocalIds();
            if (addressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                addressPersistentLocalIds.Remove(addressPersistentLocalId);
                AddressPersistentLocalIds = addressPersistentLocalIds;
            }
        }

        public void RemoveAddressId(Guid addressId)
        {
            var addresses = GetDeserializedAddressIds();
            addresses.Remove(addressId);
            AddressIds = addresses;
        }

        public ParcelSyndicationItem CloneAndApplyEventInfo(
            long position,
            string changeType,
            Instant lastChangedOn,
            Action<ParcelSyndicationItem> editFunc)
        {
            var newItem = new ParcelSyndicationItem
            {
                ChangeType = changeType,
                Position = position,
                LastChangedOn = lastChangedOn,

                ParcelId = ParcelId,
                CaPaKey = CaPaKey,
                AddressIds = AddressIds,
                AddressPersistentLocalIds = AddressPersistentLocalIds,
                Status = Status,
                ExtendedWkbGeometry = ExtendedWkbGeometry,

                RecordCreatedAt = RecordCreatedAt,
                Application = Application,
                Modification = Modification,
                Operator = Operator,
                Organisation = Organisation,
                Reason = Reason,
                SyndicationItemCreatedAt = DateTimeOffset.Now,
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public class ParcelSyndicationConfiguration : IEntityTypeConfiguration<ParcelSyndicationItem>
    {
        private const string TableName = "ParcelSyndication";

        public void Configure(EntityTypeBuilder<ParcelSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(x => x.Position)
                .IsClustered();

            b.Property(x => x.Position).ValueGeneratedNever();
            b.HasIndex(x => x.Position).IsColumnStore($"CI_{TableName}_Position");

            b.Property(x => x.ParcelId).IsRequired();
            b.Property(x => x.ChangeType);

            b.Property(x => x.CaPaKey);

            b.Ignore(x => x.Status);
            b.Property(x => x.StatusAsString)
                .HasColumnName("Status");

            b.Property(x => x.ExtendedWkbGeometry);

            b.Ignore(x => x.AddressIds);
            b.Property(x => x.AddressesAsString)
                .HasColumnName("AddressIds");

            b.Ignore(x => x.AddressPersistentLocalIds);
            b.Property(x => x.AddressPersistentLocalIdsAsString)
                .HasColumnName("AddressPersistentLocalIds");

            b.Property(x => x.RecordCreatedAtAsDateTimeOffset).HasColumnName("RecordCreatedAt");
            b.Property(x => x.LastChangedOnAsDateTimeOffset).HasColumnName("LastChangedOn");

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);
            b.Property(x => x.EventDataAsXml);
            b.Property(x => x.SyndicationItemCreatedAt).IsRequired();

            b.Ignore(x => x.RecordCreatedAt);
            b.Ignore(x => x.LastChangedOn);

            b.HasIndex(x => x.ParcelId);
        }
    }
}
