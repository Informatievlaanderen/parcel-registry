namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Crab;
    using Newtonsoft.Json;

    [EventName("ParcelSnapshot")]
    [EventSnapshot(nameof(SnapshotContainer) + "<ParcelSnapshot>", typeof(SnapshotContainer))]
    [EventDescription("Snapshot of Parcel")]
    public class ParcelSnapshot
    {
        public Guid ParcelId { get; }
        public string? ParcelStatus { get; }
        public bool IsRemoved { get; }
        public Modification LastModificationBasedOnCrab { get; }
        public Dictionary<int, int> ActiveHouseNumberIdsByTerrainObjectHouseNr { get; }
        public IEnumerable<AddressSubaddressWasImportedFromCrab> ImportedSubaddressFromCrab { get; }
        public IEnumerable<Guid> AddressIds { get; }

        public ParcelSnapshot(ParcelId parcelId,
            ParcelStatus? parcelStatus,
            bool isRemoved,
            Modification lastModificationBasedOnCrab,
            Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> activeHouseNumberIdsByTerrainObjectHouseNr,
            IEnumerable<AddressSubaddressWasImportedFromCrab> importedSubaddressFromCrab,
            IEnumerable<AddressId> addressIds)
        {
            ParcelId = parcelId;
            ParcelStatus = parcelStatus ?? string.Empty;
            IsRemoved = isRemoved;
            LastModificationBasedOnCrab = lastModificationBasedOnCrab;
            ActiveHouseNumberIdsByTerrainObjectHouseNr = activeHouseNumberIdsByTerrainObjectHouseNr
                .ToDictionary(x => (int)x.Key, y=>(int)y.Value);
            ImportedSubaddressFromCrab = importedSubaddressFromCrab;
            AddressIds = addressIds.Select(id => (Guid)id);
        }

        [JsonConstructor]
        private ParcelSnapshot(
            Guid parcelId,
            string parcelStatus,
            bool isRemoved,
            Modification lastModificationBasedOnCrab,
            Dictionary<int,int> activeHouseNumberIdsByTerrainObjectHouseNr,
            IEnumerable<AddressSubaddressWasImportedFromCrab> importedSubaddressFromCrab,
            IEnumerable<Guid> addressIds)
            : this(
                new ParcelId(parcelId),
                string.IsNullOrEmpty(parcelStatus) ? null : ParcelRegistry.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                lastModificationBasedOnCrab,
                activeHouseNumberIdsByTerrainObjectHouseNr.ToDictionary(x => new CrabTerrainObjectHouseNumberId(x.Key), y => new CrabHouseNumberId(y.Value)),
                importedSubaddressFromCrab,
                addressIds.Select(id => new AddressId(id)))
        { }
    }
}
