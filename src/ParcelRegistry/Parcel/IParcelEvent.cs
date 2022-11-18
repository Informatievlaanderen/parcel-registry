using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

namespace ParcelRegistry.Parcel
{
    public interface IParcelEvent : IHasParcelId, IHasProvenance, ISetProvenance, IHaveHash, IMessage
    { }
}
