namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class AddressIsRemovedException : DomainException
    {
        public AddressIsRemovedException()
        { }

        private AddressIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
