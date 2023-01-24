namespace ParcelRegistry.Api.Legacy.Parcel.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;

    public record ParcelListRequest(
        FilteringHeader<ParcelFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<ParcelListResponse>;
}
