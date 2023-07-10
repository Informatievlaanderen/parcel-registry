namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class Parcel
    {
        private IAddresses _addresses;
        private IParcelEvent? _lastEvent;

        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;

        private readonly List<AddressPersistentLocalId> _addressPersistentLocalIds = new();

        public ParcelId ParcelId { get; private set; }
        public VbrCaPaKey CaPaKey { get; private set; }
        public ParcelStatus ParcelStatus { get; private set; }
        public IReadOnlyList<AddressPersistentLocalId> AddressPersistentLocalIds => _addressPersistentLocalIds;
        public ExtendedWkbGeometry Geometry { get; private set; } // TODO: change prop name to ExtendedWkbGeometry?

        public bool IsRemoved { get; private set; }

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        internal Parcel(ISnapshotStrategy snapshotStrategy, IAddresses addresses) : this()
        {
            Strategy = snapshotStrategy;
            _addresses = addresses;
        }

        private Parcel()
        {
            Register<ParcelWasMigrated>(When);
            Register<ParcelAddressWasAttachedV2>(When);
            Register<ParcelAddressWasDetachedV2>(When);
            Register<ParcelWasImported>(When);
            Register<ParcelWasRetiredV2>(When);
            Register<ParcelGeometryWasChanged>(When);

            // Address Events
            Register<ParcelAddressWasDetachedBecauseAddressWasRemoved>(When);
            Register<ParcelAddressWasDetachedBecauseAddressWasRejected>(When);
            Register<ParcelAddressWasDetachedBecauseAddressWasRetired>(When);
            Register<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(When);

            Register<ParcelSnapshotV2>(When);
        }

        private void When(ParcelWasMigrated @event)
        {
            ParcelId = new ParcelId(@event.ParcelId);
            CaPaKey = new VbrCaPaKey(@event.CaPaKey);
            ParcelStatus = ParcelStatus.Parse(@event.ParcelStatus);
            IsRemoved = @event.IsRemoved;

            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
            }

            Geometry = new ExtendedWkbGeometry(@event.ExtendedWkbGeometry);

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasAttachedV2 @event)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasDetachedV2 @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(ParcelWasImported @event)
        {
            ParcelId = new ParcelId(@event.ParcelId);
            CaPaKey = new VbrCaPaKey(@event.CaPaKey);
            ParcelStatus = ParcelStatus.Realized;
            IsRemoved = false;

            Geometry = new ExtendedWkbGeometry(@event.ExtendedWkbGeometry);

            _lastEvent = @event;
        }

        private void When(ParcelWasRetiredV2 @event)
        {
            ParcelStatus = ParcelStatus.Retired;

            _lastEvent = @event;
        }

        private void When(ParcelGeometryWasChanged @event)
        {
            Geometry = new ExtendedWkbGeometry(@event.ExtendedWkbGeometry);

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasDetachedBecauseAddressWasRemoved @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasDetachedBecauseAddressWasRejected @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasDetachedBecauseAddressWasRetired @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(ParcelAddressWasReplacedBecauseAddressWasReaddressed @event)
        {
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId);
            if (_addressPersistentLocalIds.Contains(previousAddressPersistentLocalId))
            {
                _addressPersistentLocalIds.Remove(previousAddressPersistentLocalId);
            }

            var addressPersistentLocalId = new AddressPersistentLocalId(@event.NewAddressPersistentLocalId);
            if (!_addressPersistentLocalIds.Contains(addressPersistentLocalId))
            {
                _addressPersistentLocalIds.Add(addressPersistentLocalId);
            }

            _lastEvent = @event;
        }

        private void When(ParcelSnapshotV2 @event)
        {
            ParcelId = new ParcelId(@event.ParcelId);
            CaPaKey = new VbrCaPaKey(@event.CaPaKey);
            ParcelStatus = ParcelStatus.Parse(@event.ParcelStatus);
            IsRemoved = @event.IsRemoved;
            Geometry = new ExtendedWkbGeometry(@event.ExtendedWkbGeometry);

            foreach (var addressPersistentLocalId in @event.AddressPersistentLocalIds)
            {
                _addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
            }

            _lastSnapshotEventHash = @event.LastEventHash;
            _lastSnapshotProvenance = @event.LastProvenanceData;
        }
    }
}
