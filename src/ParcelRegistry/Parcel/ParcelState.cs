namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;

    public partial class Parcel
    {
        private ParcelId _parcelId;

        public bool IsRemoved { get; private set; }
        public Version Version { get; private set; }
        public bool IsRetired { get; private set; }
        public bool IsRealized { get; private set; }

        private readonly AddressCollection _addressCollection = new AddressCollection();
        public Modification LastModificationBasedOnCrab { get; private set; }

        private Parcel()
        {
            Register<ParcelWasRegistered>(When);
            Register<ParcelWasRemoved>(When);

            Register<ParcelWasRetired>(When);
            Register<ParcelWasCorrectedToRetired>(When);
            Register<ParcelWasRealized>(When);
            Register<ParcelWasCorrectedToRealized>(When);

            Register<ParcelAddressWasAttached>(When);
            Register<ParcelAddressWasDetached>(When);

            Register<AddressSubaddressWasImportedFromCrab>(When);
            Register<TerrainObjectHouseNumberWasImportedFromCrab>(@event => WhenCrabEventApplied());
            Register<TerrainObjectWasImportedFromCrab>(@event => WhenCrabEventApplied(@event.Modification == CrabModification.Delete));
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
            Version = new Version(0);
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
    }
}
