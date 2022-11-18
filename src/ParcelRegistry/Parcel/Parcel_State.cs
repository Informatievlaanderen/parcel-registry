namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class Parcel
    {
        private IParcelEvent? _lastEvent;

        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;

        private readonly List<AddressPersistentLocalId> _addressPersistentLocalIds = new();

        public ParcelId ParcelId { get; private set; }
        public ParcelStatus ParcelStatus { get; private set; }
        public IReadOnlyList<AddressPersistentLocalId> AddressPersistentLocalIds => _addressPersistentLocalIds;
        public bool IsRemoved { get; private set; }

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        internal Parcel(ISnapshotStrategy snapshotStrategy) : this()
        {
            Strategy = snapshotStrategy;
        }

        private Parcel()
        {
            Register<ParcelWasMigrated>(When);
            Register<ParcelSnapshotV2>(When);
        }

        private void When(ParcelWasMigrated @event)
        {
            ParcelId = new ParcelId(@event.ParcelId);
            ParcelStatus = ParcelStatus.Parse(@event.ParcelStatus);
            IsRemoved = @event.IsRemoved;

            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
            }

            _lastEvent = @event;
        }

        private void When(ParcelSnapshotV2 @event)
        {
            ParcelId = new ParcelId(@event.ParcelId);
            ParcelStatus = ParcelStatus.Parse(@event.ParcelStatus);
            IsRemoved = @event.IsRemoved;

            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
            }

            _lastSnapshotEventHash = @event.LastEventHash;
            _lastSnapshotProvenance = @event.LastProvenanceData;
        }
    }
}
