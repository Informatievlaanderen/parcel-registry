namespace ParcelRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Builders;
    using Fixtures;
    using FluentAssertions;
    using Parcel.Events;
    using Xunit;

    public partial class ParcelDetailItemV2Tests
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

            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasImported.GetHash() }
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelAddressWasAttachedV2.GetHash() }
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, parcelWasImportedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(new Envelope(parcelAddressWasAttachedV2, parcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>(new Envelope(@event, eventMetadata)))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetailWithCountV2.FindAsync(parcelWasImported.ParcelId);
                    parcelDetailV2.Should().NotBeNull();

                    var previousRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().BeNull();

                    var newRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.Count.Should().Be(1);

                    parcelDetailV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(@event.GetHash());
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

            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasImported.GetHash() }
            };
            var parcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelAddressWasAttachedV2.GetHash() }
            };
            var eventToAddPreviousRelationASecondTimeMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, eventToAddPreviousRelationASecondTime.GetHash() }
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
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
                    var parcelDetailV2 = await context.ParcelDetailWithCountV2.FindAsync(parcelWasImported.ParcelId);
                    parcelDetailV2.Should().NotBeNull();

                    var previousRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().NotBeNull();
                    previousRelation!.Count.Should().Be(1);

                    var newRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.Count.Should().Be(1);

                    parcelDetailV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(@event.GetHash());
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

            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasImported.GetHash() }
            };
            var previousParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, previousParcelAddressWasAttachedV2.GetHash() }
            };
            var newParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, newParcelAddressWasAttachedV2.GetHash() }
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
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
                    var parcelDetailV2 = await context.ParcelDetailWithCountV2.FindAsync(parcelWasImported.ParcelId);
                    parcelDetailV2.Should().NotBeNull();

                    var previousRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.PreviousAddressPersistentLocalId);
                    previousRelation.Should().BeNull();

                    var newRelation = parcelDetailV2!.Addresses
                        .SingleOrDefault(x => x.AddressPersistentLocalId == @event.NewAddressPersistentLocalId);
                    newRelation.Should().NotBeNull();
                    newRelation!.Count.Should().Be(2);

                    parcelDetailV2.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    parcelDetailV2.LastEventHash.Should().Be(@event.GetHash());
                });
        }

        [Fact]
        public async Task GivenParcelAddressesWereReaddressed_ThenAddressesAreAttachedAndDetached()
        {
            _fixture.Customizations.Add(new WithUniqueInteger());

            var parcelWasImported = _fixture.Create<ParcelWasImported>();
            var firstParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();
            var secondParcelAddressWasAttachedV2 = _fixture.Create<ParcelAddressWasAttachedV2>();

            var attachedAddressPersistentLocalIds = new[]
            {
                _fixture.Create<int>(),
                _fixture.Create<int>()
            };
            var detachedAddressPersistentLocalIds = new[]
            {
                firstParcelAddressWasAttachedV2.AddressPersistentLocalId,
                secondParcelAddressWasAttachedV2.AddressPersistentLocalId
            };

            var eventBuilder = new ParcelAddressesWereReaddressedBuilder(_fixture);

            foreach (var addressPersistentLocalId in attachedAddressPersistentLocalIds)
            {
                eventBuilder.WithAttachedAddress(addressPersistentLocalId);
            }

            foreach (var addressPersistentLocalId in detachedAddressPersistentLocalIds)
            {
                eventBuilder.WithDetachedAddress(addressPersistentLocalId);
            }

            var @event = eventBuilder.Build();

            var parcelWasImportedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, parcelWasImported.GetHash() }
            };
            var firstParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstParcelAddressWasAttachedV2.GetHash() }
            };
            var secondParcelAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, secondParcelAddressWasAttachedV2.GetHash() }
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            };

            await Sut
                .Given(
                    new Envelope<ParcelWasImported>(new Envelope(parcelWasImported, parcelWasImportedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(
                        new Envelope(firstParcelAddressWasAttachedV2, firstParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressWasAttachedV2>(
                        new Envelope(secondParcelAddressWasAttachedV2, secondParcelAddressWasAttachedMetadata)),
                    new Envelope<ParcelAddressesWereReaddressed>(new Envelope(@event, eventMetadata)))
                .Then(async context =>
                {
                    var parcelDetailV2 = await context.ParcelDetailWithCountV2.FindAsync(parcelWasImported.ParcelId);
                    parcelDetailV2.Should().NotBeNull();

                    foreach (var addressPersistentLocalId in attachedAddressPersistentLocalIds)
                    {
                        var parcelAddressRelation = parcelDetailV2!.Addresses
                            .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

                        parcelAddressRelation.Should().NotBeNull();
                        parcelAddressRelation!.Count.Should().Be(1);
                    }

                    foreach (var addressPersistentLocalId in detachedAddressPersistentLocalIds)
                    {
                        var parcelAddressRelation = parcelDetailV2!.Addresses
                            .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

                        parcelAddressRelation.Should().BeNull();
                    }
                });
        }
    }
}
