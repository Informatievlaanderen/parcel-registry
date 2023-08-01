namespace ParcelRegistry.Projections.Legacy.ParcelDetailV2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("API endpoint detail/lijst percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen detail & lijst voorziet.")]
    public class ParcelDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        private string MapGeometryType(OgcGeometryType ogcGeometryType)
        {
            return ogcGeometryType switch
            {
                OgcGeometryType.Polygon => OgcGeometryType.Polygon.ToString(),
                OgcGeometryType.MultiSurface => OgcGeometryType.MultiSurface.ToString(),
                OgcGeometryType.MultiPolygon => OgcGeometryType.MultiSurface.ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(ogcGeometryType), ogcGeometryType, null)
            };
        }

        public ParcelDetailV2Projections()
        {
            var wkbReader = WKBReaderFactory.Create();

            (string gmlType, string gml) ToGml(string extendedWkbGeometry)
            {
                var geometry = wkbReader.Read(extendedWkbGeometry.ToByteArray());
                var gml = geometry.ConvertToGml();
                return (MapGeometryType(geometry.OgcGeometryType), gml);
            }

            When<Envelope<ParcelWasMigrated>>(async (context, message, ct) =>
            {
                var (geometryType, gml) = ToGml(message.Message.ExtendedWkbGeometry);

                var item = new ParcelDetailV2(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Parse(message.Message.ParcelStatus),
                    message.Message.AddressPersistentLocalIds.Select(x => new ParcelDetailAddressV2(message.Message.ParcelId, x)),
                    gml,
                    geometryType,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(item, message);

                await context
                    .ParcelDetailV2
                    .AddAsync(item, ct);
            });

            When<Envelope<ParcelAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        if (!entity.Addresses.Any(parcelAddress =>
                                parcelAddress.AddressPersistentLocalId == message.Message.AddressPersistentLocalId
                                && parcelAddress.ParcelId == message.Message.ParcelId))
                        {
                            entity.Addresses.Add(new ParcelDetailAddressV2(message.Message.ParcelId, message.Message.AddressPersistentLocalId));
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.AddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);
                        if (addressToRemove is not null)
                        {
                            entity.Addresses.Remove(addressToRemove);
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.AddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);
                        if (addressToRemove is not null)
                        {
                            entity.Addresses.Remove(addressToRemove);
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.AddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);
                        if (addressToRemove is not null)
                        {
                            entity.Addresses.Remove(addressToRemove);
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.AddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);
                        if (addressToRemove is not null)
                        {
                            entity.Addresses.Remove(addressToRemove);
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();

                        var addressToRemove = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);
                        if (addressToRemove is not null)
                        {
                            entity.Addresses.Remove(addressToRemove);
                        }

                        if (!entity.Addresses.Any(parcelAddress =>
                                parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId
                                && parcelAddress.ParcelId == message.Message.ParcelId))
                        {
                            entity.Addresses.Add(new ParcelDetailAddressV2(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId));
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var (geometryType, gml) = ToGml(message.Message.ExtendedWkbGeometry);

                var item = new ParcelDetailV2(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Realized,
                    new List<ParcelDetailAddressV2>(),
                    gml,
                    geometryType,
                    false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(item, message);

                await context
                    .ParcelDetailV2
                    .AddAsync(item, ct);
            });

            When<Envelope<ParcelWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        entity.Status = ParcelStatus.Retired;

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelGeometryWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                        entity.Gml = geometry.ConvertToGml();
                        entity.GmlType = geometry.OgcGeometryType.ToString();

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        var geometry = wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                        entity.Gml = geometry.ConvertToGml();
                        entity.GmlType = geometry.OgcGeometryType.ToString();

                        entity.Status = ParcelStatus.Realized;

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
        }

        private static void UpdateHash<T>(ParcelDetailV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateVersionTimestamp(ParcelDetailV2 item, Instant versionTimestamp)
            => item.VersionTimestamp = versionTimestamp;
    }
}
