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
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNotCompletedRunHistory : ParcelRegistryTest
    {
        private readonly ImporterContext _fakeImporterContext;

        public GivenNotCompletedRunHistory(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeImporterContext = new FakeImportParcelContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task WhenContinuingRunHistory_ThenOnlyAddUnProcessedRequests()
        {
            var mockImporterContext = new Mock<IImporterContext>();
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IUniqueParcelPlanProxy>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var incompleteRunHistory = new RunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));

            mockImporterContext
                .Setup(x => x.GetLatestRunHistory())
                .ReturnsAsync(incompleteRunHistory);

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var alreadyExecutedRequest = new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9));
            var requests = new List<ParcelRequest>
            {
                alreadyExecutedRequest,
                new RetireParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 10)),
                new ChangeParcelGeometryRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 11))
            };

            mockImporterContext
                .Setup(x => x.ProcessedRequestExists(It.Is<string>(x => x == alreadyExecutedRequest.GetSHA256())))
                .ReturnsAsync(true);

            var lastRunHistory = await _fakeImporterContext.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await _fakeImporterContext.CompleteRunHistory(lastRunHistory.Id);

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, Stream>>()))
                .Returns(requests);

            var sut = new Importer(
                mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                mockImporterContext.Object,
                mockNotificationService.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockIUniqueParcelPlanProxy.Verify(x => x.GetMaxDate(), Times.Never);

            mockImporterContext.Verify(x => x.AddProcessedRequest(It.IsAny<string>()), Times.Exactly(2));
            mockImporterContext.Verify(x => x.CompleteRunHistory(incompleteRunHistory.Id), Times.Once);
            mockImporterContext.Verify(x => x.ClearProcessedRequests(), Times.Once);
        }

        [Fact]
        public async Task ThenCompleteLastRunHistory()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IUniqueParcelPlanProxy>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());

            var alreadyExecutedRequest = new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9));
            var requests = new List<ParcelRequest>
            {
                alreadyExecutedRequest,
                new RetireParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 10)),
                new ChangeParcelGeometryRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 11))
            };

            var lastRunHistory = await _fakeImporterContext.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, Stream>>()))
                .Returns(requests);

            await _fakeImporterContext.AddProcessedRequest(alreadyExecutedRequest.GetSHA256());

            var sut = new Importer(mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _fakeImporterContext,
                mockNotificationService.Object);

            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ImportParcelRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ChangeParcelGeometryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<RetireParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            mockIUniqueParcelPlanProxy.Verify(x => x.GetMaxDate(), Times.Never);

            var lastRun = await _fakeImporterContext.GetLatestRunHistory();
            lastRun.Id.Should().Be(1);
            lastRun.Completed.Should().BeTrue();
            lastRun.FromDate.Should().Be(lastRunHistory.FromDate);
            lastRun.ToDate.Should().Be(lastRun.ToDate);
        }
    }
}
