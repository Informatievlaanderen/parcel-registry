namespace ParcelRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "OntkoppelAdres", Namespace = "")]
    public class DetachAddressRequest
    {
        /// <summary>
        /// Adres welke dient ontkoppeld te worden van het perceel.
        /// </summary>
        [DataMember(Name = "AdresId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string AdresId { get; set; }
    }

    public class DetachAddressRequestExamples : IExamplesProvider<DetachAddressRequest>
    {
        public DetachAddressRequest GetExamples()
        {
            return new DetachAddressRequest
            {
                AdresId = "https://data.vlaanderen.be/id/adres/6447380"
            };
        }
    }
}
