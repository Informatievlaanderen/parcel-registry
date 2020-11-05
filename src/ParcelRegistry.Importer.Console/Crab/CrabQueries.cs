namespace ParcelRegistry.Importer.Console.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;

    public static class CrabQueries
    {
        private const string AardPerceel = "1";
        private static readonly string DeletedBewerking = CrabBewerking.Verwijdering.Code;

        public static List<string> GetChangedPerceelIdsBetween(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory)
        {
            if (since == DateTime.MinValue)
            {
                using (var crabEntities = crabEntitiesFactory())
                {
                    var odb = crabEntities.tblTerreinObject.Where(to => to.aardTerreinObjectCode == AardPerceel);
                    var cdb = crabEntities.tblTerreinObject_hist.Where(to => to.aardTerreinObjectCode == AardPerceel);

                    return odb
                        .GroupBy(t => t.identificatorTerreinObject)
                        .Select(t => new { t.Key, beginTijd = t.Min(o => o.beginTijd) })
                        .Concat(cdb
                            .GroupBy(t => t.identificatorTerreinObject)
                            .Select(t => new { t.Key, beginTijd = t.Min(o => o.beginTijd.Value) }))
                        .GroupBy(s => s.Key)
                        .Select(s => new { s.Key, beginTijd = s.Min(o => o.beginTijd) })
                        .Where(s => s.beginTijd <= until)
                        .Select(s => s.Key)
                        .ToList();
                }
            }

            var tasks = new[]
            {
                Task.Run(() => GetUpdatedIdsFromTblTerreinObject(since, until, crabEntitiesFactory)),
                Task.Run(() => GetUpdatedIdsFromTblTerreinObject_huisnummer(since, until, crabEntitiesFactory)),
                Task.Run(() => GetUpdatedIdsFromAddresses(since, until, crabEntitiesFactory))
            };

            Task.WaitAll(tasks);

            var allTerrainObjectIds = tasks.SelectMany(x => x.Result).ToList();
            var perceelIds = new List<string>();
            using (var crabEntities = crabEntitiesFactory())
            {
                const int sqlContainsSize = 1000;
                for (var i = 0; i < Math.Ceiling(allTerrainObjectIds.Count / (double)sqlContainsSize); i++)
                {
                    var idsInThisRange = allTerrainObjectIds
                        .Skip(i * sqlContainsSize)
                        .Take(Math.Min(sqlContainsSize, allTerrainObjectIds.Count - i * sqlContainsSize));

                    perceelIds.AddRange(crabEntities.tblTerreinObject.Where(t => t.aardTerreinObjectCode == AardPerceel && idsInThisRange.Contains(t.terreinObjectId)).Select(t => t.identificatorTerreinObject));
                    perceelIds.AddRange(crabEntities.tblTerreinObject_hist.Where(t => t.aardTerreinObjectCode == AardPerceel && idsInThisRange.Contains(t.terreinObjectId.Value)).Select(t => t.identificatorTerreinObject));
                }

                return perceelIds.Distinct().ToList();
            }
        }

        private static List<int> GetUpdatedIdsFromTblTerreinObject(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory)
        {
            var terrainObjectIds = new List<int>();

            using (var crabEntities = crabEntitiesFactory())
            {
                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject
                    .Where(to => to.aardTerreinObjectCode == AardPerceel)
                    .Where(to => to.beginTijd > since && to.beginTijd <= until)
                    .Select(to => to.terreinObjectId));

                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject_hist
                    .Where(to => to.aardTerreinObjectCode == AardPerceel)
                    .Where(to => to.beginTijd > since && to.beginTijd <= until)
                    .Select(to => to.terreinObjectId.Value));

                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject_hist
                    .Where(to => to.aardTerreinObjectCode == "1")
                    .Where(to =>
                        to.eindTijd > since && to.eindTijd <= until && to.eindBewerking == DeletedBewerking)
                    .Select(to => to.terreinObjectId.Value));
            }

            return terrainObjectIds;
        }

        private static List<int> GetUpdatedIdsFromTblTerreinObject_huisnummer(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory)
        {
            var terrainObjectIds = new List<int>();

            using (var crabEntities = crabEntitiesFactory())
            {
                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject_huisNummer
                    .Where(thn => thn.tblTerreinObject.aardTerreinObjectCode == "1")
                    .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                    .Select(hnr => hnr.terreinObjectId)
                    .ToList());

                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject_huisNummer_hist
                    .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                    .Select(hnr => hnr.terreinObjectId.Value)
                    .ToList());

                terrainObjectIds.AddRange(crabEntities
                    .tblTerreinObject_huisNummer_hist
                    .Where(to => to.eindTijd > since && to.eindTijd <= until && to.eindBewerking == DeletedBewerking)
                    .Select(to => to.terreinObjectId.Value));
            }

            return terrainObjectIds;
        }

        private static List<int> GetUpdatedIdsFromAddresses(DateTime since, DateTime until, Func<CRABEntities> crabEntitiesFactory)
        {
            var terrainObjectIds = new List<int>();

            using (var crabEntities = crabEntitiesFactory())
            {
                var huisNummerIdsSubadres = crabEntities
                    .tblSubAdres
                    .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                    .Select(hnr => hnr.huisNummerId)
                    .ToList();

                terrainObjectIds.AddRange(IterateSqlContains(huisNummerIdsSubadres, ids =>
                    crabEntities
                        .tblTerreinObject_huisNummer
                        .Where(sa => ids.Contains(sa.huisNummerId))
                        .Select(sa => sa.terreinObjectId)
                        .Concat(crabEntities
                            .tblTerreinObject_huisNummer_hist
                            .Where(sa => ids.Contains(sa.huisNummerId.Value))
                            .Select(sa => sa.terreinObjectId.Value))
                        .ToList()));

                var huisNummerIdsSubadresHist = crabEntities
                    .tblSubAdres_hist
                    .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                    .Select(hnr => hnr.huisNummerId.Value)
                    .ToList();

                huisNummerIdsSubadresHist.AddRange(crabEntities
                    .tblSubAdres_hist
                    .Where(crabRecord => crabRecord.eindTijd > since && crabRecord.eindTijd <= until &&
                                         crabRecord.eindBewerking == DeletedBewerking)
                    .Select(hnr => hnr.huisNummerId.Value)
                    .ToList());

                terrainObjectIds.AddRange(IterateSqlContains(huisNummerIdsSubadresHist, ids =>
                    crabEntities
                        .tblTerreinObject_huisNummer
                        .Where(sa => ids.Contains(sa.huisNummerId))
                        .Select(sa => sa.terreinObjectId)
                        .Concat(crabEntities
                            .tblTerreinObject_huisNummer_hist
                            .Where(sa => ids.Contains(sa.huisNummerId.Value))
                            .Select(sa => sa.terreinObjectId.Value))
                        .ToList()));
            }

            return terrainObjectIds;
        }

        private static List<int> IterateSqlContains(IReadOnlyCollection<int> allIds, Func<List<int>, List<int>> addRangeAction)
        {
            var filteredIds = new List<int>();
            const int sqlContainsSize = 1000;

            for (var i = 0; i < Math.Ceiling(allIds.Count / (double)sqlContainsSize); i++)
            {
                var idsInThisRange = allIds
                    .Skip(i * sqlContainsSize)
                    .Take(Math.Min(sqlContainsSize, allIds.Count - i * sqlContainsSize))
                    .ToList();

                filteredIds.AddRange(addRangeAction(idsInThisRange));
            }

            return filteredIds;
        }
    }
}
