namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class ParcelIsRemovedException : DomainException
    {
        public ParcelIsRemovedException()
        { }

        private ParcelIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
