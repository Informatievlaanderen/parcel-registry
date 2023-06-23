namespace ParcelRegistry.Tests.ProjectionTests.Consumer.Address
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using ParcelRegistry.Consumer.Address;
    using ParcelRegistry.Consumer.Address.Projections;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class
        ConsumerAddressKafkaProjectionTests : KafkaProjectionTest<ConsumerAddressContext, BackOfficeKafkaProjection>
    {
        public ConsumerAddressKafkaProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithExtendedWkbGeometry());
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
                    Fixture.Create<ExtendedWkbGeometry>().ToString(),
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
                address.GeometryMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod);
                address.GeometrySpecification.Should().Be(addressWasMigratedToStreetName.GeometrySpecification);
                address.Position.Should().Be(
                    (Point)_wkbReader.Read(addressWasMigratedToStreetName.ExtendedWkbGeometry.ToByteArray()));
            });
        }

        [Fact]
        public async Task AddressWasProposedV2_AddsAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();

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
                address.GeometryMethod.Should().Be(addressWasProposedV2.GeometryMethod);
                address.GeometrySpecification.Should().Be(addressWasProposedV2.GeometrySpecification);
                address.Position.Should()
                    .Be((Point)_wkbReader.Read(addressWasProposedV2.ExtendedWkbGeometry.ToByteArray()));
            });
        }

        [Fact]
        public async Task AddressWasApproved_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();

            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromApprovedToProposed = Fixture.Build<AddressWasCorrectedFromApprovedToProposed>()
                .FromFactory(() => new AddressWasCorrectedFromApprovedToProposed(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromApprovedToProposed = Fixture
                .Build<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>()
                .FromFactory(() => new AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasDeregulated = Fixture.Build<AddressWasDeregulated>()
                .FromFactory(() => new AddressWasDeregulated(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasDeregulated);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Current);
            });
        }

        [Fact]
        public async Task AddressWasRejected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejected>()
                .FromFactory(() => new AddressWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseHouseNumberWasRejected>()
                .FromFactory(() => new AddressWasRejectedBecauseHouseNumberWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseHouseNumberWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseHouseNumberWasRetired>()
                .FromFactory(() => new AddressWasRejectedBecauseHouseNumberWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressWasRejectedBecauseStreetNameWasRejected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseStreetNameWasRejected>()
                .FromFactory(() => new AddressWasRejectedBecauseStreetNameWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRejected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.Status.Should().Be(AddressStatus.Rejected);
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseStreetNameWasRetired_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseStreetNameWasRetired>()
                .FromFactory(() => new AddressWasRejectedBecauseStreetNameWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejected>()
                .FromFactory(() => new AddressWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromRejectedToProposed = Fixture.Build<AddressWasCorrectedFromRejectedToProposed>()
                .FromFactory(() => new AddressWasCorrectedFromRejectedToProposed(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetiredV2 = Fixture.Build<AddressWasRetiredV2>()
                .FromFactory(() => new AddressWasRetiredV2(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseHouseNumberWasRetired>()
                .FromFactory(() => new AddressWasRetiredBecauseHouseNumberWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressWasRetiredBecauseStreetNameWasRejected_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseStreetNameWasRejected>()
                .FromFactory(() => new AddressWasRetiredBecauseStreetNameWasRejected(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseStreetNameWasRetired>()
                .FromFactory(() => new AddressWasRetiredBecauseStreetNameWasRetired(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRemovedV2 = Fixture.Build<AddressWasRemovedBecauseStreetNameWasRemoved>()
                .FromFactory(() => new AddressWasRemovedBecauseStreetNameWasRemoved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasRemovedV2);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.IsRemoved.Should().BeTrue();
            });
        }

        [Fact]
        public async Task AddressWasCorrectedFromRetiredToCurrent_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetiredV2 = Fixture.Build<AddressWasRetiredV2>()
                .FromFactory(() => new AddressWasRetiredV2(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasCorrectedFromRetiredToCurrent = Fixture.Build<AddressWasCorrectedFromRetiredToCurrent>()
                .FromFactory(() => new AddressWasCorrectedFromRetiredToCurrent(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposedV2, addressWasApproved, addressWasRetiredV2,
                addressWasCorrectedFromRetiredToCurrent);

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
        public async Task AddressHouseNumberWasReaddressed_UpdatesStatusAddresses()
        {
            var houseNumberAddressWasProposedV2 = CreateAddressWasProposedV2()
                .WithAddressPersistentLocalId(1);
            var boxNumberAddressWasProposedV2 = CreateAddressWasProposedV2()
                .WithAddressPersistentLocalId(2);

            var readdressedHouseNumber = new ReaddressedAddressData(
                sourceAddressPersistentLocalId: 101,
                destinationAddressPersistentLocalId: houseNumberAddressWasProposedV2.AddressPersistentLocalId,
                true,
                AddressStatus.Current,
                destinationHouseNumber: "3",
                sourceBoxNumber: null,
                sourcePostalCode: "9000",
                "AppointedByAdministrator",
                "Entry",
                Fixture.Create<ExtendedWkbGeometry>().ToString(),
                sourceIsOfficiallyAssigned: false);

            var readdressedBoxNumber = new ReaddressedAddressData(
                sourceAddressPersistentLocalId: 101,
                destinationAddressPersistentLocalId: boxNumberAddressWasProposedV2.AddressPersistentLocalId,
                true,
                AddressStatus.Current,
                destinationHouseNumber: "3",
                sourceBoxNumber: "A",
                sourcePostalCode: "9000",
                "AppointedByAdministrator",
                "Entry",
                Fixture.Create<ExtendedWkbGeometry>().ToString(),
                sourceIsOfficiallyAssigned: false);

            var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
                1000000,
                readdressedHouseNumber.DestinationAddressPersistentLocalId,
                readdressedHouseNumber,
                new[] { readdressedBoxNumber },
                Fixture.Create<Provenance>());

            Given(houseNumberAddressWasProposedV2, boxNumberAddressWasProposedV2, addressHouseNumberWasReaddressed);

            await Then(async context =>
            {
                var houseNumberAddress =
                    await context.AddressConsumerItems.FindAsync(houseNumberAddressWasProposedV2
                        .AddressPersistentLocalId);

                houseNumberAddress.Should().NotBeNull();
                houseNumberAddress!.Status.Should().Be(AddressStatus.Current);

                var boxNumberAddress =
                    await context.AddressConsumerItems.FindAsync(boxNumberAddressWasProposedV2
                        .AddressPersistentLocalId);

                boxNumberAddress.Should().NotBeNull();
                boxNumberAddress!.Status.Should().Be(AddressStatus.Current);
                boxNumberAddress.GeometryMethod.Should().Be(readdressedBoxNumber.SourceGeometryMethod);
                boxNumberAddress.GeometrySpecification.Should().Be(readdressedBoxNumber.SourceGeometrySpecification);
                boxNumberAddress.Position.Should().Be(
                    (Point)_wkbReader.Read(readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray()));
            });
        }

        [Fact]
        public async Task AddressWasProposedBecauseOfReaddress_AddsAddress()
        {
            var addressWasProposed = Fixture
                .Build<AddressWasProposedBecauseOfReaddress>()
                .FromFactory(() => new AddressWasProposedBecauseOfReaddress(
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<ExtendedWkbGeometry>().ToString(),
                    Fixture.Create<Provenance>()))
                .Create();

            Given(addressWasProposed);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposed.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address!.AddressId.Should().BeNull();
                address.IsRemoved.Should().Be(false);
                address.Status.Should().Be(AddressStatus.Proposed);
                address.GeometryMethod.Should().Be(addressWasProposed.GeometryMethod);
                address.GeometrySpecification.Should().Be(addressWasProposed.GeometrySpecification);
                address.Position.Should().Be(
                    (Point)_wkbReader.Read(addressWasProposed.ExtendedWkbGeometry.ToByteArray()));
            });
        }

        [Fact]
        public async Task AddressWasRejectedBecauseOfReaddress_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRejected = Fixture.Build<AddressWasRejectedBecauseOfReaddress>()
                .FromFactory(() => new AddressWasRejectedBecauseOfReaddress(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressWasRetiredBecauseOfReaddress_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasApproved = Fixture.Build<AddressWasApproved>()
                .FromFactory(() => new AddressWasApproved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();
            var addressWasRetired = Fixture.Build<AddressWasRetiredBecauseOfReaddress>()
                .FromFactory(() => new AddressWasRetiredBecauseOfReaddress(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressWasRemovedV2_UpdatesStatusAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRemovedV2 = Fixture.Build<AddressWasRemovedV2>()
                .FromFactory(() => new AddressWasRemovedV2(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressWasRemovedV2 = Fixture.Build<AddressWasRemovedBecauseHouseNumberWasRemoved>()
                .FromFactory(() => new AddressWasRemovedBecauseHouseNumberWasRemoved(
                    addressWasProposedV2.StreetNamePersistentLocalId,
                    addressWasProposedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
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
        public async Task AddressPositionWasChanged_UpdatesPositionAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressPositionWasChanged =
                Fixture
                    .Build<AddressPositionWasChanged>()
                    .FromFactory(() => new AddressPositionWasChanged(
                        addressWasProposedV2.StreetNamePersistentLocalId,
                        addressWasProposedV2.AddressPersistentLocalId,
                        Fixture.Create<string>(),
                        Fixture.Create<string>(),
                        Fixture.Create<ExtendedWkbGeometry>().ToString(),
                        Fixture.Create<Provenance>()))
                    .Create();

            Given(addressWasProposedV2, addressPositionWasChanged);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.GeometryMethod.Should().Be(addressPositionWasChanged.GeometryMethod);
                address.GeometrySpecification.Should().Be(addressPositionWasChanged.GeometrySpecification);
                address.Position.Should().Be((Point)_wkbReader.Read(addressPositionWasChanged.ExtendedWkbGeometry.ToByteArray()));
            });
        }

        [Fact]
        public async Task AddressPositionWasCorrectedV2_UpdatesPositionAddress()
        {
            var addressWasProposedV2 = CreateAddressWasProposedV2();
            var addressPositionWasCorrected =
                Fixture
                    .Build<AddressPositionWasCorrectedV2>()
                    .FromFactory(() => new AddressPositionWasCorrectedV2(
                        addressWasProposedV2.StreetNamePersistentLocalId,
                        addressWasProposedV2.AddressPersistentLocalId,
                        Fixture.Create<string>(),
                        Fixture.Create<string>(),
                        Fixture.Create<ExtendedWkbGeometry>().ToString(),
                        Fixture.Create<Provenance>()))
                    .Create();

            Given(addressWasProposedV2, addressPositionWasCorrected);

            await Then(async context =>
            {
                var address =
                    await context.AddressConsumerItems.FindAsync(
                        addressWasProposedV2.AddressPersistentLocalId);

                address.Should().NotBeNull();
                address.GeometryMethod.Should().Be(addressPositionWasCorrected.GeometryMethod);
                address.GeometrySpecification.Should().Be(addressPositionWasCorrected.GeometrySpecification);
                address.Position.Should().Be((Point)_wkbReader.Read(addressPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));
            });
        }

        private AddressWasProposedV2 CreateAddressWasProposedV2()
        {
            return Fixture
                .Build<AddressWasProposedV2>()
                .FromFactory(() => new AddressWasProposedV2(
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<int>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    Fixture.Create<ExtendedWkbGeometry>().ToString(),
                    Fixture.Create<Provenance>()))
                .Create();
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
