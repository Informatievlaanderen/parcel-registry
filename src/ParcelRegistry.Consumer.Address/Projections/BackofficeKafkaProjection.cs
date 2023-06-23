namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;

    public sealed class BackOfficeKafkaProjection : ConnectedProjection<ConsumerAddressContext>
    {
        private readonly WKBReader _wkbReader;

        public BackOfficeKafkaProjection()
        {
            _wkbReader = new WKBReader(
                new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    WkbGeometry.SridLambert72));

            When<AddressWasMigratedToStreetName>(async (context, message, ct) =>
            {
                await context
                        .AddressConsumerItems
                        .AddAsync(new AddressConsumerItem(
                                message.AddressPersistentLocalId,
                                Guid.Parse(message.AddressId),
                                AddressStatus.Parse(message.Status),
                                message.IsRemoved,
                                message.GeometryMethod,
                                message.GeometrySpecification,
                                ParsePosition(message.ExtendedWkbGeometry))
                            , ct);
            });

            When<AddressWasProposedV2>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed,
                            message.GeometryMethod,
                            message.GeometrySpecification,
                            ParsePosition(message.ExtendedWkbGeometry))
                        , ct);
            });

            When<AddressWasApproved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });

            When<AddressWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseStreetNameWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRetiredV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseStreetNameWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressWasRemovedBecauseStreetNameWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
            });

            When<AddressWasRemovedV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.IsRemoved = true;
            });

            When<AddressWasDeregulated>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });

            When<AddressWasCorrectedFromApprovedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRejectedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRetiredToCurrent>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Current;
            });

            When<AddressHouseNumberWasReaddressed>(async (context, message, ct) =>
            {
                var houseNumber =
                    await context.AddressConsumerItems.FindAsync(message.ReaddressedHouseNumber.DestinationAddressPersistentLocalId, cancellationToken: ct);
                houseNumber!.GeometryMethod = message.ReaddressedHouseNumber.SourceGeometryMethod;
                houseNumber.GeometrySpecification = message.ReaddressedHouseNumber.SourceGeometrySpecification;
                houseNumber.Position = ParsePosition(message.ReaddressedHouseNumber.SourceExtendedWkbGeometry);
                houseNumber.Status = AddressStatus.Parse(message.ReaddressedHouseNumber.SourceStatus);

                foreach (var readdressedBoxNumber in message.ReaddressedBoxNumbers)
                {
                    var boxNumber =
                        await context.AddressConsumerItems.FindAsync(readdressedBoxNumber.DestinationAddressPersistentLocalId, cancellationToken: ct);
                    boxNumber!.Status = AddressStatus.Parse(readdressedBoxNumber.SourceStatus);
                    boxNumber.GeometryMethod = message.ReaddressedHouseNumber.SourceGeometryMethod;
                    boxNumber.GeometrySpecification = message.ReaddressedHouseNumber.SourceGeometrySpecification;
                    boxNumber.Position = ParsePosition(message.ReaddressedHouseNumber.SourceExtendedWkbGeometry);
                }
            });

            When<AddressWasProposedBecauseOfReaddress>(async (context, message, ct) =>
            {
                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed,
                            message.GeometryMethod,
                            message.GeometrySpecification,
                            ParsePosition(message.ExtendedWkbGeometry))
                        , ct);
            });

            When<AddressWasRejectedBecauseOfReaddress>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Rejected;
            });

            When<AddressWasRetiredBecauseOfReaddress>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address.Status = AddressStatus.Retired;
            });

            When<AddressPositionWasChanged>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.Position = ParsePosition(message.ExtendedWkbGeometry);
            });

            When<AddressPositionWasCorrectedV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.Position = ParsePosition(message.ExtendedWkbGeometry);
            });
        }

        private Point ParsePosition(string extendedWkbGeometry)
            => (Point) _wkbReader.Read(extendedWkbGeometry.ToByteArray());
    }
}
