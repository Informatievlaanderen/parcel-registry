﻿namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IParcels : IAsyncRepository<Parcel, ParcelStreamId> { }

    public class ParcelStreamId : ValueObject<ParcelStreamId>
    {
        private readonly ParcelId _parcelId;

        public ParcelStreamId(ParcelId parcelId)
        {
            _parcelId = parcelId;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return _parcelId;
        }

        public override string ToString() => $"parcel-{_parcelId}";
    }
}
