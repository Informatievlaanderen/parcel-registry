namespace ParcelRegistry.Api.Oslo.Parcel.List
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PerceelCollectie", Namespace = "")]
    public class ParcelListOsloResponse
    {
        /// <summary>
        /// De linked-data context van het perceel.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// De verzameling van percelen.
        /// </summary>
        [DataMember(Name = "Percelen", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<ParcelListItemOsloResponse> Percelen { get; set; }

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
    public class ParcelListItemOsloResponse
    {
        /// <summary>
        /// Het linked-data type van het perceel.
        /// </summary>
        [DataMember(Name = "@type", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Perceel";

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

        public ParcelListItemOsloResponse(
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

    public class ParcelListOsloResponseExamples : IExamplesProvider<ParcelListOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelListOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
        {
            _responseOptions = responseOptionsProvider.Value;
        }

        public ParcelListOsloResponse GetExamples()
        {
            var samples = new List<ParcelListItemOsloResponse>
            {
                new ParcelListItemOsloResponse("11001B0001-00S000", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PerceelStatus.Gerealiseerd, DateTimeOffset.Now.ToExampleOffset()),
                new ParcelListItemOsloResponse("11001B0009-00G004", _responseOptions.Naamruimte, _responseOptions.DetailUrl, PerceelStatus.Gerealiseerd, DateTimeOffset.Now.AddHours(-40).ToExampleOffset())
            };

            return new ParcelListOsloResponse
            {
                Percelen = samples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
