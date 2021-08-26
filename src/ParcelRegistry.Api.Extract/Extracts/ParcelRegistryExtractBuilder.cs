namespace ParcelRegistry.Api.Extract.Extracts
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;

    public class ParcelRegistryExtractBuilder
    {
        public static ExtractFile CreateParcelFiles(ExtractContext context)
        {
            var extractItems = context
                .ParcelExtract
                .AsNoTracking();

            return ExtractBuilder.CreateDbfFile<ParcelDbaseRecord>(
                ExtractController.ZipName,
                new ParcelDbaseSchema(),
                extractItems.OrderBy(m => m.CaPaKey).Select(org => org.DbaseRecord),
                extractItems.Count);
        }
    }
}
