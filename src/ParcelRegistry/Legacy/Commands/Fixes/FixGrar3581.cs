namespace ParcelRegistry.Legacy.Commands.Fixes
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class FixGrar3581
    {
        private static readonly Guid Namespace = new Guid("f7286c2d-1bec-4555-9154-51dc5d691852");

        public ParcelId ParcelId { get; }
        public ParcelStatus ParcelStatus { get; }
        public IEnumerable<AddressId> AddressIds { get; }

        public FixGrar3581(
            ParcelId parcelId,
            ParcelStatus parcelStatus,
            IEnumerable<AddressId> addressIds)
        {
            ParcelId = parcelId;
            ParcelStatus = parcelStatus;
            AddressIds = addressIds;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"FixGrar3581-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
            yield return ParcelStatus;
            foreach (var addressId in AddressIds)
            {
                yield return addressId.ToString();
            }
        }
    }
}
