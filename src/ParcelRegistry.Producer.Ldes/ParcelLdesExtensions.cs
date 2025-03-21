namespace ParcelRegistry.Producer.Ldes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Parcel;

    public static class ParcelLdesExtensions
    {
        public static PerceelStatus MapToPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;

        public static async Task<ParcelDetail> FindAndUpdateParcelDetail(
            this ProducerContext context,
            Guid parcelId,
            Action<ParcelDetail> updateFunc,
            CancellationToken ct)
        {
            var parcel = await context
                .Parcels
                .FindAsync(parcelId, cancellationToken: ct);

            if (parcel == null)
                throw new ProjectionItemNotFoundException<ProducerProjections>(parcelId.ToString("D"));

            updateFunc(parcel);

            return parcel;
        }
    }
}
