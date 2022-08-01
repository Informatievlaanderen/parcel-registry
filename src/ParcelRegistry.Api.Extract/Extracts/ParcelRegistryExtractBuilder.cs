namespace ParcelRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.ParcelExtract;

    public static class ParcelRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateParcelFiles(ExtractContext context)
        {
            var extractItems = context
                .ParcelExtract
                .AsNoTracking();

            var parcelProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(ParcelExtractProjections).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, parcelProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<ParcelDbaseRecord>(
                ExtractController.ZipName,
                new ParcelDbaseSchema(),
                extractItems.OrderBy(m => m.CaPaKey).Select(org => org.DbaseRecord),
                extractItems.Count);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractController.ZipName,
                extractMetadata);
        }
    }
}
