namespace ParcelRegistry.Api.Oslo.Parcel.V3.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Swashbuckle.AspNetCore.Filters;

    public class TotalCountOsloResponseExample : IExamplesProvider<TotaalAantalResponse>
    {
        public TotaalAantalResponse GetExamples()
        {
            return new TotaalAantalResponse
            {
                Aantal = 574512
            };
        }
    }
}
