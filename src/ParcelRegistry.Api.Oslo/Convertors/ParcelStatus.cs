namespace ParcelRegistry.Api.Oslo.Convertors
{
    using ParcelRegistry.Parcel;

    public static class ParcelStatusExtensions
    {
        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus MapToPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus.Gehistoreerd
                : Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus.Gerealiseerd;

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Perceel.PerceelStatusValue MapToOsloPerceelStatus(this ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? Be.Vlaanderen.Basisregisters.GrAr.Oslo.Perceel.PerceelStatusValue.Gehistoreerd
                : Be.Vlaanderen.Basisregisters.GrAr.Oslo.Perceel.PerceelStatusValue.Gerealiseerd;

        public static ParcelStatus MapToParcelStatus(this Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus perceelStatus)
            => perceelStatus == Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus.Gehistoreerd
                ? ParcelStatus.Retired
                : ParcelStatus.Realized;

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus? MapToPerceelStatusSyndication(this ParcelRegistry.Legacy.ParcelStatus? status)
            => status.HasValue ? MapToPerceelStatus(status.Value) : (Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus?)null;

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus MapToPerceelStatus(this ParcelRegistry.Legacy.ParcelStatus parcelStatus)
            => parcelStatus == ParcelStatus.Retired
                ? Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus.Gehistoreerd
                : Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel.PerceelStatus.Gerealiseerd;
    }
}
