namespace ParcelRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;

    /// <summary>
    /// What one parcel stream cost. Load and dispatch are kept apart because they scale with different
    /// things — loading with the number of events in the stream, dispatching with the size of the geometry
    /// written — and a staging run is only extrapolatable to production if you can tell which of the two
    /// dominates. <paramref name="GeometryByteCount"/> is recorded alongside them because polygon complexity
    /// is what makes one parcel cost more than another.
    /// </summary>
    internal sealed record StreamConversionResult(
        bool NeedsConversion,
        int GeometryByteCount,
        TimeSpan LoadDuration,
        TimeSpan DispatchDuration)
    {
        public TimeSpan TotalDuration => LoadDuration + DispatchDuration;
    }
}
