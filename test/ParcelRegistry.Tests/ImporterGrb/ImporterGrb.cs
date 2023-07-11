namespace ParcelRegistry.Tests.ImporterGrb
{
    using System.Collections.Generic;
    using System.Threading;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Importer.Grb;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using MediatR;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class ImporterGrb : ParcelRegistryTest
    {
        public ImporterGrb(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void GivenDerived()
        {
            var capakey1 = CaPaKey.CreateFrom(Fixture.Create<string>());

            var request = new List<GrbParcelRequest>()
            {
                new GrbAddParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 9)),
                new GrbDeleteParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 9)),
                new GrbUpdateParcelRequest(new GrbParcel(capakey1, GeometryHelpers.ValidPolygon, 9))
            };

            var mediator = new Mock<IMediator>();

            foreach (var r in request)
            {
                mediator.Object.Send(r);
            }

            mediator.Verify(x => x.Send(It.IsAny<GrbAddParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(x => x.Send(It.IsAny<GrbUpdateParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            mediator.Verify(x => x.Send(It.IsAny<GrbDeleteParcelRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
