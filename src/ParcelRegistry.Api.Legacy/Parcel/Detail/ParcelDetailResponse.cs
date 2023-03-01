namespace ParcelRegistry.Api.Legacy.Parcel.Detail
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
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PerceelDetail", Namespace = "")]
    public class ParcelDetailResponse
    {
        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelIdentificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PerceelStatus PerceelStatus { get; set; }

        /// <summary>
        /// De aan het perceel gekoppelde adressen.
        /// </summary>
        [DataMember(Name = "Adressen", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PerceelDetailAdres> Adressen { get; set; }

        public ParcelDetailResponse(
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

    public class ParcelResponseExamples : IExamplesProvider<ParcelDetailResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public ParcelDetailResponse GetExamples()
            => new ParcelDetailResponse(
                _responseOptions.Naamruimte,
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
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
            ApiVersion = apiVersion;
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
