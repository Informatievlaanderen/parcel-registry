namespace ParcelRegistry.Tests.EventExtensions
{
    using System;
    using System.Linq;
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using Parcel.Events;
    using ParcelId = ParcelRegistry.Legacy.ParcelId;

    public static class ParcelWasMigratedExtensions
    {
        public static ParcelWasMigrated WithClearedAddresses(this ParcelWasMigrated @event)
        {
            var newEvent = new ParcelWasMigrated(
                new ParcelId(@event.OldParcelId),
                new ParcelRegistry.Parcel.ParcelId(@event.ParcelId),
                new VbrCaPaKey(@event.CaPaKey),
                ParcelStatus.Parse(@event.ParcelStatus),
                @event.IsRemoved,
                Array.Empty<AddressPersistentLocalId>(),
                @event.XCoordinate is not null ? new  Coordinate(@event.XCoordinate.Value) : null,
                @event.YCoordinate is not null ? new  Coordinate(@event.YCoordinate.Value) : null,
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static ParcelWasMigrated WithAddress(this ParcelWasMigrated @event, int addressPersistentLocalId)
        {
            var addressPersistentLocalIds = @event.AddressPersistentLocalIds
                .Select(x => new AddressPersistentLocalId(x))
                .ToList();
            addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));

            var newEvent = new ParcelWasMigrated(
                new ParcelId(@event.OldParcelId),
                new ParcelRegistry.Parcel.ParcelId(@event.ParcelId),
                new VbrCaPaKey(@event.CaPaKey),
                ParcelStatus.Parse(@event.ParcelStatus),
                @event.IsRemoved,
                addressPersistentLocalIds,
                @event.XCoordinate is not null ? new  Coordinate(@event.XCoordinate.Value) : null,
                @event.YCoordinate is not null ? new  Coordinate(@event.YCoordinate.Value) : null,
                GeometryHelpers.ValidGmlPolygon.ToExtendedWkbGeometry());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
