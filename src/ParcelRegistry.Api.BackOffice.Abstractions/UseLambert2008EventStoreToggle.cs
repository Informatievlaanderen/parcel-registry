namespace ParcelRegistry.Api.BackOffice.Abstractions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;

    /// <summary>
    /// Indicates whether the event store persists parcel geometries in Lambert 2008 (EPSG 3812) instead of
    /// Lambert 72 (EPSG 31370). Incoming geometries are always normalized to <see cref="EventStoreSrid"/>,
    /// whichever reference system they arrive in. See ADR 0005.
    /// </summary>
    public sealed class UseLambert2008EventStoreToggle
    {
        public bool FeatureEnabled { get; }

        public int EventStoreSrid => FeatureEnabled
            ? SystemReferenceId.SridLambert2008
            : SystemReferenceId.SridLambert72;

        public UseLambert2008EventStoreToggle(bool featureEnabled) => FeatureEnabled = featureEnabled;
    }
}
