namespace ParcelRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "KoppelAdres", Namespace = "")]
    public class AttachAddressRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [DataMember(Name = "AddressPersistentLocalId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public int AddressPersistentLocalId { get; set; }
    }

    public class AttachAddressRequestExamples : IExamplesProvider<AttachAddressRequest>
    {
        public AttachAddressRequest GetExamples()
        {
            return new AttachAddressRequest
            {
                AddressPersistentLocalId = 1011
            };
        }
    }
}
