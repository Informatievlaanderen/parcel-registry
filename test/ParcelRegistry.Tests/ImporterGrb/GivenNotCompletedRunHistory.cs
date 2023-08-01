namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using FluentAssertions;
    using Importer.Grb;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using Importer.Grb.Infrastructure.Download;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNotCompletedRunHistory : ParcelRegistryTest
    {
        private readonly  FakeImportParcelContextFactory _fakeImporterContextFactory;

        public GivenNotCompletedRunHistory(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeImporterContextFactory = new FakeImportParcelContextFactory(false);
        }

        [Fact]
        public async Task ThenCompleteLastRunHistory()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IDownloadFacade>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());

            var alreadyExecutedRequest = new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9, DateTime.Now));
            var requests = new List<ParcelRequest>
            {
                alreadyExecutedRequest,
                new RetireParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 10,  DateTime.Now)),
                new ChangeParcelGeometryRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 11,  DateTime.Now))
            };

            var context = _fakeImporterContextFactory.CreateDbContext();

            var lastRunHistory = await context.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, Stream>>()))
                .Returns(requests);

            await context.AddProcessedRequest(alreadyExecutedRequest.Hash);

            var sut = new Importer(mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _fakeImporterContextFactory,
                mockNotificationService.Object);

            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ImportParcelRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ChangeParcelGeometryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<RetireParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            mockIUniqueParcelPlanProxy.Verify(x => x.GetMaxDate(), Times.Never);

            var lastRun = await context.GetLatestRunHistory();
            lastRun.Id.Should().Be(1);
            lastRun.Completed.Should().BeTrue();
            lastRun.FromDate.Should().Be(lastRunHistory.FromDate);
            lastRun.ToDate.Should().Be(lastRun.ToDate);
        }
    }
}
