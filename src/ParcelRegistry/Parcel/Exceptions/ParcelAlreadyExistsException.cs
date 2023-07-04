namespace ParcelRegistry.Parcel.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public sealed class ParcelAlreadyExistsException : DomainException
    {
        public VbrCaPaKey VbrCaPaKey { get; }

        public ParcelAlreadyExistsException()
        { }

        public ParcelAlreadyExistsException(VbrCaPaKey vbrCaPaKey)
        {
            VbrCaPaKey = vbrCaPaKey;
        }

        private ParcelAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
