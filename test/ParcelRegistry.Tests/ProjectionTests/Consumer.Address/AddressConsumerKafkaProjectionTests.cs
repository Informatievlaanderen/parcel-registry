namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using ParcelRegistry.Consumer.Address;
    using ParcelRegistry.Consumer.Address.Projections;
    using Xunit;
    using Xunit.Abstractions;

    public class ConsumerAddressKafkaProjectionTests : KafkaProjectionTest<ConsumerAddressContext, BackOfficeKafkaProjection>
    {
        public ConsumerAddressKafkaProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task AddressMigratedToStreetName_AddsAddress()
        {
            var addressStatus = Fixture
                .Build<AddressStatus>()
                .FromFactory(() =>
                {
                    var statuses = new List<AddressStatus>
                    {
                        AddressStatus.Current, AddressStatus.Proposed, AddressStatus.Rejected, AddressStatus.Retired
                    };

                    return statuses[new Random(Fixture.Create<int>()).Next(0, statuses.Count - 1)];
                })
                .Create();

            var addressWasMigratedToStreetName = Fixture
                .Build<AddressWasMigratedToStreetName>()
                .FromFactory(() => new AddressWasMigratedToStreetName(
                    Fixture.Create<int>(),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<int>(),
                    addressStatus.Status,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<bool>(),
                    Fixture.Create<string>(),
                    Fixture.Create<bool>(),
                    Fixture.Create<bool>(),
                    Fixture.Create<int?>(),
                    Fixture.Create<Provenance>()
                    ))
                .Create();

            Given(addressWasMigratedToStreetName);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasMigratedToStreetName.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.AddressId.Should().Be(Guid.Parse(addressWasMigratedToStreetName.AddressId));
                address.IsRemoved.Should().Be(address.IsRemoved);
                address.Status.Should().Be(addressStatus);
            });
        }

        [Fact]
        public async Task AddressWasProposedV2_AddsAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            Given(addressWasProposedV2);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.AddressId.Should().BeNull();
                address.IsRemoved.Should().Be(false);
                address.Status.Should().Be(AddressStatus.Proposed);
            });
        }

        [Fact]
        public async Task AddressWasApproved_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Current);
            });
        }

        [Fact]
        public async Task AddressWasCorrectedFromApprovedToProposed_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromApprovedToProposed = Fixture.Build<AddressWasCorrectedFromApprovedToProposed>()
                .FromFactory(() => new AddressWasCorrectedFromApprovedToProposed(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasCorrectedFromApprovedToProposed);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Proposed);
            });
        }

        [Fact]
        public async Task AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromApprovedToProposed = Fixture.Build<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>()
                .FromFactory(() => new AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasCorrectedFromApprovedToProposed);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Proposed);
            });
        }

        [Fact]
        public async Task AddressWasDeregulated_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasDeregulated = Fixture.Build<AddressWasDeregulated>()
                .FromFactory(() => new AddressWasDeregulated(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasDeregulated);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Current);
            });
        }

        [Fact]
        public async Task AddressWasRejected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = Fixture.Build<AddressWasRejected>()
                .FromFactory(() => new AddressWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseHouseNumberWasRejected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseHouseNumberWasRejected>()
                .FromFactory(() => new AddressWasRejectedBecauseHouseNumberWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseHouseNumberWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseHouseNumberWasRetired>()
                .FromFactory(() => new AddressWasRejectedBecauseHouseNumberWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseStreetNameWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseStreetNameWasRetired>()
                .FromFactory(() => new AddressWasRejectedBecauseStreetNameWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasCorrectedFromRejectedToProposed_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRejected = Fixture.Build<AddressWasRejected>()
                .FromFactory(() => new AddressWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromRejectedToProposed = Fixture.Build<AddressWasCorrectedFromRejectedToProposed>()
                .FromFactory(() => new AddressWasCorrectedFromRejectedToProposed(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected, addressWasCorrectedFromRejectedToProposed);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Proposed);
            });
        }

        [Fact]
        public async Task AddressWasRetiredV2_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetiredV2 = Fixture.Build<AddressWasRetiredV2>()
                .FromFactory(() => new AddressWasRetiredV2(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasRetiredV2);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Retired);
            });
        }

        [Fact]
        public async Task AddressWasRetiredBecauseHouseNumberWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseHouseNumberWasRetired>()
                .FromFactory(() => new AddressWasRetiredBecauseHouseNumberWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasRetired);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Retired);
            });
        }

        [Fact]
        public async Task AddressWasRetiredBecauseStreetNameWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseStreetNameWasRetired>()
                .FromFactory(() => new AddressWasRetiredBecauseStreetNameWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasRetired);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Retired);
            });
        }

        [Fact]
        public async Task AddressWasCorrectedFromRetiredToCurrent_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetiredV2 = Fixture.Build<AddressWasRetiredV2>()
                .FromFactory(() => new AddressWasRetiredV2(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromRetiredToCurrent = Fixture.Build<AddressWasCorrectedFromRetiredToCurrent>()
                .FromFactory(() => new AddressWasCorrectedFromRetiredToCurrent(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasRetiredV2, addressWasCorrectedFromRetiredToCurrent);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Current);
            });
        }

        [Fact]
        public async Task AddressWasRemovedV2_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRemovedV2 = Fixture.Build<AddressWasRemovedV2>()
                .FromFactory(() => new AddressWasRemovedV2(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRemovedV2);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Proposed);
                address.IsRemoved.Should().BeTrue();
            });
        }

        [Fact]
        public async Task AddressWasRemovedBecauseHouseNumberWasRemoved_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>();
            var addressWasRemovedV2 = Fixture.Build<AddressWasRemovedBecauseHouseNumberWasRemoved>()
                .FromFactory(() => new AddressWasRemovedBecauseHouseNumberWasRemoved(
                    addressWasProposedV2.StreetNamePersistentLocalId, addressWasProposedV2.AddressPersistentLocalId, Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRemovedV2);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Proposed);
                address.IsRemoved.Should().BeTrue();
            });
        }

        protected override ConsumerAddressContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerAddressContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerAddressContext(options);
        }

        protected override BackOfficeKafkaProjection CreateProjection() => new BackOfficeKafkaProjection();
    }
}
