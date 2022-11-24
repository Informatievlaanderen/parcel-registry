namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Parcel.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using Fixtures;
    using Projections.Legacy.ParcelSyndication;
    using Xunit;

    public class ParcelSyndicationV2Tests : ParcelLegacyProjectionTest<ParcelSyndicationProjections>
    {
        private readonly Fixture? _fixture;

        public ParcelSyndicationV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            const long position = 1L;
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, nameof(ParcelWasMigrated) },
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var parcelSyndicationItem =
                        await ct.ParcelSyndication.FindAsync(position);
                    parcelSyndicationItem.Should().NotBeNull();
                    parcelSyndicationItem!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    parcelSyndicationItem.Status.Should().Be(ParcelRegistry.Legacy.ParcelStatus.Parse(parcelWasMigrated.ParcelStatus));
                    parcelSyndicationItem.XCoordinate.Should().Be(parcelWasMigrated.XCoordinate);
                    parcelSyndicationItem.YCoordinate.Should().Be(parcelWasMigrated.YCoordinate);
                    parcelSyndicationItem.AddressPersistentLocalIds.Should().BeEquivalentTo(parcelWasMigrated.AddressPersistentLocalIds);
                    parcelSyndicationItem.ChangeType.Should().Be(nameof(ParcelWasMigrated));
                    parcelSyndicationItem.EventDataAsXml.Should().NotBeEmpty();
                    parcelSyndicationItem.RecordCreatedAt.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelSyndicationItem.LastChangedOn.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                });
        }

        protected override ParcelSyndicationProjections CreateProjection()
            => new ParcelSyndicationProjections();
    }
}
