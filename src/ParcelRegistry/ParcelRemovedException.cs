namespace ParcelRegistry
{
    using System;

    public class ParcelRemovedException : ParcelRegistryException
    {
        public ParcelRemovedException() { }

        public ParcelRemovedException(string message) : base(message) { }

        public ParcelRemovedException(string message, Exception inner) : base(message, inner) { }
    }
}
