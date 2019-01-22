namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IParcels : IAsyncRepository<Parcel, ParcelId> { }
}
