namespace ParcelRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class ImportTerrainObjectHouseNumberFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("b1ad75d9-d340-4c8a-b143-484b100ac8b0");

        public VbrCaPaKey CaPaKey { get; }
        public CrabTerrainObjectHouseNumberId TerrainObjectHouseNumberId { get; }
        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabHouseNumberId HouseNumberId { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportTerrainObjectHouseNumberFromCrab(
            VbrCaPaKey caPaKey,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabTerrainObjectId terrainObjectId,
            CrabHouseNumberId houseNumberId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            CaPaKey = caPaKey;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            TerrainObjectId = terrainObjectId;
            HouseNumberId = houseNumberId;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportTerrainObjectHouseNumberFromCrab-{ToString()}");

        public override string ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return CaPaKey;
            yield return TerrainObjectHouseNumberId;
            yield return TerrainObjectId;
            yield return HouseNumberId;
            yield return Lifetime;
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
