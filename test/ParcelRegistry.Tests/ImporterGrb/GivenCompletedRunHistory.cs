﻿namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using FluentAssertions;
    using Importer.Grb;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using Importer.Grb.Infrastructure.Download;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCompletedRunHistory : ParcelRegistryTest
    {
        private readonly  FakeImportParcelContextFactory _importerContextFactory;

        public GivenCompletedRunHistory(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _importerContextFactory = new FakeImportParcelContextFactory(false);
        }

        [Fact]
        public async Task ThenStartNewRunHistory_AndClearProcessedRequests()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IDownloadFacade>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());

            var requests = new List<ParcelRequest>
            {
                new RetireParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 10,  new DateTime(2023, 01, 01))),
                new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9, new DateTime(2023, 01, 02))),
                new ChangeParcelGeometryRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 11,  new DateTime(2023, 01, 02)))
            };

            var today = DateTime.Now;

            var context = _importerContextFactory.CreateDbContext();

            var lastRunHistory = await context.AddRunHistory(DateTimeOffset.Now.AddDays(-3), DateTimeOffset.Now.AddDays(-2));
            await context.CompleteRunHistory(lastRunHistory.Id);

            mockIUniqueParcelPlanProxy.Setup(x => x.GetMaxDate())
                .ReturnsAsync(today);

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, Stream>>()))
                .Returns(requests);

            var sut = new Importer(
                mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _importerContextFactory,
                mockNotificationService.Object,
                Mock.Of<IHostApplicationLifetime>());

            // Assert Order
            int callOrder = 0;
            mockMediator
                .Setup(x => x.Send<ParcelRequest>(It.IsAny<RetireParcelRequest>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    callOrder.Should().Be(0);
                    callOrder++;
                });
            mockMediator
                .Setup(x => x.Send<ParcelRequest>(It.IsAny<ImportParcelRequest>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    callOrder.Should().Be(1);
                    callOrder++;
                });
            mockMediator
                .Setup(x => x.Send<ParcelRequest>(It.IsAny<ChangeParcelGeometryRequest>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    callOrder.Should().Be(2);
                    callOrder++;
                });

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ImportParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<ChangeParcelGeometryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send<ParcelRequest>(It.IsAny<RetireParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            var processedRequests = await context.ProcessedRequests.ToListAsync();
            processedRequests.Should().HaveCount(0);

            var lastRun = await context.GetLatestRunHistory();
            lastRun.Id.Should().Be(2);
            lastRun.Completed.Should().BeTrue();
            lastRun.FromDate.Should().Be(lastRunHistory.ToDate);
            lastRun.ToDate.Should().Be(today);
        }

        [Fact]
        public async Task WhenNewToDateEqualsFromDate_ThenThrowsOrderInvalidDateRangeException()
        {
            var uniqueParcelPlanProxy = new Mock<IDownloadFacade>();

            var today = DateTimeOffset.Now;

            var context = _importerContextFactory.CreateDbContext();
            var lastRunHistory = await context.AddRunHistory(DateTimeOffset.Now.AddDays(-3), today);
            await context.CompleteRunHistory(lastRunHistory.Id);

            uniqueParcelPlanProxy.Setup(x => x.GetMaxDate()).ReturnsAsync(today.DateTime);

            var sut = new Importer(
                Mock.Of<IMediator>(),
                uniqueParcelPlanProxy.Object,
                Mock.Of<IZipArchiveProcessor>(),
                Mock.Of<IRequestMapper>(),
                _importerContextFactory,
                Mock.Of<INotificationService>(),
                Mock.Of<IHostApplicationLifetime>());

            // Act
            var act = () => sut.StartAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<OrderInvalidDateRangeException>();
        }
    }
}
