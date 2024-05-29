namespace ParcelRegistry.Tests.AggregateTests.WhenRetiringParcel
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Builders;
    using Parcel;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExistsWithAttachedAddresses : ParcelRegistryTest
    {
        public GivenParcelExistsWithAttachedAddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public void ThenParcelRetiredAndAddressesDetached()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();
            var parcelId = ParcelId.CreateFor(caPaKey);

            var parcelWasImported = new ParcelWasImportedBuilder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .Build();

            var attachedAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var addressWasAttached = new ParcelAddressWasAttachedV2Builder(Fixture)
                .WithParcelId(parcelId)
                .WithCaPaKey(caPaKey)
                .WithAddress(attachedAddressPersistentLocalId)
                .Build();

            var command = new RetireParcelV2Builder(Fixture)
                .WithVbrCaPaKey(caPaKey)
                .Build();

            Assert(new Scenario()
                .Given(new ParcelStreamId(parcelId), parcelWasImported, addressWasAttached)
                .When(command)
                .Then(new Fact(new ParcelStreamId(command.ParcelId),
                        new ParcelAddressWasDetachedV2(command.ParcelId, command.CaPaKey, attachedAddressPersistentLocalId)),
                    new Fact(new ParcelStreamId(command.ParcelId),
                        new ParcelWasRetiredV2(parcelId, caPaKey))
                ));
        }
    }
}
