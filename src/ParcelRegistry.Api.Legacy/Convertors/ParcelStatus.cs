namespace ParcelRegistry.Api.Legacy.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using ParcelRegistry.Legacy;

    public static class ParcelStatusExtensions
    {
        public static PerceelStatus? MapToPerceelStatusSyndication(this ParcelStatus? status)
            => status.HasValue ? MapToPerceelStatus(status.Value) : (PerceelStatus?)null;

        public static PerceelStatus MapToPerceelStatus(this ParcelStatus? status)
            => MapToPerceelStatus(status ?? ParcelStatus.Realized);

        public static PerceelStatus MapToPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;

        public static ParcelStatus MapToParcelStatus(this PerceelStatus perceelStatus)
            => perceelStatus == PerceelStatus.Gehistoreerd
                ? ParcelStatus.Retired
                : ParcelStatus.Realized;
    }
}
