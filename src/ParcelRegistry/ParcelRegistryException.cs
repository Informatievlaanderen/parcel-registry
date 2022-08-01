namespace ParcelRegistry
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class ParcelRegistryException : DomainException
    {
        protected ParcelRegistryException()
        { }

        protected ParcelRegistryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        protected ParcelRegistryException(string message)
            : base(message)
        { }

        protected ParcelRegistryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
