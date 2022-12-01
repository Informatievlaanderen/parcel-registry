namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class ParcelHasInvalidStatusException : DomainException
    {
        public ParcelHasInvalidStatusException()
        { }
        
        private ParcelHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
