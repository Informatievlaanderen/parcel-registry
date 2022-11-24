namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Parcel.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using Fixtures;
    using Parcel;
    using Projections.Legacy.ParcelDetailV2;
    using Xunit;

    public class ParcelDetailItemV2Tests : ParcelLegacyProjectionTest<ParcelDetailV2Projections>
    {
        private readonly Fixture? _fixture;

        public ParcelDetailItemV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithParcelStatus());
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var parcelWasMigrated = _fixture.Create<ParcelWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<ParcelWasMigrated>(new Envelope(parcelWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var parcelDetailV2 = await ct.ParcelDetailV2.FindAsync(parcelWasMigrated.ParcelId);
                    parcelDetailV2.Should().NotBeNull();
                    parcelDetailV2!.CaPaKey.Should().Be(parcelWasMigrated.CaPaKey);
                    parcelDetailV2.Status.Should().Be(ParcelStatus.Parse(parcelWasMigrated.ParcelStatus));
                    parcelDetailV2.Removed.Should().Be(parcelWasMigrated.IsRemoved);
                    parcelDetailV2.Addresses.Should().BeEquivalentTo(
                        parcelWasMigrated.AddressPersistentLocalIds.Select(x => new ParcelDetailAddressV2(parcelWasMigrated.ParcelId, x)));
                    parcelDetailV2.VersionTimestamp.Should().Be(parcelWasMigrated.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(parcelWasMigrated.GetHash());
                });
        }

        protected override ParcelDetailV2Projections CreateProjection()
            => new ParcelDetailV2Projections();
    }
}
