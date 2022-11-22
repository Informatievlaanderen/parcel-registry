namespace ParcelRegistry.Parcel.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, Tag.Migration)]
    [EventName(EventName)]
    [EventDescription("Het perceel werd gemigreerd.")]
    public sealed class ParcelWasMigrated : IParcelEvent
    {
        public const string EventName = "ParcelWasMigrated"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het perceel.")]
        public Guid ParcelId { get; }

        [EventPropertyDescription("De status van het perceel. Mogelijkheden: Realized en Retired.")]
        public string ParcelStatus { get; }

        [EventPropertyDescription("False wanneer het perceel niet werd verwijderd. True wanneer het perceel werd verwijderd.")]
        public bool IsRemoved { get; }

        [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de perceeleenheid.")]
        public List<int> AddressPersistentLocalIds { get; }

        [EventPropertyDescription("X-coördinaat van de centroïde van het terreinobject.")]
        public decimal? XCoordinate { get; }

        [EventPropertyDescription("Y-coördinaat van de centroïde van het terreinobject.")]
        public decimal? YCoordinate { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasMigrated(
            ParcelId parcelId,
            ParcelStatus parcelStatus,
            bool isRemoved,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds,
            Coordinate? xCoordinate,
            Coordinate? yCoordinate)
        {
            ParcelId = parcelId;
            ParcelStatus = parcelStatus;
            IsRemoved = isRemoved;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
            XCoordinate = xCoordinate ?? (decimal?) null;
            YCoordinate = yCoordinate ?? (decimal?) null;
        }

        [JsonConstructor]
        private ParcelWasMigrated(
            Guid parcelId,
            string parcelStatus,
            bool isRemoved,
            IEnumerable<int> addressPersistentLocalIds,
            decimal? xCoordinate,
            decimal? yCoordinate,
            ProvenanceData provenance)
            : this(
                new ParcelId(parcelId),
                ParcelRegistry.Parcel.ParcelStatus.Parse(parcelStatus),
                isRemoved,
                addressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)),
                xCoordinate.HasValue
                    ? new Coordinate(xCoordinate.Value)
                    : null,
                yCoordinate.HasValue
                    ? new Coordinate(yCoordinate.Value)
                    : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(ParcelId.ToString("D"));
            fields.Add(ParcelStatus);
            fields.Add(IsRemoved.ToString());
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            if (XCoordinate.HasValue)
            {
                fields.Add(XCoordinate.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (YCoordinate.HasValue)
            {
                fields.Add(YCoordinate.Value.ToString(CultureInfo.InvariantCulture));
            }
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}
