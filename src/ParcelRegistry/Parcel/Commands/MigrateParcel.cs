namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class MigrateParcel : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("2e07d81c-ebb9-43ed-96bc-d13911bc30c9");

        public ParcelId ParcelId { get; }
        public VbrCaPaKey CaPaKey { get; }
        public ParcelStatus ParcelStatus { get; }
        public bool IsRemoved { get; }
        public List<AddressPersistentLocalId> AddressPersistentLocalIds { get; }

        public Coordinate? XCoordinate { get; }
        public Coordinate? YCoordinate { get; }

        public Provenance Provenance { get; }

        public MigrateParcel(
            Legacy.ParcelId parcelId,
            VbrCaPaKey caPaKey,
            Legacy.ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            Coordinate? xCoordinate,
            Coordinate? yCoordinate,
            Provenance provenance)
        {
            ParcelId = new ParcelId(parcelId);
            CaPaKey = caPaKey;
            ParcelStatus = Legacy.ParcelStatusHelpers.Map(parcelStatus);
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = addressPersistentLocalIds.ToList();
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MigrateLegacyParcel-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId.ToString();
            yield return CaPaKey;
            yield return ParcelStatus;
            yield return IsRemoved.ToString();

            foreach (var addressPersistentLocalId in AddressPersistentLocalIds)
            {
                yield return addressPersistentLocalId;
            }

            if (XCoordinate is not null)
            {
                yield return XCoordinate;
            }

            if (YCoordinate is not null)
            {
                yield return YCoordinate;
            }
        }
    }
}
