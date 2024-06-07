namespace ParcelRegistry.Api.Oslo.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using ParcelRegistry.Parcel;

    public static class ParcelStatusExtensions
    {
        public static PerceelStatus MapToPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;

        public static ParcelStatus MapToParcelStatus(this PerceelStatus perceelStatus)
            => perceelStatus == PerceelStatus.Gehistoreerd
                ? ParcelStatus.Retired
                : ParcelStatus.Realized;

        public static PerceelStatus? MapToPerceelStatusSyndication(this ParcelRegistry.Legacy.ParcelStatus? status)
            => status.HasValue ? MapToPerceelStatus(status.Value) : (PerceelStatus?)null;

        public static PerceelStatus MapToPerceelStatus(this ParcelRegistry.Legacy.ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;
    }
}
