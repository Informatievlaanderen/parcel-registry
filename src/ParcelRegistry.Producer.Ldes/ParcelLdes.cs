namespace ParcelRegistry.Producer.Ldes
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public sealed class ParcelLdes
    {
        private static readonly JObject Context = JObject.Parse(@"{
    ""@base"": ""https://basisregisters.vlaanderen.be/implementatiemodel/gebouwenregister"",
    ""@vocab"": ""#"",
    ""identificator"": ""@nest"",
    ""id"": ""@id"",
    ""versieId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""naamruimte"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#naamruimte"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""objectId"": {
      ""@id"": ""https://data.vlaanderen.be/ns/generiek#lokaleIdentificator"",
      ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
    },
    ""perceelStatus"": {
      ""@id"": ""https://basisregisters.vlaanderen.be/implementatiemodel/gebouwenregister#Perceel%3Astatus"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/doc/concept/perceelstatus/""
      }
    },
    ""adressen"": {
      ""@id"": ""https://data.vlaanderen.be/ns/gebouw#Gebouweenheid.adres"",
      ""@container"": ""@set"",
      ""@type"": ""@id"",
      ""@context"": {
        ""@base"": ""https://data.vlaanderen.be/id/adres/""
      }
    }
}");

        [JsonProperty("@context", Order = 0)]
        public JObject LdContext => Context;

        [JsonProperty("@type", Order = 1)]
        public string Type => "Perceel";

        [JsonProperty("Identificator", Order = 2)]
        public PerceelIdentificator Identificator { get; }

        [JsonProperty("CaPaKey", Order = 3)]
        public string CaPaKey { get; }

        [JsonProperty("PerceelStatus", Order = 4)]
        public PerceelStatus Status { get; }

        [JsonProperty("Adressen", Order = 5)]
        public ICollection<string> Addresses { get; } = new List<string>();

        [JsonProperty("IsVerwijderd", Order = 6)]
        public bool IsRemoved { get; }

        public ParcelLdes(ParcelDetail parcel, string osloNamespace)
        {
            Identificator = new PerceelIdentificator(
                osloNamespace,
                parcel.CaPaKey,
                parcel.VersionTimestamp.ToBelgianDateTimeOffset());
            CaPaKey = new VbrCaPaKey(parcel.CaPaKey).ToCaPaKey().CaPaKeyCrabNotation2;
            Status = parcel.Status.MapToPerceelStatus();

            foreach (var address in parcel.Addresses.Where(a => a.Count > 0))
                Addresses.Add(address.AddressPersistentLocalId.ToString());

            IsRemoved = parcel.IsRemoved;
        }
    }
}
