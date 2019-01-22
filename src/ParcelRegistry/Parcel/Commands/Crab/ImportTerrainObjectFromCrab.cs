namespace ParcelRegistry.Parcel.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ImportTerrainObjectFromCrab : IHasCrabProvenance
    {
        private static readonly Guid Namespace = new Guid("87a1cf7c-9c7b-4f25-967c-66a9459c9d5e");

        public VbrCaPaKey CaPaKey { get; }
        public CrabTerrainObjectId TerrainObjectId { get; }
        public CrabIdentifierTerrainObject IdentifierTerrainObject { get; }
        public CrabTerrainObjectNatureCode TerrainObjectNatureCode { get; }
        public CrabCoordinate XCoordinate { get; }
        public CrabCoordinate YCoordinate { get; }
        public CrabBuildingNature BuildingNature { get; }
        public CrabLifetime Lifetime { get; }
        public CrabTimestamp Timestamp { get; }
        public CrabOperator Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public ImportTerrainObjectFromCrab(
            VbrCaPaKey caPaKey,
            CrabTerrainObjectId terrainObjectId,
            CrabIdentifierTerrainObject identifierTerrainObject,
            CrabTerrainObjectNatureCode terrainObjectNatureCode,
            CrabCoordinate xCoordinate,
            CrabCoordinate yCoordinate,
            CrabBuildingNature buildingNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            CaPaKey = caPaKey;
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
            TerrainObjectNatureCode = terrainObjectNatureCode;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            BuildingNature = buildingNature;
            Lifetime = lifetime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ImportTerrainObjectFromCrab-{ToString()}");

        public override string ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return CaPaKey;
            yield return TerrainObjectId;
            yield return IdentifierTerrainObject;
            yield return TerrainObjectNatureCode;
            yield return XCoordinate;
            yield return YCoordinate;
            yield return BuildingNature;
            yield return Lifetime;
            yield return Timestamp;
            yield return Operator;
            yield return Modification;
            yield return Organisation;
        }
    }
}
