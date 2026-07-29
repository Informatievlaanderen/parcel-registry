namespace ParcelRegistry.Api.Oslo.Parcel.V3.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using List;
    using MediatR;

    public record ParcelCountOsloRequest(
        FilteringHeader<ParcelFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<TotaalAantalResponse>;
}
