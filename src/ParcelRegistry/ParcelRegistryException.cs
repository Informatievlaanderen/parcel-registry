namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public abstract class ParcelRegistryException : DomainException
    {
        protected ParcelRegistryException() { }

        protected ParcelRegistryException(string message) : base(message) { }

        protected ParcelRegistryException(string message, Exception inner) : base(message, inner) { }
    }
}
