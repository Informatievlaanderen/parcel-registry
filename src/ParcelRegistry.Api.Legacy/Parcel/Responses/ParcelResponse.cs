namespace ParcelRegistry.Api.Legacy.Parcel.Responses
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
    using Newtonsoft.Json;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PerceelDetail", Namespace = "")]
    public class ParcelResponse
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
        /// De aan het perceel gelinkte adressen
        /// </summary>
        [DataMember(Name = "Adressen", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<PerceelDetailAdres> Adressen { get; set; }

        public ParcelResponse(
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

    public class ParcelResponseExamples : IExamplesProvider<ParcelResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public ParcelResponse GetExamples()
            => new ParcelResponse(
                _responseOptions.Naamruimte,
                PerceelStatus.Gerealiseerd,
                "11001B0001-00S000",
                DateTimeOffset.Now,
                new List<string> { "200001" },
                _responseOptions.AdresDetailUrl);
    }

    public class ParcelNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand perceel.",
            ProblemInstanceUri = new DefaultHttpContext().GetProblemInstanceUri()
        };
    }

    public class ParcelGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples() => new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:parcel:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Perceel werd verwijderd.",
            ProblemInstanceUri = new DefaultHttpContext().GetProblemInstanceUri()
        };
    }
}
