namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("ParcelSnapshotV2")]
    [EventSnapshot(nameof(SnapshotContainer) + "<ParcelSnapshotV2>", typeof(SnapshotContainerV2))]
    [EventDescription("Snapshot of Parcel")]
    public class ParcelSnapshotV2
    {
        public Guid ParcelId { get; }
        public string ParcelStatus { get; }
        public bool IsRemoved { get; }
        public IEnumerable<int> AddressPersistentLocalIds { get; }
        public decimal? XCoordinate { get; }
        public decimal? YCoordinate { get; }

        public string LastEventHash { get; }
        public ProvenanceData LastProvenanceData { get; }

        public ParcelSnapshotV2(
            ParcelId parcelId,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            Coordinate? xCoordinate,
            Coordinate? yCoordinate,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            ParcelId = parcelId;
            ParcelStatus = parcelStatus;
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
            XCoordinate = xCoordinate ?? (decimal?)null;
            YCoordinate = yCoordinate ?? (decimal?) null;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }

        [JsonConstructor]
        private ParcelSnapshotV2(
            Guid parcelId,
            string parcelStatus,
            bool isRemoved,
            IEnumerable<int> addressPersistentLocalIds,
            decimal? xCoordinate,
            decimal? yCoordinate,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
            : this(
                new ParcelId(parcelId),
                ParcelRegistry.Parcel.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                addressPersistentLocalIds.Select(id => new AddressPersistentLocalId(id)),
                xCoordinate.HasValue ? new Coordinate(xCoordinate.Value) : null,
                yCoordinate.HasValue ? new Coordinate(yCoordinate.Value) : null,
                lastEventHash,
                lastProvenanceData)
        { }
    }
}
