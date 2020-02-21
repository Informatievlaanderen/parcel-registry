namespace ParcelRegistry.Api.Legacy.Parcel.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PerceelCollectie", Namespace = "")]
    public class ParcelListResponse
    {
        /// <summary>
        /// De verzameling van percelen.
        /// </summary>
        [DataMember(Name = "Percelen", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<ParcelListItemResponse> Percelen { get; set; }

        /// <summary>
        /// Het totaal aantal percelen die overeenkomen met de vraag.
        /// </summary>
        //[DataMember(Name = "TotaalAantal", Order = 1)]
        //[JsonProperty(Required = Required.DisallowNull)]
        //public long TotaalAantal { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [DataMember(Name = "Volgende", Order = 2, EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Volgende { get; set; }
    }

    [DataContract(Name = "PerceelCollectieItem", Namespace = "")]
    public class ParcelListItemResponse
    {
        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelIdentificator Identificator { get; set; }

        /// <summary>
        /// De URL die naar de details van de meest recente versie van een enkel perceel leidt.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        public ParcelListItemResponse(string id, string naamruimte, string detail, DateTimeOffset version)
        {
            Identificator = new PerceelIdentificator(naamruimte, id, version);
            Detail = new Uri(string.Format(detail, id));
        }
    }

    public class ParcelListResponseExamples : IExamplesProvider<ParcelListResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelListResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
        {
            _responseOptions = responseOptionsProvider.Value;
        }

        public ParcelListResponse GetExamples()
        {
            var samples = new List<ParcelListItemResponse>
            {
                new ParcelListItemResponse("11001B0001-00S000", _responseOptions.Naamruimte, _responseOptions.DetailUrl, DateTimeOffset.Now),
                new ParcelListItemResponse("11001B0009-00G004", _responseOptions.Naamruimte, _responseOptions.DetailUrl, DateTimeOffset.Now.AddHours(-40))
            };

            return new ParcelListResponse
            {
                Percelen = samples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
