namespace ParcelRegistry.Legacy
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IParcels : IAsyncRepository<Parcel, ParcelId> { }
}
