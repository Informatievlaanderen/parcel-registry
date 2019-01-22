namespace ParcelRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExtractFiles;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Projections.Extract;
    using Projections.Extract.ParcelExtract;

    public class ParcelRegistryExtractBuilder
    {
        public static ExtractFile CreateParcelFile(IReadOnlyCollection<ParcelExtractItem> parcels)
        {
            return ExtractBuilder.CreateDbfFile<ParcelDbaseRecord>(
                ExtractController.ZipName,
                new ParcelDbaseSchema(),
                parcels
                    .Select(org => org.DbaseRecord)
                    .ToArray());
        }
    }

    public class ExtractBuilder
    {
        public static ExtractFile CreateDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, IReadOnlyCollection<byte[]> records)
            where TDbaseRecord : DbaseRecord, new()
        {
            var dbfFile = CreateEmptyDbfFile<TDbaseRecord>(
                fileName,
                schema,
                new DbaseRecordCount(records.Count));

            dbfFile.WriteBytesAs<TDbaseRecord>(records);

            return dbfFile;
        }

        private static DbfFile<TDbaseRecord> CreateEmptyDbfFile<TDbaseRecord>(string fileName, DbaseSchema schema, DbaseRecordCount recordCount)
            where TDbaseRecord : DbaseRecord
        {
            return new DbfFile<TDbaseRecord>(
                fileName,
                new DbaseFileHeader(
                    DateTime.Now,
                    DbaseCodePage.None, // TODO: this is the same code page as the old test files, to evaluate if this is important
                    recordCount,
                    schema));
        }
    }
}
