namespace ParcelRegistry.Legacy.Events
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
        public string CaPaKey { get; }
        public string? ParcelStatus { get; }
        public bool IsRemoved { get; }
        public Modification LastModificationBasedOnCrab { get; }
        public Dictionary<int, int> ActiveHouseNumberIdsByTerrainObjectHouseNr { get; }
        public IEnumerable<AddressSubaddressWasImportedFromCrab> ImportedSubaddressFromCrab { get; }
        public IEnumerable<Guid> AddressIds { get; }
        public decimal? XCoordinate { get; }
        public decimal? YCoordinate { get; }

        public ParcelSnapshot(
            ParcelId parcelId,
            VbrCaPaKey caPaKey,
            ParcelStatus? parcelStatus,
            bool isRemoved,
            Modification lastModificationBasedOnCrab,
            Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> activeHouseNumberIdsByTerrainObjectHouseNr,
            IEnumerable<AddressSubaddressWasImportedFromCrab> importedSubaddressFromCrab,
            IEnumerable<AddressId> addressIds,
            CrabCoordinate? xCoordinate,
            CrabCoordinate? yCoordinate)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            ParcelStatus = parcelStatus ?? string.Empty;
            IsRemoved = isRemoved;
            LastModificationBasedOnCrab = lastModificationBasedOnCrab;
            ActiveHouseNumberIdsByTerrainObjectHouseNr = activeHouseNumberIdsByTerrainObjectHouseNr
                .ToDictionary(x => (int)x.Key, y => (int)y.Value);
            ImportedSubaddressFromCrab = importedSubaddressFromCrab;
            AddressIds = addressIds.Select(id => (Guid)id);
            XCoordinate = xCoordinate ?? (decimal?)null;
            YCoordinate = yCoordinate ?? (decimal?)null;
        }

        [JsonConstructor]
        private ParcelSnapshot(
            Guid parcelId,
            string vbrCaPaKey,
            string parcelStatus,
            bool isRemoved,
            Modification lastModificationBasedOnCrab,
            Dictionary<int,int> activeHouseNumberIdsByTerrainObjectHouseNr,
            IEnumerable<AddressSubaddressWasImportedFromCrab> importedSubaddressFromCrab,
            IEnumerable<Guid> addressIds,
            decimal? xCoordinate,
            decimal? yCoordinate)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(vbrCaPaKey),
                string.IsNullOrEmpty(parcelStatus) ? null : Legacy.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                lastModificationBasedOnCrab,
                activeHouseNumberIdsByTerrainObjectHouseNr.ToDictionary(x => new CrabTerrainObjectHouseNumberId(x.Key), y => new CrabHouseNumberId(y.Value)),
                importedSubaddressFromCrab,
                addressIds.Select(id => new AddressId(id)),
                xCoordinate.HasValue ? new CrabCoordinate(xCoordinate.Value) : null,
                yCoordinate.HasValue ? new CrabCoordinate(yCoordinate.Value) : null)
        { }
    }
}
