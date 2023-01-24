namespace ParcelRegistry.Api.Legacy.Parcel.Count
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using List;
    using MediatR;

    public record ParcelCountRequest(
        FilteringHeader<ParcelFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<TotaalAantalResponse>;
}
