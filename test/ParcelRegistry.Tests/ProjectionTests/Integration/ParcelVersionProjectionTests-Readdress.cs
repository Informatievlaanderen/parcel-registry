namespace ParcelRegistry.Tests.ProjectionTests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using FluentAssertions;
    using Parcel.Events;
    using Xunit;

    public partial class ParcelVersionProjectionTests
    {
        [Fact]
        public async Task GivenOnlyPreviousParcelAddressRelationExistsWithCountOne_ThenRelationIsReplaced()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(_fixture)
                .WithPreviousAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId)
                .WithNewAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId + 1)
                .Build();

            var position = _fixture.Create<long>();
            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, parcelWasImportedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(parcelAddressWasAttachedV2, parcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, eventMetadata)))
                .Then(async context =>
                {
                    var previousParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().BeNull();

                    var newParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newParcelAddressRelation.Should().NotBeNull();
                    newParcelAddressRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newParcelAddressRelation.Count.Should().Be(1);

                    var parcelVersion = await context.ParcelVersions.FindAsync(position, @event.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task GivenPreviousParcelAddressRelationExistsWithCountTwo_ThenCountIsDecrementedByOne()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var parcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var eventToAddPreviousRelationASecondTime = new ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(_fixture)
                .WithPreviousAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId + 100)
                .WithNewAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId)
                .Build();
            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(_fixture)
                .WithPreviousAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId)
                .WithNewAddress(parcelAddressWasAttachedV2.AddressPersistentLocalId + 1)
                .Build();

            var position = _fixture.Create<long>();
            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventToAddPreviousRelationASecondTimeMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, parcelWasImportedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(parcelAddressWasAttachedV2, parcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(eventToAddPreviousRelationASecondTime, eventToAddPreviousRelationASecondTimeMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, eventMetadata)))
                .Then(async context =>
                {
                    var previousParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().NotBeNull();
                    previousParcelAddressRelation!.Count.Should().Be(1);

                    var newParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newParcelAddressRelation.Should().NotBeNull();
                    newParcelAddressRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newParcelAddressRelation.Count.Should().Be(1);

                    var parcelVersion = await context.ParcelVersions.FindAsync(position, @event.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task GivenNewParcelAddressRelationsExists_ThenCountIsIncrementedByOne()
        {
            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var previousParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var newParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var @event = new ParcelAddressWasReplacedBecauseAddressWasReaddressedBuilder(_fixture)
                .WithPreviousAddress(previousParcelAddressWasAttachedV2.AddressPersistentLocalId)
                .WithNewAddress(newParcelAddressWasAttachedV2.AddressPersistentLocalId)
                .Build();

            var position = _fixture.Create<long>();
            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var previousParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var newParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, _fixture.Create<string>() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, parcelWasImportedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(
                        new Envelope(previousParcelAddressWasAttachedV2, previousParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(
                        new Envelope(newParcelAddressWasAttachedV2, newParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, eventMetadata)))
                .Then(async context =>
                {
                    var previousParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.PreviousAddressPersistentLocalId);
                    previousParcelAddressRelation.Should().BeNull();

                    var newParcelAddressRelation = await context.ParcelVersionAddresses.FindAsync(
                        position,
                        @event.ParcelId,
                        @event.NewAddressPersistentLocalId);
                    newParcelAddressRelation.Should().NotBeNull();
                    newParcelAddressRelation!.CaPaKey.Should().Be(@event.CaPaKey);
                    newParcelAddressRelation.Count.Should().Be(2);

                    var parcelVersion = await context.ParcelVersions.FindAsync(position, @event.ParcelId);
                    parcelVersion.Should().NotBeNull();
                    parcelVersion!.Type.Should().Be("EventName");
                });
        }
    }
}
