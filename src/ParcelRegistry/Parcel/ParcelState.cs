namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;

    public partial class Parcel
    {
        private ParcelId _parcelId;

        public bool IsRemoved { get; private set; }
        public bool IsRetired { get; private set; }
        public bool IsRealized { get; private set; }

        private readonly AddressCollection _addressCollection = new AddressCollection();
        public Modification LastModificationBasedOnCrab { get; private set; }

        private readonly Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
            _activeHouseNumberIdsByTerreinObjectHouseNr = new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>();

        internal Parcel(ISnapshotStrategy snapshotStrategy) : this()
        {
            Strategy = snapshotStrategy;
        }

        protected Parcel()
        {
            Register<ParcelWasRegistered>(When);
            Register<ParcelWasRemoved>(When);
            Register<ParcelWasRecovered>(When);

            Register<ParcelWasRetired>(When);
            Register<ParcelWasCorrectedToRetired>(When);
            Register<ParcelWasRealized>(When);
            Register<ParcelWasCorrectedToRealized>(When);

            Register<ParcelAddressWasAttached>(When);
            Register<ParcelAddressWasDetached>(When);

            Register<AddressSubaddressWasImportedFromCrab>(When);
            Register<TerrainObjectHouseNumberWasImportedFromCrab>(When);
            Register<TerrainObjectWasImportedFromCrab>(@event => WhenCrabEventApplied(@event.Modification == CrabModification.Delete));

            Register<ParcelSnapshot>(When);
        }

        private void When(TerrainObjectHouseNumberWasImportedFromCrab @event)
        {
            var crabTerrainObjectHouseNumberId = new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId);

            var crabHouseNumberId = new CrabHouseNumberId(@event.HouseNumberId);
            if (@event.Modification == CrabModification.Delete)
                _activeHouseNumberIdsByTerreinObjectHouseNr.Remove(crabTerrainObjectHouseNumberId);
            else
                _activeHouseNumberIdsByTerreinObjectHouseNr[crabTerrainObjectHouseNumberId] = crabHouseNumberId;

            WhenCrabEventApplied();
        }

        private void When(AddressSubaddressWasImportedFromCrab @event)
        {
            _addressCollection.Add(@event);
            WhenCrabEventApplied();
        }

        private void When(ParcelAddressWasDetached @event)
        {
            _addressCollection.Remove(new AddressId(@event.AddressId));
        }

        private void When(ParcelAddressWasAttached @event)
        {
            _addressCollection.Add(new AddressId(@event.AddressId));
        }

        private void When(ParcelWasCorrectedToRealized @event)
        {
            IsRetired = false;
            IsRealized = true;
        }

        private void When(ParcelWasRealized @event)
        {
            IsRetired = false;
            IsRealized = true;
        }

        private void When(ParcelWasCorrectedToRetired @event)
        {
            IsRealized = false;
            IsRetired = true;
        }

        private void When(ParcelWasRetired @event)
        {
            IsRealized = false;
            IsRetired = true;
        }

        private void When(ParcelWasRemoved @event)
        {
            IsRemoved = true;
        }

        private void When(ParcelWasRegistered @event)
        {
            _parcelId = new ParcelId(@event.ParcelId);
        }

        private void When(ParcelWasRecovered @event)
        {
            IsRemoved = false;
            IsRealized = false;
            IsRetired = false;
            _addressCollection.Clear();
        }

        private void WhenCrabEventApplied(bool isDeleted = false)
        {
            if (isDeleted)
                LastModificationBasedOnCrab = Modification.Delete;
            else if (LastModificationBasedOnCrab == Modification.Unknown)
                LastModificationBasedOnCrab = Modification.Insert;
            else if (LastModificationBasedOnCrab == Modification.Insert)
                LastModificationBasedOnCrab = Modification.Update;
        }

        private void When(ParcelSnapshot snapshot)
        {
            _parcelId = new ParcelId(snapshot.ParcelId);
            if (!string.IsNullOrEmpty(snapshot.ParcelStatus))
            {
                var status = ParcelStatus.Parse(snapshot.ParcelStatus);
                if (status == ParcelStatus.Realized)
                    IsRealized = true;
                if (status == ParcelStatus.Retired)
                    IsRetired = true;
            }

            IsRemoved = snapshot.IsRemoved;
            LastModificationBasedOnCrab = snapshot.LastModificationBasedOnCrab;

            foreach (var activeHouseNumberByTerrainObject in snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr)
                _activeHouseNumberIdsByTerreinObjectHouseNr.Add(
                    new CrabTerrainObjectHouseNumberId(activeHouseNumberByTerrainObject.Key),
                    new CrabHouseNumberId(activeHouseNumberByTerrainObject.Value));

            foreach (var addressId in snapshot.AddressIds)
                _addressCollection.Add(new AddressId(addressId));

            foreach (var subaddressWasImportedFromCrab in snapshot.ImportedSubaddressFromCrab)
                _addressCollection.Add(subaddressWasImportedFromCrab);
        }

        public object TakeSnapshot()
        {
            var parcelStatus = (ParcelStatus?)null;
            if (IsRetired)
                parcelStatus = ParcelStatus.Retired;
            else if (IsRealized)
                parcelStatus = ParcelStatus.Realized;

            return new ParcelSnapshot(
                _parcelId,
                parcelStatus,
                IsRemoved,
                LastModificationBasedOnCrab,
                _activeHouseNumberIdsByTerreinObjectHouseNr,
                _addressCollection.AllSubaddressWasImportedFromCrabEvents(),
                _addressCollection.AllAddressIds());
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
