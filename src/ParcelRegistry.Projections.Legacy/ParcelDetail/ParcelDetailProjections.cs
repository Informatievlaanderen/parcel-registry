namespace ParcelRegistry.Projections.Legacy.ParcelDetail
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
    using NodaTime;
    using Parcel;
    using Parcel.Events;

    [ConnectedProjectionName("API endpoint detail/lijst percelen")]
    [ConnectedProjectionDescription("Projectie die de percelen data voor het percelen detail & lijst voorziet.")]
    public class ParcelDetailProjections : ConnectedProjection<LegacyContext>
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

        public ParcelDetailProjections()
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

                var item = new ParcelDetail(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Parse(message.Message.ParcelStatus),
                    message.Message.AddressPersistentLocalIds.Select(x => new ParcelDetailAddress(message.Message.ParcelId, x)),
                    gml,
                    geometryType,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(item, message);

                await context
                    .ParcelDetails
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
                            entity.Addresses.Add(new ParcelDetailAddress(message.Message.ParcelId, message.Message.AddressPersistentLocalId));
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

                        var previousAddress = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);

                        if (previousAddress is not null && previousAddress.Count == 1)
                        {
                            entity.Addresses.Remove(previousAddress);
                        }
                        else if (previousAddress is not null)
                        {
                            previousAddress.Count -= 1;
                        }

                        var newAddress = entity.Addresses.SingleOrDefault(parcelAddress =>
                            parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId
                            && parcelAddress.ParcelId == message.Message.ParcelId);

                        if (newAddress is null)
                        {
                            entity.Addresses.Add(new ParcelDetailAddress(message.Message.ParcelId, message.Message.NewAddressPersistentLocalId));
                        }
                        else
                        {
                            newAddress.Count += 1;
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateParcelDetail(
                    message.Message.ParcelId,
                    entity =>
                    {
                        context.Entry(entity).Collection(x => x.Addresses).Load();


                        foreach (var addressPersistentLocalId in message.Message.DetachedAddressPersistentLocalIds)
                        {
                            var relation = entity.Addresses.SingleOrDefault(parcelAddress =>
                                parcelAddress.AddressPersistentLocalId == addressPersistentLocalId
                                && parcelAddress.ParcelId == message.Message.ParcelId);

                            if (relation is not null)
                            {
                                entity.Addresses.Remove(relation);
                            }
                        }

                        foreach (var addressPersistentLocalId in message.Message.AttachedAddressPersistentLocalIds)
                        {
                            var relation = entity.Addresses.SingleOrDefault(parcelAddress =>
                                parcelAddress.AddressPersistentLocalId == addressPersistentLocalId
                                && parcelAddress.ParcelId == message.Message.ParcelId);

                            if (relation is null)
                            {
                                entity.Addresses.Add(new ParcelDetailAddress(message.Message.ParcelId, addressPersistentLocalId));
                            }
                        }

                        UpdateHash(entity, message);
                        UpdateVersionTimestamp(entity, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<ParcelWasImported>>(async (context, message, ct) =>
            {
                var (geometryType, gml) = ToGml(message.Message.ExtendedWkbGeometry);

                var item = new ParcelDetail(
                    message.Message.ParcelId,
                    message.Message.CaPaKey,
                    ParcelStatus.Realized,
                    new List<ParcelDetailAddress>(),
                    gml,
                    geometryType,
                    false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(item, message);

                await context
                    .ParcelDetails
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

        private static void UpdateHash<T>(ParcelDetail entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static void UpdateVersionTimestamp(ParcelDetail item, Instant versionTimestamp)
            => item.VersionTimestamp = versionTimestamp;
    }
}
