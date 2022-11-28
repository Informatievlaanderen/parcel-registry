namespace ParcelRegistry.Api.BackOffice.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Parcel;

    public interface IIfMatchHeaderValidator
    {
        public Task<bool> IsValid(
            string? ifMatchHeaderValue,
            ParcelId parcelId,
            CancellationToken cancellationToken);
    }

    public class IfMatchHeaderValidator : IIfMatchHeaderValidator
    {
        private readonly IParcels _parcels;

        public IfMatchHeaderValidator(IParcels parcels)
        {
            _parcels = parcels;
        }

        public async Task<bool> IsValid(
            string? ifMatchHeaderValue,
            ParcelId parcelId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ifMatchHeaderValue))
            {
                return true;
            }

            var ifMatchTag = ifMatchHeaderValue.Trim();
            var lastHash = await _parcels.GetHash(
                parcelId,
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            return ifMatchTag == lastHashTag.ToString();
        }
    }
}
