namespace ParcelRegistry.Tests.AggregateTests.WhenChangingParcelGeometry
{
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Parcel;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelDoesNotExist : ParcelRegistryTest
    {
        public GivenParcelDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenParcelWasImported()
        {
            var command = new ChangeParcelGeometryBuilder(Fixture)
                .Build();

            Assert(new Scenario()
                .Given()
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasImported(command.ParcelId, command.VbrCaPaKey,  GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry())));
        }
    }
}
