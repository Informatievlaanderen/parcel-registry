namespace ParcelRegistry.Tests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using Parcel.Events;
    using Parcel.Events.Crab;

    public static class SnapshotBuilder
    {
        public static ParcelSnapshot WithParcelStatus(this ParcelSnapshot snapshot, ParcelStatus? parcelStatus)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                parcelStatus,
                snapshot.IsRemoved,
                snapshot.LastModificationBasedOnCrab,
                snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                    .ToDictionary(
                        x => new CrabTerrainObjectHouseNumberId(x.Key),
                        y => new CrabHouseNumberId(y.Value)),
                snapshot.ImportedSubaddressFromCrab,
                snapshot.AddressIds.Select(x => new AddressId(x)));
        }

        public static ParcelSnapshot WithIsRemoved(this ParcelSnapshot snapshot, bool isRemoved)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                string.IsNullOrEmpty(snapshot.ParcelStatus) ? null : ParcelStatus.Parse(snapshot.ParcelStatus),
                isRemoved,
                snapshot.LastModificationBasedOnCrab,
                snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                    .ToDictionary(
                        x => new CrabTerrainObjectHouseNumberId(x.Key),
                        y => new CrabHouseNumberId(y.Value)),
                snapshot.ImportedSubaddressFromCrab,
                snapshot.AddressIds.Select(x => new AddressId(x)));
        }

        public static ParcelSnapshot WithLastModificationBasedOnCrab(this ParcelSnapshot snapshot, Modification lastModification)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                string.IsNullOrEmpty(snapshot.ParcelStatus) ? null : ParcelStatus.Parse(snapshot.ParcelStatus),
                snapshot.IsRemoved,
                lastModification,
                snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                    .ToDictionary(
                        x => new CrabTerrainObjectHouseNumberId(x.Key),
                        y => new CrabHouseNumberId(y.Value)),
                snapshot.ImportedSubaddressFromCrab,
                snapshot.AddressIds.Select(x => new AddressId(x)));
        }

        public static ParcelSnapshot WithActiveHouseNumberIdsByTerrainObjectHouseNr
            (this ParcelSnapshot snapshot, Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> activeHouseNumberIdsByTerrainObjectHouseNr)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                string.IsNullOrEmpty(snapshot.ParcelStatus) ? null : ParcelStatus.Parse(snapshot.ParcelStatus),
                snapshot.IsRemoved,
                snapshot.LastModificationBasedOnCrab,
                activeHouseNumberIdsByTerrainObjectHouseNr,
                snapshot.ImportedSubaddressFromCrab,
                snapshot.AddressIds.Select(x => new AddressId(x)));
        }

        public static ParcelSnapshot WithImportedSubaddressFromCrab(this ParcelSnapshot snapshot, IEnumerable<AddressSubaddressWasImportedFromCrab> importedSubaddressFromCrab)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                string.IsNullOrEmpty(snapshot.ParcelStatus) ? null : ParcelStatus.Parse(snapshot.ParcelStatus),
                snapshot.IsRemoved,
                snapshot.LastModificationBasedOnCrab,
                snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                    .ToDictionary(
                        x => new CrabTerrainObjectHouseNumberId(x.Key),
                        y => new CrabHouseNumberId(y.Value)),
                importedSubaddressFromCrab,
                snapshot.AddressIds.Select(x => new AddressId(x)));
        }

        public static ParcelSnapshot WithAddressIds(this ParcelSnapshot snapshot, IEnumerable<AddressId> addressIds)
        {
            return new ParcelSnapshot(new ParcelId(snapshot.ParcelId),
                string.IsNullOrEmpty(snapshot.ParcelStatus) ? null : ParcelStatus.Parse(snapshot.ParcelStatus),
                snapshot.IsRemoved,
                snapshot.LastModificationBasedOnCrab,
                snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                    .ToDictionary(
                        x => new CrabTerrainObjectHouseNumberId(x.Key),
                        y => new CrabHouseNumberId(y.Value)),
                snapshot.ImportedSubaddressFromCrab,
                addressIds);
        }

        public static SnapshotContainer Build(
            this ParcelSnapshot snapshot,
            long position,
            JsonSerializerSettings serializerSettings)
        {
            return new SnapshotContainer
            {
                Info = new SnapshotInfo { Position = position, Type = nameof(ParcelSnapshot) },
                Data = JsonConvert.SerializeObject(snapshot, serializerSettings)
            };
        }

        public static ParcelSnapshot CreateDefaultSnapshot(ParcelId parcelId)
        {
            return new ParcelSnapshot(
                parcelId,
                null,
                false,
                Modification.Insert,
                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(),
                new List<AddressSubaddressWasImportedFromCrab>(),
                new List<AddressId>());
        }
    }
}
