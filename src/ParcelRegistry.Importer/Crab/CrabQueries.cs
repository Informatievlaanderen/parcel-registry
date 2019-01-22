namespace ParcelRegistry.Importer.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.CrabModel;

    public static class CrabQueries
    {
        private const string AardPerceel = "1";
        private static readonly string DeletedBewerking = CrabBewerking.Verwijdering.Code;

        public static List<string> GetChangedPerceelIdsBetween(DateTime since, DateTime until)
        {
            if (since == DateTime.MinValue)
            {
                using (var crabEntities = new CRABEntities())
                {
                    crabEntities.Database.CommandTimeout = 10 * 60;
                    var odb = crabEntities.tblTerreinObject.Where(to => to.aardTerreinObjectCode == AardPerceel);
                    var cdb = crabEntities.tblTerreinObject_hist.Where(to => to.aardTerreinObjectCode == AardPerceel);

                    return odb
                        .GroupBy(t => t.identificatorTerreinObject)
                        .Select(t => new {t.Key, beginTijd = t.Min(o => o.beginTijd)})
                        .Concat(cdb
                            .GroupBy(t => t.identificatorTerreinObject)
                            .Select(t => new {t.Key, beginTijd = t.Min(o => o.beginTijd.Value)}))
                        .GroupBy(s => s.Key)
                        .Select(s => new {s.Key, beginTijd = s.Min(o => o.beginTijd)})
                        .Where(s => s.beginTijd <= until)
                        .Select(s => s.Key)
                        .ToList();
                }
            }

            var tasks = new List<Task<List<int>>>();

            tasks.Add(Task.Run(() =>
            {
                var terrainObjectIds = new List<int>();
                using (var crabEntities = new CRABEntities())
                {
                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject
                        .Where(to => to.aardTerreinObjectCode == AardPerceel)
                        .Where(to => to.beginTijd > since && to.beginTijd <= until)
                        .Take(5)
                        .Select(to => to.terreinObjectId));

                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject_hist
                        .Where(to => to.aardTerreinObjectCode == AardPerceel)
                        .Where(to => to.beginTijd > since && to.beginTijd <= until)
                        .Take(5)
                        .Select(to => to.terreinObjectId.Value));

                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject_hist
                        .Where(to => to.aardTerreinObjectCode == "1")
                        .Where(to =>
                            to.eindTijd > since && to.eindTijd <= until && to.eindBewerking == DeletedBewerking)
                        .Select(to => to.terreinObjectId.Value));
                }

                return terrainObjectIds;
            }));

            tasks.Add(Task.Run(() =>
            {
                var terrainObjectIds = new List<int>();
                using (var crabEntities = new CRABEntities())
                {

                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject_huisNummer
                        .Where(thn => thn.tblTerreinObject.aardTerreinObjectCode == "1")
                        .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                        .Select(hnr => hnr.terreinObjectId)
                        .ToList());

                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject_huisNummer_hist
                        .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                        .Select(hnr => hnr.terreinObjectId.Value)
                        .ToList());

                    terrainObjectIds.AddRange(crabEntities.tblTerreinObject_huisNummer_hist
                        .Where(to => to.eindTijd > since && to.eindTijd <= until && to.eindBewerking == DeletedBewerking)
                        .Select(to => to.terreinObjectId.Value));
                }

                return terrainObjectIds;
            }));

            tasks.Add(Task.Run(() =>
            {
                var terrainObjectIds = new List<int>();
                using (var crabEntities = new CRABEntities())
                {
                    var huisNummerIdsSubadres = crabEntities.tblSubAdres
                        .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                        .Select(hnr => hnr.huisNummerId)
                        .ToList();

                    terrainObjectIds.AddRange(IterateSqlContains(huisNummerIdsSubadres, (idsInRange, filteredIds) =>
                    {
                        filteredIds.AddRange(
                            crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId))
                                .Select(sa => sa.terreinObjectId)
                                .Concat(crabEntities.tblTerreinObject_huisNummer_hist
                                    .Where(sa => idsInRange.Contains(sa.huisNummerId.Value))
                                    .Select(sa => sa.terreinObjectId.Value))
                                .ToList());
                    }));

                    var huisNummerIdsSubadresHist = crabEntities.tblSubAdres_hist
                        .Where(crabRecord => crabRecord.beginTijd > since && crabRecord.beginTijd <= until)
                        .Select(hnr => hnr.huisNummerId.Value)
                        .ToList();

                    huisNummerIdsSubadresHist.AddRange(crabEntities.tblSubAdres_hist
                        .Where(crabRecord => crabRecord.eindTijd > since && crabRecord.eindTijd <= until &&
                                             crabRecord.eindBewerking == DeletedBewerking)
                        .Select(hnr => hnr.huisNummerId.Value)
                        .ToList());

                    terrainObjectIds.AddRange(IterateSqlContains(huisNummerIdsSubadresHist, (idsInRange, filteredIds) =>
                    {
                        filteredIds.AddRange(
                            crabEntities.tblTerreinObject_huisNummer.Where(sa => idsInRange.Contains(sa.huisNummerId))
                                .Select(sa => sa.terreinObjectId)
                                .Concat(crabEntities.tblTerreinObject_huisNummer_hist
                                    .Where(sa => idsInRange.Contains(sa.huisNummerId.Value))
                                    .Select(sa => sa.terreinObjectId.Value))
                                .ToList());
                    }));
                }
                return terrainObjectIds;
            }));

            Task.WaitAll(tasks.ToArray());

            var allTerrainObjectIds = tasks.SelectMany(x => x.Result).ToList();
            var perceelIds = new List<string>();
            using (var crabEntities = new CRABEntities())
            {
                var sqlContainsSize = 1000;
                for (int i = 0; i < Math.Ceiling(allTerrainObjectIds.Count / (double)sqlContainsSize); i++)
                {
                    var idsInThisRange = allTerrainObjectIds.Skip(i * sqlContainsSize).Take(Math.Min(sqlContainsSize, allTerrainObjectIds.Count - i * sqlContainsSize));

                    perceelIds.AddRange(crabEntities.tblTerreinObject.Where(t => t.aardTerreinObjectCode == AardPerceel && idsInThisRange.Contains(t.terreinObjectId)).Select(t => t.identificatorTerreinObject));
                    perceelIds.AddRange(crabEntities.tblTerreinObject_hist.Where(t => t.aardTerreinObjectCode == AardPerceel && idsInThisRange.Contains(t.terreinObjectId.Value)).Select(t => t.identificatorTerreinObject));
                }

                return perceelIds.Distinct().ToList();
            }
        }

        private static List<int> IterateSqlContains(IReadOnlyCollection<int> allIds, Action<List<int>, List<int>> addRangeAction)
        {
            var filteredIds = new List<int>();
            const int sqlContainsSize = 1000;
            for (var i = 0; i < Math.Ceiling(allIds.Count / (double)sqlContainsSize); i++)
            {
                var idsInThisRange = allIds
                    .Skip(i * sqlContainsSize)
                    .Take(Math.Min(sqlContainsSize, allIds.Count - i * sqlContainsSize))
                    .ToList();

                addRangeAction(idsInThisRange, filteredIds);
            }

            return filteredIds;
        }
    }
}
