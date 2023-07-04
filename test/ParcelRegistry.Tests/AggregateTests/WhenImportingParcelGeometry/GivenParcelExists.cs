﻿namespace ParcelRegistry.Tests.AggregateTests.WhenImportingParcelGeometry
{
    using Api.BackOffice.Abstractions.Extensions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using Fixtures;
    using FluentAssertions;
    using Parcel;
    using Parcel.Commands;
    using Parcel.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenParcelExists : ParcelRegistryTest
    {
        public GivenParcelExists (ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedParcelId());
            Fixture.Customize(new WithParcelStatus());
            Fixture.Customize(new Legacy.AutoFixture.WithFixedParcelId());
        }

        [Fact]
        public void ThenParcelGeometryImported()
        {
            var caPaKey = Fixture.Create<VbrCaPaKey>();

            var command = new ImportParcel(
                caPaKey,
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new ParcelStreamId(command.ParcelId))
                .When(command)
                .Then(new ParcelStreamId(command.ParcelId),
                    new ParcelWasImported(
                        command.ParcelId,
                        caPaKey,
                        command.Geometry)));
        }

        // [Fact]
        // public void WithAlreadyExistingParcel_ThenParcelGeometryImported()
        // {
        //     var caPaKey = Fixture.Create<VbrCaPaKey>();
        //
        //     var parcelWasMigrated = new Parcel
        //
        //     var command = new ImportParcel(
        //         caPaKey,
        //         GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry(),
        //         Fixture.Create<Provenance>());
        //
        //     Assert(new Scenario()
        //         .Given(new ParcelStreamId(command.ParcelId))
        //         .When(command)
        //         .Then(new ParcelStreamId(command.ParcelId),
        //             new ParcelWasImported(
        //                 command.ParcelId,
        //                 caPaKey,
        //                 command.Geometry)));
        // }
    }
}
