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
    }
}
