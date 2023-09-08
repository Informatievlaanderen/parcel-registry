namespace ParcelRegistry.Api.Legacy.Parcel.List
{
    using System;
    using NodaTime;
    using ParcelRegistry.Parcel;

    public sealed class ParcelListV2QueryItem
    {
        public string CaPaKey { get; init; }
        public ParcelStatus Status => ParcelStatus.Parse(StatusAsString);

        public string StatusAsString { get; init; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; init; }

        public Instant VersionTimestamp => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
    }
}
