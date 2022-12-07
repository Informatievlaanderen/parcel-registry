namespace ParcelRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "KoppelAdres", Namespace = "")]
    public class AttachAddressRequest
    {
        /// <summary>
        /// Adres welke dient gekoppeld te worden aan het perceel.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }
    }

    public class AttachAddressRequestExamples : IExamplesProvider<AttachAddressRequest>
    {
        public AttachAddressRequest GetExamples()
        {
            return new AttachAddressRequest
            {
                AdresId = "https://data.vlaanderen.be/id/adressen/6447380"
            };
        }
    }
}
