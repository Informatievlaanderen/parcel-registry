namespace ParcelRegistry.Legacy.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy exception and should not be used anymore.")]
    [Serializable]
    public sealed class ParcelRemovedException : DomainException
    {
        public ParcelRemovedException() { }

        private ParcelRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ParcelRemovedException(string message) : base(message) { }

        public ParcelRemovedException(string message, Exception inner) : base(message, inner) { }
    }
}
