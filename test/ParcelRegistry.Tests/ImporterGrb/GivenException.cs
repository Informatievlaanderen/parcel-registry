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

    public class GivenException : ParcelRegistryTest
    {
        private readonly FakeImportParcelContext _fakeImporterContext;

        public GivenException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeImporterContext = new FakeImportParcelContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task InMediator_ThenSendNotificationAndThrowException()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IUniqueParcelPlanProxy>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());

            var requests = new List<ParcelRequest>
            {
                new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9))
            };

            var today = DateTime.Now;

            var lastRunHistory = await _fakeImporterContext.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await _fakeImporterContext.CompleteRunHistory(lastRunHistory.Id);

            mockIUniqueParcelPlanProxy.Setup(x => x.GetMaxDate())
                .ReturnsAsync(today);

            mockRequestMapper.Setup(x => x.Map(It.IsAny<Dictionary<GrbParcelActions, Stream>>()))
                .Returns(requests);

            mockMediator.Setup(x => x.Send(It.IsAny<ParcelRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var sut = new Importer(
                mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _fakeImporterContext,
                mockNotificationService.Object);

            // Act
            var act = async () => await sut.StartAsync(CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().Result
                .Where(x => x.Message == $"Exception for parcel: {caPaKey.VbrCaPaKey}, {typeof(Exception)}");

            mockNotificationService.Verify(x => x.PublishToTopicAsync(
                It.Is<NotificationMessage>(y => y.BasisregistersError == $"Exception for parcel: {caPaKey.VbrCaPaKey}, {typeof(Exception)}")));
        }

        [Fact]
        public async Task InUniqueParcelPlanProxy_ThenSendNotificationAndThrowException()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IUniqueParcelPlanProxy>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var lastRunHistory = await _fakeImporterContext.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await _fakeImporterContext.CompleteRunHistory(lastRunHistory.Id);

            mockIUniqueParcelPlanProxy.Setup(x => x.GetMaxDate())
                .Throws(new Exception("Something went wrong"));

            var sut = new Importer(
                mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _fakeImporterContext,
                mockNotificationService.Object);

            // Act
            var act = async () => await sut.StartAsync(CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().Result
                .Where(x => x.Message == "Something went wrong");

            mockNotificationService.Verify(x => x.PublishToTopicAsync(
                It.Is<NotificationMessage>(y => y.BasisregistersError == "Something went wrong")));
        }
    }
}
