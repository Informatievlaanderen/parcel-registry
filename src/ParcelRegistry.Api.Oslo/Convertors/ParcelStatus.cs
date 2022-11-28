namespace ParcelRegistry.Api.Oslo.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using ParcelRegistry.Parcel;

    public static class ParcelStatusExtensions
    {
        public static PerceelStatus MapToPerceelStatus(this Legacy.ParcelStatus? status)
            => MapToPerceelStatus(status ?? Legacy.ParcelStatus.Realized);

        public static PerceelStatus MapToPerceelStatus(this Legacy.ParcelStatus parcelStatus)
            => parcelStatus == Legacy.ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;

        public static PerceelStatus MapToPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? PerceelStatus.Gehistoreerd
                : PerceelStatus.Gerealiseerd;

        public static Legacy.ParcelStatus MapToParcelStatus(this PerceelStatus perceelStatus)
            => perceelStatus == PerceelStatus.Gehistoreerd
                ? Legacy.ParcelStatus.Retired
                : Legacy.ParcelStatus.Realized;
    }
}
