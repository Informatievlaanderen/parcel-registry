namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class AddressHasInvalidStatusException : DomainException
    {
        public AddressHasInvalidStatusException()
        { }
        
        private AddressHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
