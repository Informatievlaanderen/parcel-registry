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
    [EventSnapshot(nameof(SnapshotContainer) + "<ParcelSnapshotV2>", typeof(SnapshotContainer))]
    [EventDescription("Snapshot of Parcel")]
    public class ParcelSnapshotV2
    {
        public Guid ParcelId { get; }
        public int ParcelPersistentLocalId { get; }
        public string ParcelStatus { get; }
        public bool IsRemoved { get; }
        public IEnumerable<int> AddressPersistentLocalIds { get; }
        public string LastEventHash { get; }
        public ProvenanceData LastProvenanceData { get; }

        public ParcelSnapshotV2(
            ParcelId parcelId,
            ParcelPersistentLocalId parcelPersistentLocalId,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> persistentLocalIds,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
        {
            ParcelId = parcelId;
            ParcelPersistentLocalId = parcelPersistentLocalId;
            ParcelStatus = parcelStatus;
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = persistentLocalIds.Select(x => (int)x).ToList();
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
        }

        [JsonConstructor]
        private ParcelSnapshotV2(
            Guid parcelId,
            int parcelPersistentLocalId,
            string parcelStatus,
            bool isRemoved,
            IEnumerable<int> addressPersistentLocalIds,
            string lastEventHash,
            ProvenanceData lastProvenanceData)
            : this(
                new ParcelId(parcelId),
                new ParcelPersistentLocalId(parcelPersistentLocalId),
                ParcelRegistry.Parcel.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                addressPersistentLocalIds.Select(id => new AddressPersistentLocalId(id)),
                lastEventHash,
                lastProvenanceData)
        { }
    }
}
