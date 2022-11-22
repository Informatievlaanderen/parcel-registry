namespace ParcelRegistry.Legacy
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;
    using Microsoft.Extensions.Logging;

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

        public CrabCoordinate? XCoordinate { get; private set; }
        public CrabCoordinate? YCoordinate { get; private set; }

        public bool IsMigrated { get; private set; }

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
            Register<TerrainObjectWasImportedFromCrab>(When);

            Register<ParcelWasMarkedAsMigrated>(When);

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

        private void When(TerrainObjectWasImportedFromCrab @event)
        {
            XCoordinate =  @event.XCoordinate.HasValue
                ? new CrabCoordinate(@event.XCoordinate.Value)
                : null;

            YCoordinate = @event.YCoordinate.HasValue
                ? new CrabCoordinate(@event.YCoordinate.Value)
                : null;

            WhenCrabEventApplied(@event.Modification == CrabModification.Delete);
        }

        private void When(ParcelWasMarkedAsMigrated @event)
        {
            IsMigrated = true;
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

            XCoordinate = snapshot.XCoordinate.HasValue
                ? new CrabCoordinate(snapshot.XCoordinate.Value)
                : null;

            YCoordinate = snapshot.YCoordinate.HasValue
                ? new CrabCoordinate(snapshot.YCoordinate.Value)
                : null;
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
                _addressCollection.AllAddressIds(),
                XCoordinate,
                YCoordinate);
        }

        public ISnapshotStrategy Strategy { get; }
    }
}
