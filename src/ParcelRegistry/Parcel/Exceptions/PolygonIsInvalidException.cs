namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class PolygonIsInvalidException : DomainException
    {
        public PolygonIsInvalidException()
        { }

        private PolygonIsInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
