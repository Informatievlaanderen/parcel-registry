namespace ParcelRegistry.Api.Oslo.Parcel.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PerceelDetail", Namespace = "")]
    public class ParcelDetailOsloResponse
    {
        /// <summary>
        /// De linked-data context van het perceel.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van het perceel.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Perceel";

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

        public ParcelDetailOsloResponse(
            string naamruimte,
            string contextUrlDetail,
            PerceelStatus status,
            string caPaKey,
            DateTimeOffset version,
            List<string> addressPersistentLocalIds,
            string adresDetailUrl)
        {
            Context = contextUrlDetail;
            Identificator = new PerceelIdentificator(naamruimte, caPaKey, version);
            PerceelStatus = status;

            Adressen = addressPersistentLocalIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => PerceelDetailAdres.Create(x, new Uri(string.Format(adresDetailUrl, x))))
                .ToList();
        }
    }

    public class ParcelOsloResponseExamples : IExamplesProvider<ParcelDetailOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public ParcelDetailOsloResponse GetExamples()
            => new ParcelDetailOsloResponse(
                _responseOptions.Naamruimte,
                _responseOptions.ContextUrlDetail,
                PerceelStatus.Gerealiseerd,
                "11001B0001-00S000",
                DateTimeOffset.Now.ToExampleOffset(),
                new List<string> { "200001" },
                _responseOptions.AdresDetailUrl);
    }

    public class ParcelNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        protected string ApiVersion { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public ParcelNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper,
            string apiVersion = "v1")
        {
            ApiVersion = apiVersion;
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand perceel.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, ApiVersion)
        };
    }

    public class ParcelNotFoundResponseExamplesV2 : ParcelNotFoundResponseExamples
    {
        public ParcelNotFoundResponseExamplesV2(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper) : base(httpContextAccessor, problemDetailsHelper, "v2")
        { }
    }

    public class ParcelGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        protected string ApiVersion { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public ParcelGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper,
            string apiVersion = "v1")
        {
            ApiVersion = apiVersion;
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd perceel.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, ApiVersion)
        };
    }

    public class ParcelGoneResponseExamplesV2 : ParcelGoneResponseExamples
    {
        public ParcelGoneResponseExamplesV2(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper) : base(httpContextAccessor, problemDetailsHelper, "v2")
        { }
    }
}
