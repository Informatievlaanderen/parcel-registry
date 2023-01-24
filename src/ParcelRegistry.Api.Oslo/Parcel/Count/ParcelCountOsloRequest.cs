namespace ParcelRegistry.Api.Oslo.Parcel.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;

    public record ParcelCountOsloRequest(
        FilteringHeader<ParcelFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<TotaalAantalResponse>;
}
