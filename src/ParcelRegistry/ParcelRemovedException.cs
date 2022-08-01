namespace ParcelRegistry
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ParcelRemovedException : ParcelRegistryException
    {
        public ParcelRemovedException() { }

        private ParcelRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ParcelRemovedException(string message) : base(message) { }

        public ParcelRemovedException(string message, Exception inner) : base(message, inner) { }
    }
}
