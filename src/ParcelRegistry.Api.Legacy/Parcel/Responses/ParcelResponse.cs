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
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [DataContract(Name = "PerceelDetail", Namespace = "")]
    public class ParcelResponse
    {
        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 2)]
        public PerceelStatus PerceelStatus { get; set; }

        /// <summary>
        /// De aan het perceel gelinkte adressen
        /// </summary>
        [DataMember(Name = "Adressen", Order = 3)]
        public List<PerceelDetailAdres> Adressen { get; set; }

        public ParcelResponse(
            string naamruimte,
            PerceelStatus status,
            string caPaKey,
            DateTimeOffset version,
            List<string> osloIds,
            string adresDetailUrl)
        {
            Identificator = new Identificator(naamruimte, caPaKey, version);
            PerceelStatus = status;

            Adressen = osloIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => PerceelDetailAdres.Create(x, new Uri(string.Format(adresDetailUrl, x))))
                .ToList();
        }
    }

    public class ParcelResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
            => new ParcelResponse(
                _responseOptions.Naamruimte,
                PerceelStatus.Gerealiseerd,
                "11001B0001-00S000",
                DateTimeOffset.Now,
                new List<string> { "200001" },
                _responseOptions.AdresDetailUrl);
    }

    public class ParcelNotFoundResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new ProblemDetails
        {
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand perceel.",
            ProblemInstanceUri = ProblemDetails.GetProblemNumber()
        };
    }

    public class ParcelGoneResponseExamples : IExamplesProvider
    {
        public object GetExamples() => new ProblemDetails
        {
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Perceel werd verwijderd.",
            ProblemInstanceUri = ProblemDetails.GetProblemNumber()
        };
    }
}
