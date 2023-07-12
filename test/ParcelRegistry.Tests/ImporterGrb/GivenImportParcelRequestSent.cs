﻿namespace ParcelRegistry.Tests.ImporterGrb
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using Importer.Grb.Handlers;
    using Importer.Grb.Infrastructure;
    using NodaTime;
    using Parcel;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenImportParcelRequestSent : ParcelRegistryTest
    {
        public GivenImportParcelRequestSent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task ThenCommandDispatched()
        {
            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey.VbrCaPaKey));

            var importRequest = new ImportParcelRequest(new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon, 9));

            var sut = new ImportParcelHandler(Container.Resolve<ICommandHandlerResolver>());

            await sut.Handle(importRequest, CancellationToken.None);

            var parcels = Container.Resolve<IParcels>();
            var parcel = await parcels.GetOptionalAsync(new ParcelStreamId(parcelId));

            parcel.Should().NotBeNull();
            parcel.Value.Geometry.Should().Be(ExtendedWkbGeometry.CreateEWkb(importRequest.GrbParcel.Geometry.ToBinary()));
            parcel.Value.LastProvenanceData.ToProvenance().Should().BeEquivalentTo(new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelen Plan"),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen),
                options => options.Excluding(x => x.Timestamp));
        }


        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }
    }
}
