namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class AddressNotFoundException : DomainException
    {
        public AddressNotFoundException()
        { }

        private AddressNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
