namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Builders;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Parcel.Events;
    using ParcelRegistry.Legacy;
    using ParcelRegistry.Legacy.Events;
    using Projections.Integration;
    using Projections.Integration.Infrastructure;
    using Projections.Integration.ParcelVersion;
    using Tests.Legacy;
    using Xunit;
    using ParcelId = Parcel.ParcelId;
    using ParcelStatus = Parcel.ParcelStatus;

    public partial class ParcelVersionProjectionTests : IntegrationProjectionTest<ParcelVersionProjections>
    {
        private readonly Fixture _fixture;
        private const string Namespace = "https://data.vlaanderen.be/id/perceel";
        private Mock<IAddressRepository> _addressRepository;

        public ParcelVersionProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedParcelId());
            _fixture.Customize(new Tests.Legacy.AutoFixture.WithFixedParcelId());
            _fixture.Customize(new WithParcelStatus());
            _fixture.Customize(new WithExtendedWkbGeometryPolygon());

            _addressRepository = new Mock<IAddressRepository>();
        }

        [Fact]
        public async Task WhenParcelWasMigrated()
        {
            var message = _fixture.Create<ParcelWasMigrated>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(new Envelope<ParcelWasMigrated>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelVersions = await context.ParcelVersions.FindAsync(position, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelVersions.Position.Should().Be(position);
                    parcelVersions.Status.Should().Be(ParcelStatus.Parse(message.ParcelStatus));
                    parcelVersions.OsloStatus.Should().BeOneOf("Gerealiseerd", "Gehistoreerd");
                    parcelVersions.Namespace.Should().Be(Namespace);
                    parcelVersions.Puri.Should().Be($"{Namespace}/{message.CaPaKey}");
                    parcelVersions.IsRemoved.Should().Be(message.IsRemoved);
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.CreatedOnTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.CreatedOnAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Geometry.Should().BeEquivalentTo(geometry);
                    parcelVersions.Type.Should().Be("EventName");

                    foreach (var addressPersistentLocalId in message.AddressPersistentLocalIds)
                    {
                        await context
                            .ParcelVersionAddresses
                            .FindAsync(position, message.ParcelId, addressPersistentLocalId);
                        parcelVersions.Should().NotBeNull();
                        parcelVersions.CaPaKey.Should().Be(message.CaPaKey);
                    }
                });
        }

        [Fact]
        public async Task WhenParcelIsImported()
        {
            var message = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(new Envelope<ParcelWasImported>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelVersions = await context.ParcelVersions.FindAsync(position, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.CaPaKey.Should().Be(message.CaPaKey);
                    parcelVersions.Position.Should().Be(position);
                    parcelVersions.Status.Should().Be(ParcelStatus.Realized);
                    parcelVersions.OsloStatus.Should().Be("Gerealiseerd");
                    parcelVersions.Namespace.Should().Be(Namespace);
                    parcelVersions.Puri.Should().Be($"{Namespace}/{message.CaPaKey}");
                    parcelVersions.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.CreatedOnTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.CreatedOnAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Geometry.Should().BeEquivalentTo(geometry);
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelGeometryWasChanged()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var message = new ParcelGeometryWasChanged(
                new ParcelId(parcelWasImported.ParcelId),
                new VbrCaPaKey(parcelWasImported.CaPaKey),
                GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry());
            ((ISetProvenance)message).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelGeometryWasChanged>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();

                    parcelVersions!.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Geometry.Should().BeEquivalentTo(geometry);
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedFromRetiredToRealized()
        {
            var message = _fixture.Create<ParcelWasCorrectedFromRetiredToRealized>();
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelWasCorrectedFromRetiredToRealized>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var geometry = WKBReaderFactory.Create().Read(message.ExtendedWkbGeometry.ToByteArray());

                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Realized);
                    parcelVersions.OsloStatus.Should().Be("Gerealiseerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Geometry.Should().BeEquivalentTo(geometry);
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasRetiredV2()
        {
            var message = _fixture.Create<ParcelWasRetiredV2>();
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelWasRetiredV2>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Retired);
                    parcelVersions.OsloStatus.Should().Be("Gehistoreerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttachedV2()
        {
            var message = new ParcelAddressWasAttachedV2Builder(_fixture)
                .WithAddress(123)
                .Build();
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, message.AddressPersistentLocalId);
                    parcelVersionAddress.Should().NotBeNull();
                    parcelVersionAddress!.CaPaKey.Should().Be(message.CaPaKey);

                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedV2()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedV2Builder(_fixture)
                .WithParcelId(new ParcelId(attached.ParcelId))
                .WithVbrCaPaKey(new VbrCaPaKey(attached.CaPaKey))
                .WithAddress(attached.AddressPersistentLocalId)
                .Build();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var attachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(attached, attachedMetadata)),
                    new Envelope<ParcelAddressWasDetachedV2>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, message.AddressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.AddressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();

                    var parcelVersion = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRejected()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRejectedBuilder(_fixture)
                .WithParcelId(new ParcelId(attached.ParcelId))
                .WithAddress(attached.AddressPersistentLocalId)
                .Build();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var attachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(attached, attachedMetadata)),
                    new Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, message.AddressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.AddressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();

                    var parcelVersion = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRemoved()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRemovedBuilder(_fixture)
                .WithParcelId(new ParcelId(attached.ParcelId))
                .WithAddress(attached.AddressPersistentLocalId)
                .Build();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var attachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(attached, attachedMetadata)),
                    new Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, message.AddressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.AddressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();

                    var parcelVersion = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasDetachedBecauseAddressWasRetired()
        {
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasDetachedBecauseAddressWasRetiredBuilder(_fixture)
                .WithParcelId(new ParcelId(attached.ParcelId))
                .WithAddress(attached.AddressPersistentLocalId)
                .Build();

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var attachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, metadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(attached, attachedMetadata)),
                    new Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, message.AddressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.AddressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();

                    var parcelVersion = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasReplacedBecauseOfMunicipalityMerger()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var attached = _fixture.Create<ParcelAddressWasAttachedV2>();
            var message = new ParcelAddressWasReplacedBecauseOfMunicipalityMergerBuilder(_fixture)
                .WithPreviousAddress(attached.AddressPersistentLocalId)
                .WithNewAddress(attached.AddressPersistentLocalId + 1)
                .Build();

            var position = _fixture.Create<long>();
            var importedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var attachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, importedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(attached, attachedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Type.Should().Be("EventName");

                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.PreviousAddressPersistentLocalId);
                    previousParcelVersionAddress.Should().BeNull();

                    var newParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, message.NewAddressPersistentLocalId);
                    newParcelVersionAddress.Should().NotBeNull();
                    newParcelVersionAddress!.CaPaKey.Should().Be(message.CaPaKey);
                });
        }

        #region Legacy

        [Fact]
        public async Task WhenParcelWasRegistered()
        {
            var message = _fixture.Create<ParcelWasRegistered>();
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(new Envelope<ParcelWasRegistered>(new Envelope(message, metadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.CaPaKey.Should().Be(CaPaKey.CreateFrom(message.VbrCaPaKey));
                    parcelVersions.Position.Should().Be(position);
                    parcelVersions.Namespace.Should().Be(Namespace);
                    parcelVersions.Puri.Should().Be($"{Namespace}/{CaPaKey.CreateFrom(message.VbrCaPaKey)}");
                    parcelVersions.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.CreatedOnTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.CreatedOnAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasRecovered()
        {
            var message = _fixture.Create<ParcelWasRecovered>();

            var addressId = Guid.NewGuid();
            var addressPersistentLocalId = 123;
            var parcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            _addressRepository.Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(parcelAddressWasAttached, parcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelWasRecovered>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().BeNull();
                    parcelVersions.OsloStatus.Should().BeNull();
                    parcelVersions.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());

                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, addressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, addressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();

                    var parcelVersion = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasRemoved()
        {
            var message = _fixture.Create<ParcelWasRemoved>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelWasRemoved>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions.IsRemoved.Should().BeTrue();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasRetired()
        {
            var message = _fixture.Create<ParcelWasRetired>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelWasRetired>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Retired);
                    parcelVersions.OsloStatus.Should().Be("Gehistoreerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedToRetired()
        {
            var message = _fixture.Create<ParcelWasCorrectedToRetired>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelWasCorrectedToRetired>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Retired);
                    parcelVersions.OsloStatus.Should().Be("Gehistoreerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasRealized()
        {
            var message = _fixture.Create<ParcelWasRealized>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelWasRealized>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Realized);
                    parcelVersions.OsloStatus.Should().Be("Gerealiseerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelWasCorrectedToRealized()
        {
            var message = _fixture.Create<ParcelWasCorrectedToRealized>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelWasCorrectedToRealized>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.Status.Should().Be(ParcelStatus.Realized);
                    parcelVersions.OsloStatus.Should().Be("Gerealiseerd");
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttached()
        {

            var addressId = Guid.NewGuid();
            var addressPersistentLocalId = 123;

            var message = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            _addressRepository.Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 1, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, addressPersistentLocalId);
                    currentParcelVersionAddress.Should().NotBeNull();
                    currentParcelVersionAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenParcelAddressWasAttached_WithAlreadyAttached()
        {
            var addressId = Guid.NewGuid();
            var addressPersistentLocalId = 123;

            var firstMessage = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));
            var secondMessage = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var firstMessageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            var secondMessageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            _addressRepository.Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(firstMessage, firstMessageMetadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(secondMessage, secondMessageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position, secondMessage.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(secondMessage.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(secondMessage.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position, secondMessage.ParcelId, addressPersistentLocalId);
                    currentParcelVersionAddress.Should().NotBeNull();
                    currentParcelVersionAddress!.Count.Should().Be(2);
                });
        }

         [Fact]
        public async Task WhenParcelAddressWasDetached()
        {
            var addressId = Guid.NewGuid();
            var addressPersistentLocalId = 123;

            var message = _fixture.Create<ParcelAddressWasDetached>()
                .WithAddressId(new AddressId(addressId));
            var parcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));
            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 1 },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position + 2 },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            _addressRepository.Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(parcelAddressWasAttached, parcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasDetached>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position + 2, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");

                    var previousParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 1, message.ParcelId, addressPersistentLocalId);
                    previousParcelVersionAddress.Should().NotBeNull();

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position + 2, message.ParcelId, addressPersistentLocalId);
                    currentParcelVersionAddress.Should().BeNull();
                });
        }

         [Fact]
        public async Task WhenParcelAddressWasDetached_WithAttachedTwice()
        {
            var addressId = Guid.NewGuid();
            var addressPersistentLocalId = 123;

            var message = _fixture.Create<ParcelAddressWasDetached>()
                .WithAddressId(new AddressId(addressId));
            var firstParcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));
            var secondParcelAddressWasAttached = _fixture.Create<ParcelAddressWasAttached>()
                .WithAddressId(new AddressId(addressId));

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var firstParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var secondParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var messageMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            _addressRepository.Setup(x => x.GetAddressPersistentLocalId(addressId))
                .ReturnsAsync(addressPersistentLocalId);

            await Sut.Given(
                    new Envelope<ParcelWasRegistered>(new Envelope(_fixture.Create<ParcelWasRegistered>(), metadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(firstParcelAddressWasAttached, firstParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasAttached>(new Envelope(secondParcelAddressWasAttached, secondParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasDetached>(new Envelope(message, messageMetadata)))
                .Then(async context =>
                {
                    var parcelVersions = await context.ParcelVersions.FindAsync(position, message.ParcelId);
                    parcelVersions.Should().NotBeNull();
                    parcelVersions!.IsRemoved.Should().BeFalse();
                    parcelVersions.VersionTimestamp.Should().Be(message.Provenance.Timestamp);
                    parcelVersions.VersionAsString.Should()
                        .Be(new Rfc3339SerializableDateTimeOffset(message.Provenance.Timestamp.ToBelgianDateTimeOffset()).ToString());
                    parcelVersions.Type.Should().Be("EventName");

                    var currentParcelVersionAddress =
                        await context.ParcelVersionAddresses.FindAsync(position, message.ParcelId, addressPersistentLocalId);
                    currentParcelVersionAddress.Should().NotBeNull();
                    currentParcelVersionAddress!.Count.Should().Be(1);
                });
        }

        #endregion

        protected override ParcelVersionProjections CreateProjection()
            => new(_addressRepository.Object ,new OptionsWrapper<IntegrationOptions>(new IntegrationOptions { Namespace = Namespace }));
    }
}
