namespace ParcelRegistry.Parcel.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportSubaddressFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("8e303364-68ff-45d5-a864-375e749b700a");

        public VbrCaPaKey CaPaKey { get; }
        public CrabSubaddressId SubaddressId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public BoxNumber BoxNumber { get; }
        public CrabBoxNumberType BoxNumberType { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportSubaddressFromCrab(
            VbrCaPaKey caPaKey,
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            CaPaKey = caPaKey;
            SubaddressId = subaddressId;
            HouseNumberId = houseNumberId;
            BoxNumber = boxNumber;
            BoxNumberType = boxNumberType;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportSubaddressFromCrab-{ToString()}");

        public override string ToString()
            => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return CaPaKey;
            yield return SubaddressId;
            yield return HouseNumberId;
            yield return BoxNumber;
            yield return BoxNumberType;
            yield return Lifetime;
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
