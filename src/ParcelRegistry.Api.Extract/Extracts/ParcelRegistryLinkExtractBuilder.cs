namespace ParcelRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Handlers;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.ParcelLinkExtract;

    public class ParcelRegistryLinkExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateParcelFiles(ExtractContext context)
        {
            var extractItems = context
                .ParcelLinkExtractWithCount
                .AsNoTracking();

            var parcelProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(ParcelLinkExtractProjections).FullName);

            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, parcelProjectionState.Position.ToString()}
            };

            yield return ExtractBuilder.CreateDbfFile<ParcelLinkDbaseRecord>(
                ExtractFileNames.ParcelLinkExtractZipName,
                new ParcelLinkDbaseSchema(),
                extractItems.OrderBy(m => m.CaPaKey).Select(org => org.DbaseRecord),
                extractItems.Count);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.ParcelLinkExtractZipName,
                extractMetadata);
        }
    }
}
