namespace ParcelRegistry.Api.Legacy.Parcel.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Legacy.ParcelSyndication;
    using Query;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class ParcelSyndicationResponse
    {
        public static async Task WriteParcel(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category,
            ParcelSyndicationQueryResult parcel)
        {
            var item = new SyndicationItem
            {
                Id = parcel.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{parcel.ChangeType}-{parcel.Position}",
                Published = parcel.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = parcel.LastChangedOn.ToBelgianDateTimeOffset(),
                Description = BuildDescription(parcel, responseOptions.Value.Naamruimte)
            };

            item.AddLink(
                new SyndicationLink(
                    new Uri($"{responseOptions.Value.Naamruimte}/{parcel.CaPaKey}"),
                    AtomLinkTypes.Related));

            item.AddLink(
                new SyndicationLink(
                    new Uri(string.Format(responseOptions.Value.DetailUrl, parcel.CaPaKey)),
                    AtomLinkTypes.Self));

            item.AddLink(
                new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", parcel.CaPaKey)),
                        AtomLinkTypes.Alternate)
                { MediaType = MediaTypeNames.Application.Xml });

            item.AddLink(
                new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", parcel.CaPaKey)),
                        AtomLinkTypes.Alternate)
                { MediaType = MediaTypeNames.Application.Json });

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    "agentschap Informatie Vlaanderen",
                    "informatie.vlaanderen@vlaanderen.be",
                    AtomContributorTypes.Author));

            await writer.Write(item);
        }

        private static string BuildDescription(ParcelSyndicationQueryResult parcel, string naamruimte)
        {
            if (!parcel.ContainsEvent && !parcel.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();
            if(parcel.ContainsObject)
                content.Object = new ParcelSyndicationContent(
                    parcel.ParcelId,
                    naamruimte,
                    parcel.CaPaKey,
                    parcel.LastChangedOn.ToBelgianDateTimeOffset(),
                    parcel.Status.MapToPerceelStatus(),
                    parcel.AddressIds,
                    parcel.IsComplete,
                    parcel.Organisation,
                    parcel.Reason);

            if (parcel.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(parcel.EventDataAsXml);
                content.Event = doc.DocumentElement;
            }

            return content.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public ParcelSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "Perceel", Namespace = "")]
    public class ParcelSyndicationContent
    {
        /// <summary>
        /// De technische id van het perceel.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid ParcelId { get; set; }

        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public PerceelIdentificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel.
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 3)]
        public PerceelStatus? Status { get; set; }

        /// <summary>
        /// De aan het perceel gelinkte adressen
        /// </summary>
        [DataMember(Name = "AdressenIds", Order = 4)]
        public List<Guid> AddressIds { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 5)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 6)]
        public Provenance Provenance { get; set; }

        public ParcelSyndicationContent(
            Guid parcelId,
            string naamruimte,
            string caPaKey,
            DateTimeOffset version,
            PerceelStatus? status,
            IEnumerable<Guid> addressIds,
            bool isComplete,
            Organisation? organisation,
            string reason)
        {
            ParcelId = parcelId;
            Identificator = new PerceelIdentificator(naamruimte, string.IsNullOrEmpty(caPaKey) ? string.Empty : caPaKey, version);
            Status = status;
            AddressIds = addressIds.ToList();
            IsComplete = isComplete;

            Provenance = new Provenance(organisation, new Reason(reason));
        }
    }

    public class ParcelSyndicationResponseExamples : IExamplesProvider<object>
    {
        private ParcelSyndicationContent ContentExample => new ParcelSyndicationContent(
            Guid.NewGuid(),
            _responseOptions.Naamruimte,
            "13023-1510",
            DateTimeOffset.Now,
            PerceelStatus.Gerealiseerd,
            new List<Guid> { Guid.NewGuid() },
            true,
            Organisation.Agiv,
            Reason.CentralManagementCrab);

        private readonly ResponseOptions _responseOptions;

        public ParcelSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <id>https://basisregisters.vlaanderen/syndication/feed/parcel.atom</id>
  <title>Basisregisters Vlaanderen - Percelenregister</title>
  <subtitle>Basisregisters Vlaanderen stelt u in staat om alles te weten te komen rond: de Belgische gemeenten; de Belgische postcodes; de Vlaamse straatnamen; de Vlaamse adressen; de Vlaamse gebouwen en gebouweenheden; de Vlaamse percelen; de Vlaamse organisaties en organen; de Vlaamse dienstverlening.</subtitle>
  <generator uri=""https://basisregisters.vlaanderen"" version=""2.0.0.0"">Basisregisters Vlaanderen</generator>
  <rights>Copyright (c) 2017-2018, Informatie Vlaanderen</rights>
  <updated>2018-10-05T14:06:53Z</updated>
  <author>
    <name>agentschap Informatie Vlaanderen</name>
    <email>informatie.vlaanderen@vlaanderen.be</email>
  </author>
  <link href=""https://basisregisters.vlaanderen/syndication/feed/parcel.atom"" rel=""self"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/"" rel=""related"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/v1/feeds/percelen.atom?offset=100&limit=100"" rel=""next""/>
  <entry>
    <id>4</id>
    <title>ParcelWasRegistered-4</title>
    <updated>2018-10-04T13:12:17Z</updated>
    <published>2018-10-04T13:12:17Z</published>
    <link href=""{_responseOptions.Naamruimte}/13023-1510"" rel=""related"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/percelen/13023-1510"" rel=""self"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/percelen/13023-1510.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/percelen/13023-1510.json"" rel=""alternate"" type=""application/json"" />
    <author>
      <name>agentschap Informatie Vlaanderen</name>
      <email>informatie.vlaanderen@vlaanderen.be</email>
    </author>
    <category term=""https://data.vlaanderen.be/ns/perceel"" />
    <content><![CDATA[{ContentExample.ToXml()}]]></content>
  </entry>
</feed>";
        }
    }
}
