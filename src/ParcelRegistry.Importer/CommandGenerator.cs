using Be.Vlaanderen.Basisregisters.GrAr.Common;

namespace ParcelRegistry.Importer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
    using Crab;
    using NodaTime;
    using Parcel.Commands.Crab;

    public class CommandGenerator : ICommandGenerator<CaPaKey>
    {
        public string Name => GetType().FullName;

        public IEnumerable<CaPaKey> GetChangedKeys(DateTime from, DateTime until)
            => CrabQueries.GetChangedPerceelIdsBetween(from, until)
                .Select(CaPaKey.CreateFrom)
                .Distinct()
                .OrderBy(x => x.VbrCaPaKey)
                .ToList();

        public IEnumerable<dynamic> GenerateInitCommandsFor(CaPaKey key, DateTime from, DateTime until)
            => CreateCommandsInOrder(ImportMode.Init, key, from, until);

        public IEnumerable<dynamic> GenerateUpdateCommandsFor(CaPaKey key, DateTime from, DateTime until)
            => CreateCommandsInOrder(ImportMode.Update, key, from, until);

        private static IEnumerable<dynamic> CreateCommandsInOrder(ImportMode importMode, CaPaKey caPaKey, DateTime from, DateTime until)
        {
            var importTerrainObjectCommands = new List<ImportTerrainObjectFromCrab>();
            var importTerrainObjectHouseNumberCommands = new List<ImportTerrainObjectHouseNumberFromCrab>();
            var importSubaddressCommands = new List<ImportSubaddressFromCrab>();

            using (var crabEntities = new CRABEntities())
            {
                var terrainObjectIds = PerceelQueries
                    .GetTblTerreinObjectIdsByCapaKeys(
                        caPaKey.CaPaKeyCrabNotation1,
                        caPaKey.CaPaKeyCrabNotation2,
                        crabEntities)
                    .Concat(PerceelQueries
                    .GetTblTerreinObjectIdsHistByCapaKeys(
                        caPaKey.CaPaKeyCrabNotation1,
                        caPaKey.CaPaKeyCrabNotation2,
                        crabEntities))
                    .Distinct()
                    .ToList();

                var terrainObjects = PerceelQueries
                    .GetTblTerreinObjectenByTerreinObjectIds(terrainObjectIds, crabEntities);
                var terrainObjectsHist = PerceelQueries
                    .GetTblTerreinObjectenHistByTerreinObjectIds(terrainObjectIds, crabEntities);

                importTerrainObjectCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(terrainObjects, caPaKey));
                importTerrainObjectCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(terrainObjectsHist, caPaKey));

                var terrainObjectHouseNumbers = PerceelQueries
                    .GetTblTerreinObjectHuisNummersByTerreinObjectIds(terrainObjectIds, crabEntities);
                var terrainObjectHouseNumbersHistList = PerceelQueries
                    .GetTblTerreinObjectHuisNummersHistByTerreinObjectIds(terrainObjectIds, crabEntities);

                importTerrainObjectHouseNumberCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(terrainObjectHouseNumbers, caPaKey));
                importTerrainObjectHouseNumberCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(terrainObjectHouseNumbersHistList, caPaKey));

                var allHouseNumberIds = importTerrainObjectHouseNumberCommands
                    .Select(x => (int)x.HouseNumberId)
                    .ToList();

                var subaddresses = AdresSubadresQueries
                    .GetTblSubAdressenByHuisnummerIds(allHouseNumberIds, crabEntities);
                var subaddressesHist = AdresSubadresQueries
                    .GetTblSubAdressenHistByHuisnummerIds(allHouseNumberIds, crabEntities);

                var allSubadresIds = subaddresses
                    .Select(s => s.subAdresId)
                    .Concat(subaddressesHist.Where(s => s.subAdresId.HasValue).Select(s => s.subAdresId.Value))
                    .Distinct()
                    .ToList();

                foreach (var subadresId in allSubadresIds)
                {
                    var firstOccurrenceOdb = subaddresses.Where(sa => sa.subAdresId == subadresId)
                        .OrderBy(sa => sa.beginTijd).FirstOrDefault();
                    var firstOccurrenceCdb = subaddressesHist.Where(sa => sa.subAdresId == subadresId)
                        .OrderBy(sa => sa.beginTijd).FirstOrDefault();

                    if (firstOccurrenceCdb != null &&
                        (firstOccurrenceOdb == null || firstOccurrenceCdb.beginTijd <= firstOccurrenceOdb.beginTijd))
                        importSubaddressCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(firstOccurrenceCdb, caPaKey));
                    else if (firstOccurrenceOdb != null)
                        importSubaddressCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(firstOccurrenceOdb, caPaKey));

                    importSubaddressCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(
                        subaddresses.Where(sa => sa.subAdresId == subadresId && sa.eindDatum.HasValue),
                        caPaKey));
                    importSubaddressCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(
                        subaddressesHist.Where(hist => hist.subAdresId == subadresId && hist.eindDatum.HasValue),
                        caPaKey));

                    var removedRecord = subaddressesHist.SingleOrDefault(histRecord =>
                        histRecord.subAdresId == subadresId &&
                        histRecord.eindBewerking == BewerkingCodes.Remove);

                    if (removedRecord != null)
                        importSubaddressCommands.AddRange(TerrainObjectCommandsFactory.CreateFor(removedRecord.CreateBeginSituation(), caPaKey));
                }
            }

            var commands = new List<dynamic>();

            var groupedTerrainObjectCommands = importTerrainObjectCommands
                .GroupBy(x => x.CaPaKey)
                .ToDictionary(x => x.Key, x => x.ToList().OrderBy(y => (Instant)y.Timestamp));

            foreach (var groupedTerrainObjectCommand in groupedTerrainObjectCommands)
                commands.Add(groupedTerrainObjectCommand.Value.First());

            var allParcelCommands = importTerrainObjectCommands
                .Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 0, $"CaPaKey: {x.CaPaKey}"))
                .Concat(importTerrainObjectHouseNumberCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 0, $"CaPaKey: {x.CaPaKey}")))
                .Concat(importSubaddressCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, -1, 0, $"CaPaKey: {x.CaPaKey}")))
                .ToList();

            var allCommands = allParcelCommands
                .Where(x => x.Item1.Timestamp <= until.ToCrabInstant() && x.Item1.Timestamp > from.ToCrabInstant())
                .ToList();

            if (importMode == ImportMode.Update) //if an update couples the terrainobjecthousenumber, with subaddress already created before the terrainobjecthousenumber => import the subaddress
            {
                var houseNumberForUpdates = importTerrainObjectHouseNumberCommands
                    .Where(x => x.Timestamp > from.ToCrabInstant() && x.Timestamp <= until.ToCrabInstant())
                    .Select(x => x.HouseNumberId)
                    .ToList();

                if (houseNumberForUpdates.Any())
                {
                    var houseNumbersBeforeUpdate = importTerrainObjectHouseNumberCommands
                        .Where(x => x.Timestamp <= from.ToCrabInstant())
                        .Select(x => x.HouseNumberId)
                        .ToList();

                    var newHouseNumbers = houseNumberForUpdates.Except(houseNumbersBeforeUpdate);

                    foreach (var newHouseNumber in newHouseNumbers)
                    {
                        var allNewSubaddressIds = importSubaddressCommands
                            .Where(subaddressFromCrab => subaddressFromCrab.HouseNumberId == newHouseNumber)
                            .Select(x => x.SubaddressId);
                                
                        foreach (var newSubaddressId in allNewSubaddressIds)
                        {
                            allCommands = allCommands
                                .Concat(allParcelCommands
                                    .Except(allCommands)
                                    .Where(x => x.Item1 is ImportSubaddressFromCrab importSubaddressFromCrab && importSubaddressFromCrab.SubaddressId == newSubaddressId))
                                .ToList();
                        }
                    }
                }
            }

            allCommands = allCommands
                .OrderBy(x => x.Item1.Timestamp)
                .ThenBy(x => x.Item2)
                .ThenBy(x => x.Item3)
                .ToList();

            commands.AddRange(allCommands.Select(command => command.Item1));

            return commands;
        }
    }
}
