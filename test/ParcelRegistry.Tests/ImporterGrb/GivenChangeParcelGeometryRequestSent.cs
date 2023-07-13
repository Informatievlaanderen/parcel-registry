﻿namespace ParcelRegistry.Tests.ImporterGrb
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Extensions;
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
    using Parcel.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenChangeParcelGeometryRequestSent : ParcelRegistryTest
    {
        public GivenChangeParcelGeometryRequestSent(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithExtendedWkbGeometryPolygon());
        }

        [Fact]
        public async Task ThenCommandDispatched_StateCheck()
        {
            var caPaKey = CaPaKey.CreateFrom(Fixture.Create<string>());
            var parcelId = ParcelId.CreateFor(new VbrCaPaKey(caPaKey.VbrCaPaKey));

            DispatchArrangeCommand(
                new ImportParcel(
                    new VbrCaPaKey(caPaKey.VbrCaPaKey),
                    GeometryHelpers.ValidGmlPolygon.GmlToExtendedWkbGeometry(),
                    Fixture.Create<Provenance>()));

            var changeParcelGeometryRequest = new ChangeParcelGeometryRequest(
                new GrbParcel(caPaKey, GeometryHelpers.ValidPolygon2, 9));

            var sut = new ChangeParcelGeometryHandler(Container.Resolve<ICommandHandlerResolver>());

            await sut.Handle(changeParcelGeometryRequest, CancellationToken.None);

            var parcels = Container.Resolve<IParcels>();
            var parcel = await parcels.GetOptionalAsync(new ParcelStreamId(parcelId));

            parcel.Should().NotBeNull();
            parcel.Value.Geometry.Should().Be(ExtendedWkbGeometry.CreateEWkb(changeParcelGeometryRequest.GrbParcel.Geometry.ToBinary()));
            parcel.Value.LastProvenanceData.ToProvenance().Should().BeEquivalentTo(new Provenance(
                    parcel.Value.LastProvenanceData.Timestamp,
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelen Plan"),
                    new Operator("Parcel Registry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen));
        }
    }
}
