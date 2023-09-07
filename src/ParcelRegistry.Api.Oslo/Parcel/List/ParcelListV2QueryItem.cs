namespace ParcelRegistry.Api.Oslo.Parcel.List
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using ParcelRegistry.Parcel;
    using Projections.Legacy.ParcelDetailV2;

    public sealed class ParcelListV2QueryItem
    {
        public string CaPaKey { get; init; }
        public ParcelStatus Status => ParcelStatus.Parse(StatusAsString);

        public string StatusAsString { get; init; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; init; }

        public Instant VersionTimestamp => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
    }
}
