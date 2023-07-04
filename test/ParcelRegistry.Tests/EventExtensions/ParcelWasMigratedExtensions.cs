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
                GeometryHelpers.SecondGmlPointGeometry.GmlToExtendedWkbGeometry());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static ParcelWasMigrated WithRemoved(this ParcelWasMigrated @event, bool removed)
        {
            var newEvent = new ParcelWasMigrated(
                new ParcelId(@event.OldParcelId),
                new ParcelRegistry.Parcel.ParcelId(@event.ParcelId),
                new VbrCaPaKey(@event.CaPaKey),
                ParcelStatus.Parse(@event.ParcelStatus),
                removed,
                @event.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                GeometryHelpers.SecondGmlPointGeometry.GmlToExtendedWkbGeometry());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }

        public static ParcelWasMigrated WithParcelId(this ParcelWasMigrated @event,  ParcelRegistry.Legacy.ParcelId parcelId)
        {
            var newEvent = new ParcelWasMigrated(
                parcelId,
                new ParcelRegistry.Parcel.ParcelId(@event.ParcelId),
                new VbrCaPaKey(@event.CaPaKey),
                ParcelStatus.Parse(@event.ParcelStatus),
                @event.IsRemoved,
                @event.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                GeometryHelpers.SecondGmlPointGeometry.GmlToExtendedWkbGeometry());
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
                GeometryHelpers.SecondGmlPointGeometry.GmlToExtendedWkbGeometry());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());

            return newEvent;
        }
    }
}
