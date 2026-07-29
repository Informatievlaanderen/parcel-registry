namespace ParcelRegistry.Api.Oslo.Parcel.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Perceel;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    public class ParcelDetailOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het perceel.
        /// </summary>
        [JsonProperty("@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de perceel envelop.
        /// </summary>
        [JsonProperty("@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "PerceelEnvelop";

        /// <summary>
        /// De data van het perceel.
        /// </summary>
        [JsonProperty("data", Order = 2, Required = Required.DisallowNull)]
        public ParcelDetailOsloV3Data Data { get; }

        /// <summary>
        /// De hyperlinks die gerelateerd zijn aan het perceel.
        /// </summary>
        [JsonProperty("_links", Order = 99, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ParcelDetailOsloV3ResponseLinks? Links { get; set; }

        public ParcelDetailOsloV3Response(
            string contextUrlDetail,
            PerceelStatusValue status,
            string caPaKey,
            DateTimeOffset version,
            List<string> addressPersistentLocalIds,
            string adresDetailUrl,
            string selfDetailUrl,
            string buildingsLinkUrl)
        {
            Context = contextUrlDetail;
            Data = new ParcelDetailOsloV3Data(caPaKey, status, version, addressPersistentLocalIds, adresDetailUrl);

            Links = new ParcelDetailOsloV3ResponseLinks(
                self: new Link
                {
                    Href = new Uri(string.Format(selfDetailUrl, caPaKey))
                },
                gebouwen: new Link
                {
                    Href = new Uri(string.Format(buildingsLinkUrl, caPaKey))
                });
        }
    }

    /// <summary>
    /// De data van het perceel.
    /// </summary>
    public class ParcelDetailOsloV3Data
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
        /// De status van het perceel
        /// </summary>
        [JsonProperty("status", Order = 4, Required = Required.DisallowNull)]
        public PerceelStatus PerceelStatus { get; set; }

        /// <summary>
        /// De aan het perceel gekoppelde adressen.
        /// </summary>
        [DataMember(Name = "toegekendAdres", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PerceelToegekendAdres> Adressen { get; set; }

        public ParcelDetailOsloV3Data(
            string caPaKey,
            PerceelStatusValue status,
            DateTimeOffset version,
            List<string> addressPersistentLocalIds,
            string adresDetailUrl)
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
            PerceelStatus = new PerceelStatus(status);

            Adressen = addressPersistentLocalIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new PerceelToegekendAdres(OsloNamespaces.Adres.ToPuri(x), new Uri(string.Format(adresDetailUrl, x)).ToString()))
                .ToList();
        }
    }

    /// <summary>
    /// De hyperlinks die gerelateerd zijn aan het perceel.
    /// </summary>
    [DataContract(Name = "_links", Namespace = "")]
    public class ParcelDetailOsloV3ResponseLinks
    {
        [DataMember(Name = "self")]
        [JsonProperty(Required = Required.DisallowNull)]
        public Link Self { get; set; }

        [DataMember(Name = "gebouwen", EmitDefaultValue = false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Link? Gebouwen { get; set; }

        public ParcelDetailOsloV3ResponseLinks(
            Link self,
            Link? gebouwen = null)
        {
            Self = self;
            Gebouwen = gebouwen;
        }
    }

    public class ParcelOsloResponseExamples : IExamplesProvider<ParcelDetailOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public ParcelOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public ParcelDetailOsloV3Response GetExamples()
            => new ParcelDetailOsloV3Response(
                _responseOptions.ContextUrlDetail,
                PerceelStatusValue.Gerealiseerd,
                "11001B0001-00S000",
                DateTimeOffset.Now.ToExampleOffset(),
                new List<string> { "200001" },
                _responseOptions.AdresDetailUrl,
                _responseOptions.DetailUrl,
                _responseOptions.ParcelDetailBuildingsLink);
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
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
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
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
        };
    }
}
