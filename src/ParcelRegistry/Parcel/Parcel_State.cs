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

        private List<AddressPersistentLocalId> _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        public ParcelId ParcelId { get; private set; }
        public ParcelPersistentLocalId ParcelPersistentLocalId { get; private set; }
        public ParcelStatus ParcelStatus { get; private set; }
        public IReadOnlyList<AddressPersistentLocalId> AddressPersistentLocalIds => _addressPersistentLocalIds;
        public bool IsRemoved { get; private set; }

        //private readonly Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
        //    _activeHouseNumberIdsByTerreinObjectHouseNr = new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>();

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        internal Parcel(ISnapshotStrategy snapshotStrategy) : this()
        {
            Strategy = snapshotStrategy;
        }

        protected Parcel()
        {
            Register<ParcelSnapshotV2>(When);
        }

        private void When(ParcelSnapshotV2 @event)
        {
            ParcelPersistentLocalId = new ParcelPersistentLocalId(@event.ParcelPersistentLocalId);
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
