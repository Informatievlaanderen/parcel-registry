namespace ParcelRegistry.Api.Oslo.Parcel.V3.List
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Perceel;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PerceelCollectie", Namespace = "")]
    public class ParcelListOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het perceel.
        /// </summary>
        [JsonProperty("@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; set; }

        /// <summary>
        /// Het linked-data type van de percelen envelop.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order= 1, Required = Required.DisallowNull)]
        public string Type => "PercelenEnvelop";

        /// <summary>
        /// De verzameling van percelen.
        /// </summary>
        [JsonProperty("data", Order = 2, Required = Required.DisallowNull)]
        public List<ParcelListItemOsloV3Response> Percelen { get; set; }

        /// <summary>
        /// De URL voor het ophalen van de volgende verzameling.
        /// </summary>
        [JsonProperty("volgende", Order = 10, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Uri Volgende { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public SortingHeader Sorting { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public PaginationInfo Pagination { get; set; }
    }

    public class ParcelListItemOsloV3Response
    {
        /// <summary>
        /// Het linked-data type van het perceel.
        /// </summary>
        [JsonProperty("@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "KadastraalPlanperceel";

        /// <summary>
        /// De unieke en persistente identificator van het perceel (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty("@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificatoren van het perceel.
        /// </summary>
        [JsonProperty("identificator", Order = 2, Required = Required.DisallowNull)]
        public List<PerceelIdentificator> Identificator { get; set; }

        /// <summary>
        /// De URL die de details van de meest recente versie van het perceel weergeeft.
        /// </summary>
        [JsonProperty("detail", Order = 3, Required = Required.DisallowNull)]
        public Uri Detail { get; set; }

        /// <summary>
        /// De status van het perceel
        /// </summary>
        [JsonProperty("status", Order = 5, Required = Required.DisallowNull)]
        public PerceelStatus PerceelStatus { get; set; }

        public ParcelListItemOsloV3Response(
            string caPaKey,
            string detail,
            PerceelStatusValue status,
            DateTimeOffset version)
        {
            Id = OsloNamespaces.Perceel.ToPuri(caPaKey);
            Identificator = [
                new PerceelIdentificator(
                    caPaKey,
                    version,
                    PerceelIdentificatorToegekendDoor.Basisregisters),
                new PerceelIdentificator(
                    Be.Vlaanderen.Basisregisters.GrAr.Common.CaPaKey.CreateFrom(caPaKey).CaPaKeyCrabNotation2!,
                    PerceelIdentificatorToegekendDoor.Aapd)
            ];
            Detail = new Uri(string.Format(detail, caPaKey));
            PerceelStatus = new PerceelStatus(status);
        }
    }

    public class ParcelListOsloV3ResponseExamples : IExamplesProvider<ParcelListOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public ParcelListOsloV3ResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
        {
            _responseOptions = responseOptionsProvider.Value;
        }

        public ParcelListOsloV3Response GetExamples()
        {
            var samples = new List<ParcelListItemOsloV3Response>
            {
                new ParcelListItemOsloV3Response("11001B0001-00S000", _responseOptions.DetailUrl, PerceelStatusValue.Gerealiseerd, DateTimeOffset.Now.ToExampleOffset()),
                new ParcelListItemOsloV3Response("11001B0009-00G004", _responseOptions.DetailUrl, PerceelStatusValue.Gerealiseerd, DateTimeOffset.Now.AddHours(-40).ToExampleOffset())
            };

            return new ParcelListOsloV3Response
            {
                Percelen = samples,
                Volgende = new Uri(string.Format(_responseOptions.VolgendeUrl, 2, 10)),
                Context = _responseOptions.ContextUrlList
            };
        }
    }
}
