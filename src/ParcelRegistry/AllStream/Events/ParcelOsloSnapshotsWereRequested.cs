namespace ParcelRegistry.AllStream.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using Parcel;

    [HideEvent]
    [EventName(EventName)]
    [EventDescription("Nieuwe OSLO snapshots werd aangevraagd voor de percelen.")]
    public sealed class ParcelOsloSnapshotsWereRequested : IHasProvenance, ISetProvenance, IMessage
    {
        public const string EventName = "ParcelOsloSnapshotsWereRequested"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID's van de percelen met de bijhorende CaPaKey.")]
        public IDictionary<Guid, string> ParcelIdsWithCapaKey { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public ParcelOsloSnapshotsWereRequested(
            IDictionary<ParcelId, VbrCaPaKey> parcelIdsWithCapaKey)
        {
            ParcelIdsWithCapaKey = parcelIdsWithCapaKey.ToDictionary(
                x => (Guid)x.Key,
                x => (string)x.Value);
        }

        [JsonConstructor]
        private ParcelOsloSnapshotsWereRequested(
            IDictionary<Guid, string> parcelIdsWithCapaKey,
            ProvenanceData provenance)
            : this(
                parcelIdsWithCapaKey.ToDictionary(
                    x => new ParcelId(x.Key),
                    x => new VbrCaPaKey(x.Value)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.AddRange(ParcelIdsWithCapaKey.Keys.Select(x => x.ToString("D")));
            fields.AddRange(ParcelIdsWithCapaKey.Values.Select(x => x));

            return fields;
        }
    }
}
