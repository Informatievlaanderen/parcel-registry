namespace ParcelRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "OntkoppelAdres", Namespace = "")]
    public class DetachAddressRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
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
