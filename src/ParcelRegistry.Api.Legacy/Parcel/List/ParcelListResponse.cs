namespace ParcelRegistry.Api.Legacy.Parcel.List
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using ParcelRegistry.Api.Legacy.Infrastructure.Options;
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

        [JsonIgnore]
        [IgnoreDataMember]
        public SortingHeader Sorting { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public PaginationInfo Pagination { get; set; }
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
        /// De URL die de details van de meest recente versie van het perceel weergeeft.
        /// </summary>
        [DataMember(Name = "Detail", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De status van het perceel
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelStatus PerceelStatus { get; set; }

        public ParcelListItemResponse(
            string id,
            string naamruimte,
            string detail,
            PerceelStatus status,
            DateTimeOffset version)
        {
            Identificator = new PerceelIdentificator(naamruimte, id, version);
            Detail = new Uri(string.Format(detail, id));
            PerceelStatus = status;
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
                new ParcelListItemResponse("11001B0001-00S000", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PerceelStatus.Gerealiseerd, DateTimeOffset.Now.ToExampleOffset()),
                new ParcelListItemResponse("11001B0009-00G004", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PerceelStatus.Gerealiseerd, DateTimeOffset.Now.AddHours(-40).ToExampleOffset())
            };

            return new ParcelListResponse
            {
                Percelen = samples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10))
            };
        }
    }
}
