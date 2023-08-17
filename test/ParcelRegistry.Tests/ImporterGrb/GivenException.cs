namespace ParcelRegistry.Tests.ImporterGrb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenException : ParcelRegistryTest
    {
        private readonly  FakeImportParcelContextFactory _fakeImporterContextFactory;

        public GivenException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeImporterContextFactory = new FakeImportParcelContextFactory(false);
        }

        [Fact]
        public async Task InMediator_ThenSendNotificationAndThrowException()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IDownloadFacade>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());

            var requests = new List<ParcelRequest>
            {
                new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9, DateTime.Now))
            };

            var today = DateTime.Now;

            var context = _fakeImporterContextFactory.CreateDbContext();

            var lastRunHistory = await context.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await context.CompleteRunHistory(lastRunHistory.Id);

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
                _fakeImporterContextFactory,
                mockNotificationService.Object,
                Mock.Of<IHostApplicationLifetime>(),
                NullLoggerFactory.Instance);

            // Act
            var act = async () => await sut.StartAsync(CancellationToken.None);

            // Assert

            var expectedExceptionMessage =
                $"Exception for parcel: {caPaKey.VbrCaPaKey}, {typeof(Exception)} {Environment.NewLine} Parcel hash: {requests.First().Hash}";
            act.Should().ThrowAsync<Exception>().Result
                .Where(x => x.Message == expectedExceptionMessage);

            mockNotificationService.Verify(x => x.PublishToTopicAsync(
                It.Is<NotificationMessage>(y => y.BasisregistersError == expectedExceptionMessage)));
        }

        [Fact]
        public async Task InUniqueParcelPlanProxy_ThenSendNotificationAndThrowException()
        {
            var mockMediator = new Mock<IMediator>();
            var mockIUniqueParcelPlanProxy = new Mock<IDownloadFacade>();
            var mockZipArchiveProcessor = new Mock<IZipArchiveProcessor>();
            var mockRequestMapper = new Mock<IRequestMapper>();
            var mockNotificationService = new Mock<INotificationService>();

            var context = _fakeImporterContextFactory.CreateDbContext();

            var lastRunHistory = await context.AddRunHistory(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1));
            await context.CompleteRunHistory(lastRunHistory.Id);

            mockIUniqueParcelPlanProxy.Setup(x => x.GetMaxDate())
                .Throws(new Exception("Something went wrong"));

            var sut = new Importer(
                mockMediator.Object,
                mockIUniqueParcelPlanProxy.Object,
                mockZipArchiveProcessor.Object,
                mockRequestMapper.Object,
                _fakeImporterContextFactory,
                mockNotificationService.Object,
                Mock.Of<IHostApplicationLifetime>(),
                NullLoggerFactory.Instance);

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
