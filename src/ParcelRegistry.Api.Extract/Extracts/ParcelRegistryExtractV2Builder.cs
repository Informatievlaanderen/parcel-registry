namespace ParcelRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Handlers;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.ParcelExtract;

    public static class ParcelRegistryExtractV2Builder
    {
        public static IEnumerable<ExtractFile> CreateParcelFiles(ExtractContext context)
        {
            var extractItems = context
                .ParcelExtractV2
                .AsNoTracking();

            var parcelProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(ParcelExtractV2Projections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, parcelProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<ParcelDbaseRecord>(
                ExtractFileNames.ParcelExtractZipName,
                new ParcelDbaseSchema(),
                extractItems.OrderBy(m => m.CaPaKey).Select(org => org.DbaseRecord),
                extractItems.Count);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.ParcelExtractZipName,
                extractMetadata);
        }
    }
}
