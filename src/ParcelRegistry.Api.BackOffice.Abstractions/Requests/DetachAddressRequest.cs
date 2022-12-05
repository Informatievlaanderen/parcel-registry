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
        [DataMember(Name = "AddressPersistentLocalId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public int AddressPersistentLocalId { get; set; }
    }

    public class DetachAddressRequestExamples : IExamplesProvider<DetachAddressRequest>
    {
        public DetachAddressRequest GetExamples()
        {
            return new DetachAddressRequest
            {
                AddressPersistentLocalId = 1011
            };
        }
    }
}
