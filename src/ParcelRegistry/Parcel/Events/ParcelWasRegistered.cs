namespace ParcelRegistry.Parcel.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("ParcelWasRegistered")]
    [EventDescription("Het perceel werd geregistreerd.")]
    public class ParcelWasRegistered : IHasProvenance, ISetProvenance
    {
        public Guid ParcelId { get; }
        public string VbrCaPaKey { get; }
        public ProvenanceData Provenance { get; private set; }

        public ParcelWasRegistered(
            ParcelId parcelId,
            VbrCaPaKey vbrCaPaKey)
        {
            ParcelId = parcelId;
            VbrCaPaKey = vbrCaPaKey;
        }

        [JsonConstructor]
        private ParcelWasRegistered(
            Guid parcelId,
            string vbrCaPaKey,
            ProvenanceData provenance)
            : this(new ParcelId(parcelId),
                new VbrCaPaKey(vbrCaPaKey)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}
