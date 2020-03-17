namespace ParcelRegistry.Importer.Console
{
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Crab;
    using Parcel.Commands.Crab;
    using System.Collections.Generic;
    using System.Linq;

    internal class TerrainObjectCommandsFactory
    {
        public static IEnumerable<ImportSubaddressFromCrab> CreateFor(tblSubAdres_hist subAddressHist, CaPaKey caPaKey)
            => CreateFor(new List<tblSubAdres_hist> { subAddressHist }, caPaKey);

        public static IEnumerable<ImportSubaddressFromCrab> CreateFor(IEnumerable<tblSubAdres_hist> subAddressesHist, CaPaKey caPaKey)
        {
            return subAddressesHist
                .Select(
                    subaddress =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabSubaddressId(subaddress.subAdresId.Value),
                            new CrabHouseNumberId(subaddress.huisNummerId.Value),
                            new BoxNumber(subaddress.subAdres),
                            new CrabBoxNumberType(subaddress.aardSubAdresCode),
                            new CrabLifetime(subaddress.beginDatum?.ToCrabLocalDateTime(), subaddress.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subaddress.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subaddress.Operator),
                            CrabMappings.ParseBewerking(subaddress.Bewerking),
                            CrabMappings.ParseOrganisatie(subaddress.Organisatie));
                    });
        }

        public static IEnumerable<ImportSubaddressFromCrab> CreateFor(tblSubAdres subAddress, CaPaKey caPaKey)
            => CreateFor(new List<tblSubAdres> { subAddress }, caPaKey);

        public static IEnumerable<ImportSubaddressFromCrab> CreateFor(IEnumerable<tblSubAdres> subAddresses, CaPaKey caPaKey)
        {
            return subAddresses
                .Select(
                    subAddress =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabSubaddressId(subAddress.subAdresId),
                            new CrabHouseNumberId(subAddress.huisNummerId),
                            new BoxNumber(subAddress.subAdres),
                            new CrabBoxNumberType(subAddress.aardSubAdresCode),
                            new CrabLifetime(subAddress.beginDatum.ToCrabLocalDateTime(), subAddress.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subAddress.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subAddress.Operator),
                            CrabMappings.ParseBewerking(subAddress.Bewerking),
                            CrabMappings.ParseOrganisatie(subAddress.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectHouseNumberFromCrab> CreateFor(IEnumerable<tblTerreinObject_huisNummer_hist> terreinObjectHuisNummersHist, CaPaKey caPaKey)
        {
            return terreinObjectHuisNummersHist
                .Select(
                    terreinObjectHuisNummer =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectHouseNumberFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabTerrainObjectHouseNumberId(terreinObjectHuisNummer.terreinObject_huisNummer_Id.Value),
                            new CrabTerrainObjectId(terreinObjectHuisNummer.terreinObjectId.Value),
                            new CrabHouseNumberId(terreinObjectHuisNummer.huisNummerId.Value),
                            new CrabLifetime(terreinObjectHuisNummer.beginDatum?.ToCrabLocalDateTime(), terreinObjectHuisNummer.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObjectHuisNummer.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObjectHuisNummer.Operator),
                            CrabMappings.ParseBewerking(terreinObjectHuisNummer.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObjectHuisNummer.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectHouseNumberFromCrab> CreateFor(IEnumerable<tblTerreinObject_huisNummer> terreinObjectHuisNummers, CaPaKey caPaKey)
        {
            return terreinObjectHuisNummers
                .Select(
                    terreinObjectHuisNummer =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectHouseNumberFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabTerrainObjectHouseNumberId(terreinObjectHuisNummer.terreinObject_huisNummer_Id),
                            new CrabTerrainObjectId(terreinObjectHuisNummer.terreinObjectId),
                            new CrabHouseNumberId(terreinObjectHuisNummer.huisNummerId),
                            new CrabLifetime(terreinObjectHuisNummer.beginDatum.ToCrabLocalDateTime(), terreinObjectHuisNummer.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObjectHuisNummer.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObjectHuisNummer.Operator),
                            CrabMappings.ParseBewerking(terreinObjectHuisNummer.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObjectHuisNummer.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectFromCrab> CreateFor(IEnumerable<tblTerreinObject_hist> terreinObjectsHist, CaPaKey caPaKey)
        {
            return terreinObjectsHist
                .Select(
                    terreinObject =>
                    {
                        MapLogging.Log(".");

                        return new ImportTerrainObjectFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabTerrainObjectId(terreinObject.terreinObjectId.Value),
                            new CrabIdentifierTerrainObject(terreinObject.identificatorTerreinObject),
                            new CrabTerrainObjectNatureCode(terreinObject.aardTerreinObjectCode),
                            terreinObject.x_coordinaat.HasValue ? new CrabCoordinate(terreinObject.x_coordinaat.Value) : null,
                            terreinObject.y_coordinaat.HasValue ? new CrabCoordinate(terreinObject.y_coordinaat.Value) : null,
                            new CrabBuildingNature(terreinObject.aardGebouw),
                            new CrabLifetime(terreinObject.beginDatum?.ToCrabLocalDateTime(), terreinObject.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObject.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObject.Operator),
                            CrabMappings.ParseBewerking(terreinObject.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObject.Organisatie));
                    });
        }

        public static IEnumerable<ImportTerrainObjectFromCrab> CreateFor(IEnumerable<tblTerreinObject> terreinObjects, CaPaKey caPaKey)
        {
            return terreinObjects
                .Select(
                    terreinObject =>
                    {
                        MapLogging.Log(".");
                        return new ImportTerrainObjectFromCrab(
                            new VbrCaPaKey(caPaKey.VbrCaPaKey),
                            new CrabTerrainObjectId(terreinObject.terreinObjectId),
                            new CrabIdentifierTerrainObject(terreinObject.identificatorTerreinObject),
                            new CrabTerrainObjectNatureCode(terreinObject.aardTerreinObjectCode),
                            terreinObject.x_coordinaat.HasValue
                                ? new CrabCoordinate(terreinObject.x_coordinaat.Value)
                                : null,
                            terreinObject.y_coordinaat.HasValue
                                ? new CrabCoordinate(terreinObject.y_coordinaat.Value)
                                : null,
                            new CrabBuildingNature(terreinObject.aardGebouw),
                            new CrabLifetime(terreinObject.beginDatum.ToCrabLocalDateTime(), terreinObject.eindDatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(terreinObject.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(terreinObject.Operator),
                            CrabMappings.ParseBewerking(terreinObject.Bewerking),
                            CrabMappings.ParseOrganisatie(terreinObject.Organisatie));
                    });
        }
    }
}
