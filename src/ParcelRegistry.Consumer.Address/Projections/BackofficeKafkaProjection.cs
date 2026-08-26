namespace ParcelRegistry.Consumer.Address.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;

    // ADR 0003 records why this alias is here rather than a plain using: in a file that imports the GrAr
    // namespace, the simple name would bind to GrAr's WKBReaderFactory, which throws on SRID-less EWKB
    // where ours falls back to Lambert 72 — silently, with no ambiguity error and no warning.
    using WKBReaderFactory = ParcelRegistry.WKBReaderFactory;

    public sealed class BackOfficeKafkaProjection : ConnectedProjection<ConsumerAddressContext>
    {
        /// <summary>
        /// The decimals a transformed position is rounded to. Positions are persisted at centimetre
        /// precision and the transform is accurate to it, so this drops floating point noise rather than
        /// information. Only a transformed position is rounded; one that needs no transform is stored
        /// exactly as the event store holds it. See ADR 0004.
        /// </summary>
        private const int TransformedCoordinateDecimals = 2;

        public BackOfficeKafkaProjection()
        {
            When<AddressWasMigratedToStreetName>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                await context
                        .AddressConsumerItems
                        .AddAsync(new AddressConsumerItem(
                                message.AddressPersistentLocalId,
                                Guid.Parse(message.AddressId),
                                AddressStatus.Parse(message.Status),
                                message.IsRemoved,
                                message.GeometryMethod,
                                message.GeometrySpecification,
                                position.InLambert72,
                                position.InLambert2008)
                            , ct);
            });

            When<AddressWasProposedV2>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed,
                            message.GeometryMethod,
                            message.GeometrySpecification,
                            position.InLambert72,
                            position.InLambert2008)
                        , ct);
            });

            When<AddressWasProposedForMunicipalityMerger>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed,
                            message.GeometryMethod,
                            message.GeometrySpecification,
                            position.InLambert72,
                            position.InLambert2008)
                        , ct);
            });

            When<AddressWasApproved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Current;
            });

            When<AddressWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseStreetNameWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRejectedBecauseOfMunicipalityMerger>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRetiredV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseStreetNameWasRejected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressWasRetiredBecauseOfMunicipalityMerger>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressWasRemovedBecauseStreetNameWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.IsRemoved = true;
            });

            When<AddressWasRemovedV2>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.IsRemoved = true;
            });

            When<AddressRemovalWasCorrected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);

                var position = ParsePosition(message.ExtendedWkbGeometry);

                address!.Status = AddressStatus.Parse(message.Status);
                address.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.Position = position.InLambert72;
                address.PositionLambert2008 = position.InLambert2008;
                address.IsRemoved = false;
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.IsRemoved = true;
            });

            When<AddressWasDeregulated>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Current;
            });

            When<AddressWasCorrectedFromApprovedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRejectedToProposed>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Proposed;
            });

            When<AddressWasCorrectedFromRetiredToCurrent>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Current;
            });

            When<AddressHouseNumberWasReaddressed>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ReaddressedHouseNumber.SourceExtendedWkbGeometry);

                var houseNumber =
                    await context.AddressConsumerItems.FindAsync(message.ReaddressedHouseNumber.DestinationAddressPersistentLocalId, cancellationToken: ct);
                houseNumber!.GeometryMethod = message.ReaddressedHouseNumber.SourceGeometryMethod;
                houseNumber.GeometrySpecification = message.ReaddressedHouseNumber.SourceGeometrySpecification;
                houseNumber.Position = position.InLambert72;
                houseNumber.PositionLambert2008 = position.InLambert2008;
                houseNumber.Status = AddressStatus.Parse(message.ReaddressedHouseNumber.SourceStatus);

                foreach (var readdressedBoxNumber in message.ReaddressedBoxNumbers)
                {
                    var boxNumber =
                        await context.AddressConsumerItems.FindAsync(readdressedBoxNumber.DestinationAddressPersistentLocalId, cancellationToken: ct);
                    boxNumber!.Status = AddressStatus.Parse(readdressedBoxNumber.SourceStatus);
                    boxNumber.GeometryMethod = message.ReaddressedHouseNumber.SourceGeometryMethod;
                    boxNumber.GeometrySpecification = message.ReaddressedHouseNumber.SourceGeometrySpecification;
                    boxNumber.Position = position.InLambert72;
                    boxNumber.PositionLambert2008 = position.InLambert2008;
                }
            });

            When<AddressWasProposedBecauseOfReaddress>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                await context
                    .AddressConsumerItems
                    .AddAsync(new AddressConsumerItem(
                            message.AddressPersistentLocalId,
                            AddressStatus.Proposed,
                            message.GeometryMethod,
                            message.GeometrySpecification,
                            position.InLambert72,
                            position.InLambert2008)
                        , ct);
            });

            When<AddressWasRejectedBecauseOfReaddress>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Rejected;
            });

            When<AddressWasRetiredBecauseOfReaddress>(async (context, message, ct) =>
            {
                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.Status = AddressStatus.Retired;
            });

            When<AddressPositionWasChanged>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.Position = position.InLambert72;
                address.PositionLambert2008 = position.InLambert2008;
            });

            When<AddressPositionWasCorrectedV2>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.Position = position.InLambert72;
                address.PositionLambert2008 = position.InLambert2008;
            });

            // The conversion of the address event store to Lambert 2008. This is the event that fills
            // PositionLambert2008 for the entire table — every address is converted, and each conversion is
            // an event — so it is what makes a backfill unnecessary.
            //
            // It deliberately does not write Position. The address does not move here, it is re-expressed,
            // so the stored Lambert 72 value is already exact; transforming this payload back would replace
            // it with a centimetre-rounded round trip of itself. Position is queried right up until parcels
            // are converted, and FindAddressesWithinGeometry matches on Touches as well as Contains, so
            // moving every address in the register by up to a centimetre is not free. See ADR 0004.
            When<AddressPositionCrsWasChanged>(async (context, message, ct) =>
            {
                var position = ParsePosition(message.ExtendedWkbGeometry);

                var address = await context.AddressConsumerItems.FindAsync(message.AddressPersistentLocalId, cancellationToken: ct);
                address!.GeometryMethod = message.GeometryMethod;
                address.GeometrySpecification = message.GeometrySpecification;
                address.PositionLambert2008 = position.InLambert2008;
            });
        }

        /// <summary>
        /// A position in both reference systems. Whichever one the event carries is passed through
        /// untransformed and unrounded; the other is derived from it.
        /// </summary>
        private readonly record struct AddressPosition(Point InLambert72, Point InLambert2008);

        /// <summary>
        /// Reads a position in whatever reference system the EWKB carries, rather than assuming one, and
        /// derives the other. Read through <see cref="ParcelRegistry.WKBReaderFactory"/> rather than GrAr's:
        /// addresses migrated before the event store wrote EWKB carry no SRID, and GrAr's factory throws on
        /// those where ours falls back to Lambert 72.
        /// </summary>
        private static AddressPosition ParsePosition(string extendedWkbGeometry)
        {
            var bytes = extendedWkbGeometry.ToByteArray()!;
            var point = (Point)WKBReaderFactory.CreateForEwkb(bytes).Read(bytes);

            // Both Ensure* methods decide by envelope, not by SRID, so a position outside both would not be
            // transformed at all — it would just have an SRID stamped on unmoved coordinates, putting it
            // ~500 km from where it belongs, where it falls inside no parcel and silently belongs to none.
            // The consumer is replayable; stopping is cheaper than storing that.
            if (!point.IsInsideFlandersUsingLambert72() && !point.IsInsideFlandersUsingLambert08())
            {
                throw new InvalidOperationException(
                    $"Address position {point.AsText()} (SRID {point.SRID}) lies outside Flanders in both "
                    + "Lambert 72 and Lambert 2008, so it cannot be transformed into either.");
            }

            return new AddressPosition(
                point.IsLambert72() ? point : point.EnsureLambert72().RoundCoordinates(TransformedCoordinateDecimals),
                point.IsLambert08() ? point : point.EnsureLambert08(TransformedCoordinateDecimals));
        }
    }
}
