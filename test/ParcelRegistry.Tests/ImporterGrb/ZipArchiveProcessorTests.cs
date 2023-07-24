namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using FluentAssertions;
    using Importer.Grb.Infrastructure.Download;
    using Xunit;

    public class ZipArchiveProcessorTests
    {
        [Fact]
        public void GivenZipFile_ThenReadEntries()
        {
            using var zipFile = new FileStream($"{AppContext.BaseDirectory}/ImporterGrb/grb_download_file.zip", FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read, false);

            var result = new ZipArchiveProcessor().Open(archive);

            result.Count.Should().Be(3);
        }
    }
}
