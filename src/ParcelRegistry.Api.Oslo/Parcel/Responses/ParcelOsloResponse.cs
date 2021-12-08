namespace ParcelRegistry.Api.Oslo.Parcel.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.JsonConverters;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PerceelDetail", Namespace = "")]
    public class ParcelOsloResponse
    {
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        [JsonConverter(typeof(PlainStringJsonConverter))]
        public object Context => @"{
        ""identificator"": ""@nest"",
        ""id"": ""@id"",
        ""versieId"": {
            ""@id"": ""https://data.vlaanderen.be/ns/generiek#versieIdentificator"",
            ""@type"": ""http://www.w3.org/2001/XMLSchema#string""
        },

    ""perceelStatus"": {
    ""@id"": ""https://data.vlaanderen.be/ns/perceel"",
    ""@type"": ""@id"",
    ""@context"": {
    ""@base"": ""https://data.vlaanderen.be/id/concept/perceelstatus/""
}
},
""adressen"":{
    ""@id"": ""https://data.vlaanderen.be/ns/adres"",
    ""@type"":""@id"",
    ""@context"":{
        ""objectId"":""@index"",
        ""detail"":""@value""
    }
}
}";

        /// <summary>
        /// Het linked-data type van het perceel.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "https://data.vlaanderen.be/ns/perceel";

        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelIdentificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelStatus PerceelStatus { get; set; }

        /// <summary>
        /// De aan het perceel gekoppelde adressen.
        /// </summary>
        [DataMember(Name = "Adressen", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PerceelDetailAdres> Adressen { get; set; }

        public ParcelOsloResponse(
            string naamruimte,
            PerceelStatus status,
            string caPaKey,
            DateTimeOffset version,
            List<string> addressPersistentLocalIds,
            string adresDetailUrl)
        {
            Identificator = new PerceelIdentificator(naamruimte, caPaKey, version);
            PerceelStatus = status;

            Adressen = addressPersistentLocalIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => PerceelDetailAdres.Create(x, new Uri(string.Format(adresDetailUrl, x))))
                .ToList();
        }
    }

    public class ParcelOsloResponseExamples : IExamplesProvider<ParcelOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public ParcelOsloResponse GetExamples()
            => new ParcelOsloResponse(
                _responseOptions.Naamruimte,
                PerceelStatus.Gerealiseerd,
                "11001B0001-00S000",
                DateTimeOffset.Now.ToExampleOffset(),
                new List<string> { "200001" },
                _responseOptions.AdresDetailUrl);
    }

    public class ParcelNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public ParcelNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand perceel.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
        };
    }

    public class ParcelGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public ParcelGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd perceel.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
        };
    }
}
