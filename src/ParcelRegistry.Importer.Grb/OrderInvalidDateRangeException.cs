namespace ParcelRegistry.Importer.Grb
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class OrderInvalidDateRangeException : Exception
    {
        public OrderInvalidDateRangeException(string message) : base(message)
        { }

        private OrderInvalidDateRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
