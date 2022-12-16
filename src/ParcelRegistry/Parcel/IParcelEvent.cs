namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public interface IParcelEvent : IHasParcelId, IHasProvenance, ISetProvenance, IHaveHash, IMessage
    { }
}
