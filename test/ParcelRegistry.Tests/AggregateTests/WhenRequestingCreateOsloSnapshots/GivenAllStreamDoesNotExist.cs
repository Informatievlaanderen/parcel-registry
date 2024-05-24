namespace ParcelRegistry.Tests.AggregateTests.WhenRequestingCreateOsloSnapshots
{
    using System.Linq;
    using AllStream;
    using AllStream.Commands;
    using AllStream.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAllStreamDoesNotExist : ParcelRegistryTest
    {
        public GivenAllStreamDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public void ThenParcelOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [new VbrCaPaKey("11001B0001-00X000"), new VbrCaPaKey("11001B0001-00S000")],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .When(command)
                .Then(AllStreamId.Instance,
                    new ParcelOsloSnapshotsWereRequested(
                        command.CaPaKeys.ToDictionary(ParcelId.CreateFor, x => x))));
        }
    }
}
