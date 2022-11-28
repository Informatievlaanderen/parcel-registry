using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelRegistry.Parcel.Exceptions
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class AddressIsRemovedException : DomainException
    {
        public AddressIsRemovedException()
        { }

        private AddressIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
