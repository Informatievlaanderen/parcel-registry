namespace ParcelRegistry.Tests.ApiTests.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using Api.BackOffice.Abstractions.Extensions;
    using Api.Oslo.Infrastructure.Options;
    using Api.Oslo.Parcel.Sync;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed.Atom;
    using NodaTime;
    using Parcel;
    using Xunit;
    using ParcelStatus = ParcelRegistry.Legacy.ParcelStatus;

    /// <summary>
    /// The objectCrs filter selects the reference system of the embedded object only. The embedded event is
    /// always emitted exactly as the event store held it at that position. See ADR 0003.
    /// </summary>
    public class GivenObjectCrsFilter
    {
        // First vertex of GeometryHelpers.ValidGmlPolygon and of its genuine Lambert 2008 transform.
        private const double FirstXLambert72 = 140284.15;
        private const double FirstXLambert2008 = 640281.95;

        /// <summary>
        /// The object's GML carries no srsName — GmlPolygon has no such member — so the coordinates are the
        /// only evidence of which reference system the object came back in. They are read numerically because
        /// the posList is rendered at 11 decimals, where 640281.95 shows up as 640281.94999999995.
        /// </summary>
        private static double FirstXOfObjectGeometry(string feed)
        {
            var posList = Regex.Match(feed, "<posList>([^<]+)</posList>");
            posList.Success.Should().BeTrue("the feed should contain an object geometry");

            return double.Parse(posList.Groups[1].Value.Split(' ')[0], CultureInfo.InvariantCulture);
        }

        private static byte[] Lambert72Geometry()
            => GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry();

        private static byte[] Lambert2008Geometry()
            => GeometryHelpers.ValidGmlPolygonLambert2008.ToExtendedWkbGeometryLambert2008();

        private static async Task<string> WriteFeed(byte[] geometry, string? objectCrs)
        {
            var parcel = new ParcelSyndicationQueryResult(
                Guid.NewGuid(),
                1,
                "11001_A_0001_00_000",
                "ParcelWasImported",
                Instant.FromUtc(2026, 1, 1, 0, 0),
                Instant.FromUtc(2026, 1, 1, 0, 0),
                ParcelStatus.Realized,
                new List<Guid>(),
                new List<int>(),
                null,
                geometry,
                "reason",
                // The event payload carries the store's own hex, and must come out untouched whatever objectCrs says.
                $"<ParcelWasImported><ExtendedWkbGeometry>{Convert.ToHexString(geometry)}</ExtendedWkbGeometry></ParcelWasImported>");

            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);

                await writer.WriteParcel(
                    new OptionsWrapper<ResponseOptions>(new ResponseOptions { Naamruimte = "https://data.vlaanderen.be/id/perceel" }),
                    formatter,
                    "category",
                    parcel,
                    ObjectCrs.ToSrid(objectCrs));

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        [Theory]
        [InlineData("3812", SystemReferenceId.SridLambert2008)]
        [InlineData(" 3812 ", SystemReferenceId.SridLambert2008)]
        [InlineData("31370", SystemReferenceId.SridLambert72)]
        [InlineData("EPSG:3812", SystemReferenceId.SridLambert72)]
        [InlineData("nonsense", SystemReferenceId.SridLambert72)]
        [InlineData("", SystemReferenceId.SridLambert72)]
        [InlineData(null, SystemReferenceId.SridLambert72)]
        public void ThenOnlyTheExactValue3812SelectsLambert2008(string? objectCrs, int expectedSrid)
            => ObjectCrs.ToSrid(objectCrs).Should().Be(expectedSrid);

        [Fact]
        public async Task WhenNotRequested_ThenLambert72SourceIsUnchanged()
        {
            var feed = await WriteFeed(Lambert72Geometry(), objectCrs: null);

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert72SourceIsConverted()
        {
            var feed = await WriteFeed(Lambert72Geometry(), objectCrs: "3812");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert2008SourceStaysAsIs()
        {
            var feed = await WriteFeed(Lambert2008Geometry(), objectCrs: "3812");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
        }

        /// <summary>
        /// The default direction, and the one that only starts mattering once the event store is converted:
        /// a caller that does not ask keeps getting Lambert 72, so the feed's existing contract holds.
        /// </summary>
        [Fact]
        public async Task WhenNotRequested_ThenLambert2008SourceIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Geometry(), objectCrs: null);

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
        }

        [Fact]
        public async Task WhenUnrecognisedValue_ThenLambert2008SourceIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Geometry(), objectCrs: "nonsense");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
        }

        /// <summary>
        /// The embedded event is the event store's own payload and is never reprojected, even when the object
        /// beside it is.
        /// </summary>
        [Fact]
        public async Task WhenRequesting3812_ThenTheEmbeddedEventIsStillTheStoredGeometry()
        {
            var stored = Lambert72Geometry();

            var feed = await WriteFeed(stored, objectCrs: "3812");

            feed.Should().Contain(Convert.ToHexString(stored));
            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
        }
    }
}
