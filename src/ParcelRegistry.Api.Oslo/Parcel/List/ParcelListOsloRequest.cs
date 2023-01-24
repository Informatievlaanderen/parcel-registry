namespace ParcelRegistry.Api.Oslo.Parcel.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record ParcelListOsloRequest(
        FilteringHeader<ParcelFilter> Filtering,
        SortingHeader Sorting,
        IPaginationRequest Pagination) : IRequest<ParcelListOsloResponse>;
}
