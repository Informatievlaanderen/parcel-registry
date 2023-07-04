namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NetTopologySuite.Geometries;
    using Newtonsoft.Json;
    using Coordinate = ParcelRegistry.Parcel.Coordinate;

    [EventName("ParcelSnapshotV2")]
    [EventSnapshot(nameof(SnapshotContainer) + "<ParcelSnapshotV2>", typeof(SnapshotContainerV2))]
    [EventDescription("Snapshot of Parcel")]
    public class ParcelSnapshotV2
    {
        public Guid ParcelId { get; }
        public string CaPaKey { get; }
        public string ParcelStatus { get; }
        public bool IsRemoved { get; }
        public IEnumerable<int> AddressPersistentLocalIds { get; }
        public string ExtendedWkbGeometry { get; }
        public string LastEventHash { get; }
        public ProvenanceData LastProvenanceData { get; }

        public ParcelSnapshotV2(
            ParcelId parcelId,
            VbrCaPaKey caPaKey,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            string extendedWkbGeometry,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            ParcelStatus = parcelStatus;
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
            ExtendedWkbGeometry = extendedWkbGeometry;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }

        [JsonConstructor]
        private ParcelSnapshotV2(
            Guid parcelId,
            string caPaKey,
            string parcelStatus,
            bool isRemoved,
            IEnumerable<int> addressPersistentLocalIds,
            string extendedWkbGeometry,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
            : this(
                new ParcelId(parcelId),
                new VbrCaPaKey(caPaKey),
                ParcelRegistry.Parcel.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                addressPersistentLocalIds.Select(id => new AddressPersistentLocalId(id)),
                extendedWkbGeometry,
                lastEventHash,
                lastProvenanceData)
        { }
    }
}
