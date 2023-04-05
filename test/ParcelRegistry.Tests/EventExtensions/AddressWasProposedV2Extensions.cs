namespace ParcelRegistry.Tests.EventExtensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;

    public static class AddressWasProposedV2Extensions
    {
        public static AddressWasProposedV2 WithAddressPersistentLocalId(this AddressWasProposedV2 @event, int addressPersistentLocalId)
        {
            return new AddressWasProposedV2(
                @event.StreetNamePersistentLocalId,
                addressPersistentLocalId,
                @event.ParentPersistentLocalId,
                @event.PostalCode,
                @event.HouseNumber,
                @event.BoxNumber,
                @event.GeometryMethod,
                @event.GeometrySpecification,
                @event.ExtendedWkbGeometry,
                @event.Provenance);
        }
    }
}
