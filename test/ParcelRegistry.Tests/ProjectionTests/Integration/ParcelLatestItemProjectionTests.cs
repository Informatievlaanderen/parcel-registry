namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Parcel;
    using Parcel.Events;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.ParcelLatestItemV2;
    using Xunit;

    public partial class ParcelLatestItemProjectionTests : IntegrationProjectionTest<ParcelLatestItemV2Projections>
    {
        private readonly Fixture _fixture;
        private const string Namespace = "https://data.vlaanderen.be/id/perceel";

        public ParcelLatestItemProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            _fixture.Customize(new WithParcelStatus());
            _fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public async Task GivenParcelWasMigrated()
        {
            var message = _fixture.Create<ParcelWasMigrated>();

            await Sut.Given(message)
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.CaPaKey.Should().Be(message.CaPaKey);
                    latestItem.Status.Should().Be(ParcelStatus.Parse(message.ParcelStatus));
                    latestItem.OsloStatus.Should().BeOneOf("Gerealiseerd", "Gehistoreerd");
                    latestItem.Namespace.Should().Be(Namespace);
                    latestItem.Puri.Should().Be($"{Namespace}/{message.CaPaKey}");
                    latestItem.IsRemoved.Should().Be(message.IsRemoved);
                    latestItem.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    latestItem.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    latestItem.Geometry.Should().BeEquivalentTo(geometry);

                    foreach (var addressPersistentLocalId in message.AddressPersistentLocalIds)
                    {
                        await context
                            .ParcelLatestItemV2Addresses
                            .FindAsync(message.ParcelId, addressPersistentLocalId);
                        latestItem.Should().NotBeNull();
                        latestItem.CaPaKey.Should().Be(message.CaPaKey);
                    }
                });
        }

        [Fact]
        public async Task GivenParcelIsImported()
        {
            var message = _fixture.Create<ParcelWasImported>();

            await Sut.Given(message)
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.CaPaKey.Should().Be(message.CaPaKey);
                    latestItem.Status.Should().Be(ParcelStatus.Realized);
                    latestItem.OsloStatus.Should().Be("Gerealiseerd");
                    latestItem.Namespace.Should().Be(Namespace);
                    latestItem.Puri.Should().Be($"{Namespace}/{message.CaPaKey}");
                    latestItem.IsRemoved.Should().BeFalse();
                    latestItem.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    latestItem.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    latestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task GivenParcelGeometryWasChanged()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var message = new ParcelGeometryWasChanged(
                new ParcelId(parcelWasImported.ParcelId),
                new VbrCaPaKey(parcelWasImported.CaPaKey),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            ((ISetProvenance)message).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(parcelWasImported, message)
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    latestItem.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    latestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task GivenParcelWasCorrectedFromRetiredToRealized()
        {
            var message = _fixture.Create<ParcelWasCorrectedFromRetiredToRealized>();

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), message)
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.Status.Should().Be(ParcelStatus.Realized);
                    latestItem.OsloStatus.Should().Be("Gerealiseerd");
                    latestItem.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    latestItem.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    latestItem.Geometry.Should().BeEquivalentTo(geometry);
                });
        }

        [Fact]
        public async Task GivenParcelWasRetiredV2()
        {
            var message = _fixture.Create<ParcelWasRetiredV2>();

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.Status.Should().Be(ParcelStatus.Retired);
                    latestItem.OsloStatus.Should().Be("Gehistoreerd");
                    latestItem.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    latestItem.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());

                });
        }

        [Fact]
        public async Task GivenParcelAddressWasAttachedV2()
        {
            var message = _fixture.Create<ParcelAddressWasAttachedV2>();

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var latestItemAddress = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.AddressPersistentLocalId);
                    latestItemAddress.Should().NotBeNull();
                    latestItemAddress!.CaPaKey.Should().Be(message.CaPaKey);
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedV2()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedV2(
                new ParcelId(attached.ParcelId),
                new VbrCaPaKey(attached.CaPaKey),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId));
            message.SetFixtureProvenance(_fixture);

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), attached, message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var latestItemAddress = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.AddressPersistentLocalId);
                    latestItemAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRejected()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRejected(
                new ParcelId(attached.ParcelId),
                new VbrCaPaKey(attached.CaPaKey),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId));
            message.SetFixtureProvenance(_fixture);

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), attached, message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var latestItemAddress = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.AddressPersistentLocalId);
                    latestItemAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRemoved()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRemoved(
                new ParcelId(attached.ParcelId),
                new VbrCaPaKey(attached.CaPaKey),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId));
            message.SetFixtureProvenance(_fixture);

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), attached, message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var latestItemAddress = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.AddressPersistentLocalId);
                    latestItemAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasDetachedBecauseAddressWasRetired()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRetired(
                new ParcelId(attached.ParcelId),
                new VbrCaPaKey(attached.CaPaKey),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId));
            message.SetFixtureProvenance(_fixture);

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), attached, message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var latestItemAddress = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.AddressPersistentLocalId);
                    latestItemAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenParcelAddressWasReplacedBecauseOfMunicipalityMerger()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();

            var message = new ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
                new ParcelId(attached.ParcelId),
                new VbrCaPaKey(attached.CaPaKey),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId + 1),
                new AddressPersistentLocalId(attached.AddressPersistentLocalId));
            message.SetFixtureProvenance(_fixture);

            await Sut
                .Given(_fixture.Create<ParcelWasImported>(), message)
                .Then(async context =>
                {
                    var latestItem = await context.ParcelLatestItemsV2.FindAsync(message.ParcelId);
                    latestItem.Should().NotBeNull();
                    latestItem!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);

                    var previousAddressLatestItem = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.PreviousAddressPersistentLocalId);
                    previousAddressLatestItem.Should().BeNull();

                    var newAddressLatestItem = await context.ParcelLatestItemV2Addresses.FindAsync(message.ParcelId, message.NewAddressPersistentLocalId);
                    newAddressLatestItem.Should().NotBeNull();
                    newAddressLatestItem!.CaPaKey.Should().Be(message.CaPaKey);
                });
        }

        protected override ParcelLatestItemV2Projections CreateProjection()
            => new(new OptionsWrapper<IntegrationOptions>(new IntegrationOptions{Namespace = Namespace }));
    }
}
