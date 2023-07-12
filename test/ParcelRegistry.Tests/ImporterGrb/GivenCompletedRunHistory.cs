﻿namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using FluentAssertions;
    using Importer.Grb;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class ImporterGrbTests : ParcelRegistryTest
    {
        private readonly ImporterContext _fakeImporterContext;

        public ImporterGrbTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeImporterContext = new FakeImportParcelContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task Given()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IUniqueParcelPlanProxy>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();

            var capakey1 = CaPaKey.CreateFrom(Fixture.Create<string>());

            var requests = new List<GrbParcelRequest>
            {
                new GrbAddParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 9)),
                new GrbDeleteParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 10)),
                new GrbUpdateParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 11))
            };

            var today = DateTimeOffset.Now;

            var lastRunHistory = await _fakeImporterContext.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await _fakeImporterContext.CompleteRunHistory(lastRunHistory.Id);

            mockIUniqueParcelPlanProxy.Setup(x => x.GetMaxDate())
                .Returns(today);

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, FileStream>>()))
                .Returns(requests);

            var sut = new Importer(mockMediator.Object, mockIUniqueParcelPlanProxy.Object, mockZipArchiveProcessor.Object, mockRequestMapper.Object, _fakeImporterContext);

            await sut.StartAsync(CancellationToken.None);

            mockMediator.Verify(x => x.Send(It.IsAny<GrbAddParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send(It.IsAny<GrbUpdateParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify(x => x.Send(It.IsAny<GrbDeleteParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            var processedRequests = await _fakeImporterContext.ProcessedRequests.ToListAsync();
            processedRequests.Should().HaveCount(3);
            processedRequests.FirstOrDefault(x => x.SHA256 == requests[0].GetSHA256())
                .Should().NotBeNull();
            processedRequests.FirstOrDefault(x => x.SHA256 == requests[1].GetSHA256())
                .Should().NotBeNull();
            processedRequests.FirstOrDefault(x => x.SHA256 == requests[2].GetSHA256())
                .Should().NotBeNull();

            var lastRun = await _fakeImporterContext.GetLatestRunHistory();
            lastRun.Id.Should().Be(2);
            lastRun.Completed.Should().BeTrue();
            lastRun.FromDate.Should().Be(lastRunHistory.ToDate);
            lastRun.ToDate.Should().Be(today);
        }
    }
}
