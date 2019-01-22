namespace ParcelRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel.Events;

    public static class EventExtensions
    {
        public static ParcelAddressWasAttached WithAddressId(this ParcelAddressWasAttached @event, AddressId addressId)
        {
            var newEvent = new ParcelAddressWasAttached(new ParcelId(@event.ParcelId), addressId);
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }
    }
}
